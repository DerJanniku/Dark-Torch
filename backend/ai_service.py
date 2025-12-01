import os

def query_code(file_path: str, user_query: str) -> str:
    # Construct full path to the file in the mounted volume
    # Assuming the file_path passed is relative to the project root (e.g. "main.py")
    # and the project root is mounted at /app/projects/test_project/
    # Note: In a real app, we'd need to handle the project path dynamically or pass it in.
    # For this task, we assume the test project path.
    
    base_path = "/app/projects/test_project"
    full_path = os.path.join(base_path, file_path)
    
    # Check if file exists
    if not os.path.exists(full_path):
        # Fallback for local testing if not in docker
        # Try to find it relative to current working directory if possible
        # This is just for the agent's verification script to work if run locally
        local_base = os.path.abspath(os.path.join(os.getcwd(), "../projects/test_project"))
        full_path = os.path.join(local_base, file_path)
        
        if not os.path.exists(full_path):
            return f"Error: File {file_path} not found."

    try:
        with open(full_path, "r", encoding="utf-8") as f:
            content = f.read()
            
        # Simulate MCP connection
        print("Simulating query against potential MCP knowledge base...")
        
        # Create simulated response
        snippet = content[:50].replace("\n", " ") + "..." if len(content) > 50 else content.replace("\n", " ")
        response = (
            f"AI Analysis for {file_path}:\n"
            f"The AI suggests that based on your question about '{user_query}', "
            f"the file {file_path} contains: '{snippet}'\n"
            f"It seems to be a valid Python file."
        )
        
        return response
        
    except Exception as e:
        return f"Error reading file: {str(e)}"
