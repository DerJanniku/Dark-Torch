import sys
import os
import json

# Add current directory to path
sys.path.append(os.getcwd())

from ai_service import query_code

def test_ai_service():
    print("Testing AI Service...")
    
    file_path = "main.py"
    query = "What does this file do?"
    
    # We need to ensure the test file exists relative to where we run this script
    # The ai_service tries to find it in /app/projects/test_project or ../projects/test_project
    # Let's verify that logic works.
    
    response = query_code(file_path, query)
    
    print("Response received:")
    print(response)
    
    if "AI Analysis for main.py" in response and "What does this file do?" in response:
        print("SUCCESS: AI Service returned expected response.")
    else:
        print("FAIL: Unexpected response format.")
        if "Error" in response:
            print("Make sure the test project files exist in ../projects/test_project relative to backend/")

if __name__ == "__main__":
    test_ai_service()
