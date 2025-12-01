import os
import git
from urllib.parse import urlparse, urlunparse

def clone_repo(repo_url: str, project_name: str, token: str = None) -> dict:
    """
    Clones a git repository to a local directory.
    
    Args:
        repo_url: The URL of the git repository.
        project_name: The name of the project (used for directory name).
        token: Optional GitHub Personal Access Token for authentication.
        
    Returns:
        dict: A dictionary containing 'success' (bool), 'path' (str), and 'message' (str).
    """
    projects_dir = "/app/projects"
    if not os.path.exists(projects_dir):
        os.makedirs(projects_dir)
        
    target_path = os.path.join(projects_dir, project_name)
    
    if os.path.exists(target_path):
        return {
            "success": False,
            "path": target_path,
            "message": f"Project directory '{project_name}' already exists."
        }
        
    final_url = repo_url
    if token:
        # Inject token into URL: https://oauth2:TOKEN@github.com/...
        parsed = urlparse(repo_url)
        if parsed.scheme in ('http', 'https'):
            netloc = f"oauth2:{token}@{parsed.netloc}"
            final_url = urlunparse((parsed.scheme, netloc, parsed.path, parsed.params, parsed.query, parsed.fragment))
            
    try:
        git.Repo.clone_from(final_url, target_path)
        return {
            "success": True,
            "path": target_path,
            "message": "Repository cloned successfully."
        }
    except git.GitCommandError as e:
        return {
            "success": False,
            "path": None,
            "message": f"Git clone failed: {str(e)}"
        }
    except Exception as e:
        return {
            "success": False,
            "path": None,
            "message": f"An unexpected error occurred: {str(e)}"
        }
