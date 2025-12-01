import httpx
import time

BASE_URL = "http://localhost:5000/api"

def test_github_import():
    print("Testing GitHub Import...")
    
    # Use a small, public repository for testing
    repo_url = "https://github.com/octocat/Hello-World.git"
    project_name = "test_github_import_project"
    
    payload = {
        "repo_url": repo_url,
        "project_name": project_name,
        "token": None
    }
    
    try:
        with httpx.Client() as client:
            response = client.post(f"{BASE_URL}/projects/import/github", json=payload)
            
            if response.status_code == 200:
                print("SUCCESS: Project imported successfully.")
                data = response.json()
                print(f"Project ID: {data['id']}")
                print(f"Local Path: {data['local_path']}")
                
                # Verify it's in the list
                list_response = client.get(f"{BASE_URL}/projects/")
                projects = list_response.json()
                found = any(p['name'] == project_name for p in projects)
                if found:
                    print("SUCCESS: Project found in project list.")
                else:
                    print("FAILURE: Project not found in list.")
                    
                # Cleanup
                print("Cleaning up...")
                client.delete(f"{BASE_URL}/projects/{data['id']}")
                print("Cleanup complete.")
                
            elif response.status_code == 400 and "already exists" in response.text:
                 print("WARNING: Project already exists. Skipping import test.")
            else:
                print(f"FAILURE: Import failed with status {response.status_code}")
                print(response.text)
            
    except Exception as e:
        print(f"ERROR: {e}")

if __name__ == "__main__":
    # Wait for service to be ready
    print("Waiting for backend...")
    time.sleep(2)
    test_github_import()
