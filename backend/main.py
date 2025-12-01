from fastapi import FastAPI, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from analyzer import analyze_project
import os
import sqlite3
from database import init_db, get_db_connection
from typing import List, Optional
from git_service import clone_repo

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allow all origins for development
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- Models ---
class AnalyzeRequest(BaseModel):
    project_id: int

class ProjectIn(BaseModel):
    name: str
    local_path: str
    github_url: Optional[str] = None

class ProjectOut(BaseModel):
    id: int
    name: str
    local_path: str
    github_url: Optional[str] = None
    last_analysis: Optional[str] = None

class NoteIn(BaseModel):
    file_path: str
    content: str

class NoteOut(BaseModel):
    id: int
    file_path: str
    content: str

# --- Startup ---
@app.on_event("startup")
def on_startup():
    try:
        init_db()
    except Exception as e:
        print(f"Error initializing database: {e}")

# --- Endpoints ---
@app.get("/api/health")
def health_check():
    return {"status": "ok", "service": "backend"}

@app.post("/api/projects/add", response_model=ProjectOut)
def add_project(project: ProjectIn):
    conn = get_db_connection()
    cursor = conn.cursor()
    try:
        cursor.execute(
            "INSERT INTO projects (name, local_path, github_url) VALUES (?, ?, ?)",
            (project.name, project.local_path, project.github_url)
        )
        project_id = cursor.lastrowid
        conn.commit()
        return {
            "id": project_id,
            "name": project.name,
            "local_path": project.local_path,
            "github_url": project.github_url,
            "last_analysis": None
        }
    except sqlite3.IntegrityError:
        raise HTTPException(status_code=400, detail="Project with this path already exists")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@app.get("/api/projects/", response_model=List[ProjectOut])
def list_projects():
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects")
    projects = cursor.fetchall()
    conn.close()
    return [dict(p) for p in projects]

