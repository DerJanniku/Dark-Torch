import sys
import os
import json

# Add current directory to path so we can import analyzer
sys.path.append(os.getcwd())

from analyzer import analyze_project

def test_analysis():
    # Path to the test project relative to this script
    # Assuming this script is run from backend/ directory
    project_path = os.path.abspath(os.path.join(os.getcwd(), "../projects/test_project"))
    
    print(f"Analyzing project at: {project_path}")
    
    if not os.path.exists(project_path):
        print("Error: Test project path does not exist.")
        return

    result = analyze_project(project_path)
    
    print("Analysis Result:")
    print(json.dumps(result, indent=2))
    
    # Basic assertions
    nodes = result["nodes"]
    edges = result["edges"]
    
    node_ids = [n["id"] for n in nodes]
    expected_nodes = ["main.py", "utils.py"]
    
    for expected in expected_nodes:
        if expected not in node_ids:
            print(f"FAIL: Expected node {expected} not found.")
            return

    expected_edge = {"source": "main.py", "target": "utils.py"}
    if expected_edge not in edges:
        print(f"FAIL: Expected edge {expected_edge} not found.")
        return

    print("SUCCESS: Analysis logic verified.")

if __name__ == "__main__":
    test_analysis()
