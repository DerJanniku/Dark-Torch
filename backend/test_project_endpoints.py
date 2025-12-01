import sys
import os
import sqlite3
from fastapi.testclient import TestClient

# Add current directory to path
sys.path.append(os.getcwd())
import database

# Monkeypatch DB_PATH for testing
database.DB_PATH = "test_projects.db"

from main import app
from database import init_db
import main

client = TestClient(app)

def test_project_crud():
    # Init DB
    if os.path.exists(database.DB_PATH):
        try:
            os.remove(database.DB_PATH)
        except PermissionError:
            pass
    init_db()

    # 1. Add Project
    print("Testing Add Project...")
    response = client.post("/api/projects/add", json={
        "name": "Test Project",
        "local_path": "non_existent_path",
        "github_url": "http://github.com/test/test"
    })
    if response.status_code != 200:
        print(f"Add Project Failed: {response.text}")
    assert response.status_code == 200
    project = response.json()
    assert project["name"] == "Test Project"
    assert project["id"] is not None
    project_id = project["id"]
    print(f"Added project ID: {project_id}")

    # 2. List Projects
    print("Testing List Projects...")
    response = client.get("/api/projects/")
    assert response.status_code == 200
    projects = response.json()
    assert len(projects) == 1
    assert projects[0]["id"] == project_id

    # 3. Get Project
    print("Testing Get Project...")
    response = client.get(f"/api/projects/{project_id}")
    assert response.status_code == 200
    assert response.json()["name"] == "Test Project"

    # 4. Analyze
    # Create a dummy directory for testing analysis
    test_dir = "test_project_dir"
    if not os.path.exists(test_dir):
        os.makedirs(test_dir)
    
    real_path = os.path.abspath(test_dir)
    
    # Add a project with real path
    response = client.post("/api/projects/add", json={
        "name": "Real Project",
        "local_path": real_path
    })
    real_project_id = response.json()["id"]

    print("Testing Analyze...")
    # Mock analyze_project
    original_analyze = main.analyze_project
    main.analyze_project = lambda path: {"status": "analyzed", "path": path}
    
    try:
        response = client.post("/api/analyze", json={"project_id": real_project_id})
        if response.status_code != 200:
            print(f"Analyze Failed: {response.text}")
        assert response.status_code == 200
        assert response.json()["status"] == "analyzed"
        
        # Verify last_analysis updated
        response = client.get(f"/api/projects/{real_project_id}")
        assert response.json()["last_analysis"] is not None
        print("Analyze verification successful")
        
    finally:
        main.analyze_project = original_analyze

    # 5. Delete Project
    print("Testing Delete Project...")
    response = client.delete(f"/api/projects/{project_id}")
    assert response.status_code == 200
    
    response = client.get(f"/api/projects/{project_id}")
    assert response.status_code == 404

    # Cleanup
    if os.path.exists(database.DB_PATH):
        try:
            os.remove(database.DB_PATH)
        except PermissionError:
            print("Warning: Could not remove test db file")
    
    if os.path.exists(test_dir):
        os.rmdir(test_dir)
    
    print("SUCCESS: Project CRUD verified.")

if __name__ == "__main__":
    test_project_crud()