@app.get("/api/projects/{project_id}", response_model=ProjectOut)
def get_project(project_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ?", (project_id,))
    project = cursor.fetchone()
    conn.close()
    if project is None:
        raise HTTPException(status_code=404, detail="Project not found")
    return dict(project)

@app.delete("/api/projects/{project_id}")
def delete_project(project_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    
    # Get local_path before deleting
    cursor.execute("SELECT local_path FROM projects WHERE id = ?", (project_id,))
    project = cursor.fetchone()
    
    if not project:
        conn.close()
        raise HTTPException(status_code=404, detail="Project not found")
        
    local_path = project['local_path']
    
    cursor.execute("DELETE FROM projects WHERE id = ?", (project_id,))
    conn.commit()
    conn.close()
    
    # Delete from filesystem
    if os.path.exists(local_path):
        try:
            import shutil
            shutil.rmtree(local_path)
        except Exception as e:
            print(f"Error deleting directory {local_path}: {e}")
            
    return {"status": "deleted", "id": project_id}

@app.post("/api/analyze")
def analyze_endpoint(request: AnalyzeRequest):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT local_path FROM projects WHERE id = ?", (request.project_id,))
    project = cursor.fetchone()
    conn.close()

    if project is None:
        raise HTTPException(status_code=404, detail="Project not found")
    
    path = project['local_path']

    if not os.path.exists(path):
        raise HTTPException(status_code=404, detail="Project path not found on server")
    
    try:
        result = analyze_project(path)
        # Update last_analysis timestamp
        import datetime
        timestamp = datetime.datetime.now().isoformat()
        conn = get_db_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE projects SET last_analysis = ? WHERE id = ?", (timestamp, request.project_id))
        conn.commit()
        conn.close()
        
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/notes/", response_model=NoteOut)
def create_or_update_note(note: NoteIn):
    conn = get_db_connection()
    cursor = conn.cursor()
    
    try:
        # Check if note exists
        cursor.execute("SELECT id FROM notes WHERE file_path = ?", (note.file_path,))
        existing = cursor.fetchone()
        
        if existing:
            cursor.execute("UPDATE notes SET content = ? WHERE file_path = ?", (note.content, note.file_path))
            note_id = existing['id']
        else:
            cursor.execute("INSERT INTO notes (file_path, content) VALUES (?, ?)", (note.file_path, note.content))
            note_id = cursor.lastrowid
            
        conn.commit()
        return {"id": note_id, "file_path": note.file_path, "content": note.content}
    except Exception as e:
        conn.rollback()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@app.get("/api/notes/{file_path:path}", response_model=NoteOut)
def get_note(file_path: str):
    conn = get_db_connection()
    cursor = conn.cursor()
    # Decode path if needed, but FastAPI handles path params well.
    # Note: file_path in URL might need to be double encoded if it contains slashes, 
    # but here we use :path converter which captures everything.
    
    cursor.execute("SELECT * FROM notes WHERE file_path = ?", (file_path,))
    note = cursor.fetchone()
    conn.close()
    
    if note is None:
        raise HTTPException(status_code=404, detail="Note not found")
        
    return dict(note)

@app.get("/api/notes/", response_model=List[NoteOut])
def get_all_notes():
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM notes")
    notes = cursor.fetchall()
    conn.close()
    
    return [dict(note) for note in notes]

# --- AI Endpoints ---
from ai_service import query_code

class AIQueryIn(BaseModel):
    file_path: str
    query: str

@app.post("/api/ai/query")
def ai_query_endpoint(query_in: AIQueryIn):
    response = query_code(query_in.file_path, query_in.query)
    return {"file": query_in.file_path, "response": response}

class GitHubImportIn(BaseModel):
    repo_url: str
    project_name: str
    token: Optional[str] = None

@app.post("/api/projects/import/github")
def import_github_project(import_data: GitHubImportIn):
    # 1. Clone the repo
    clone_result = clone_repo(import_data.repo_url, import_data.project_name, import_data.token)
    
    if not clone_result["success"]:
        raise HTTPException(status_code=400, detail=clone_result["message"])
        
    local_path = clone_result["path"]
    
    # 2. Add to database
    conn = get_db_connection()
    cursor = conn.cursor()
    
    try:
        cursor.execute(
            "INSERT INTO projects (name, local_path, github_url, last_analysis) VALUES (?, ?, ?, ?)",
            (import_data.project_name, local_path, import_data.repo_url, None)
        )
        conn.commit()
        project_id = cursor.lastrowid
        return {
            "id": project_id,
            "name": import_data.project_name,
            "local_path": local_path,
            "github_url": import_data.repo_url,
            "last_analysis": None
        }
    except sqlite3.IntegrityError:
        conn.close()
        raise HTTPException(status_code=400, detail="Project with this path already exists.")
    except Exception as e:
        conn.close()
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@app.post("/api/projects/{project_id}/sync")
def sync_project(project_id: int):
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM projects WHERE id = ?", (project_id,))
    project = cursor.fetchone()
    conn.close()
    
    if not project:
        raise HTTPException(status_code=404, detail="Project not found")
        
    if not project['github_url']:
        raise HTTPException(status_code=400, detail="Project is not linked to a GitHub repository")
        
    # Perform git pull
    try:
        repo = git.Repo(project['local_path'])
        origin = repo.remotes.origin
        pull_info = origin.pull()
        
        if len(pull_info) == 0:
             return {"status": "success", "message": "Already up to date."}
             
        commit_info = pull_info[0]
        if commit_info.flags & commit_info.HEAD_UPTODATE:
             return {"status": "success", "message": f"Project '{project['name']}' is already up to date."}
        
        return {"status": "success", "message": f"Project '{project['name']}' updated successfully."}
        
    except git.GitCommandError as e:
        print(f"Git pull failed for project {project['name']}: {e}")
        raise HTTPException(status_code=500, detail=f"Git pull failed: {str(e)}")
    except Exception as e:
        print(f"Unexpected error during sync for project {project['name']}: {e}")
        raise HTTPException(status_code=500, detail=f"An error occurred during sync: {str(e)}")
