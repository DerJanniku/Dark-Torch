import os
import ast
from typing import List, Dict, Any

def analyze_project(root_path: str) -> Dict[str, List[Dict[str, Any]]]:
    nodes = []
    edges = []
    
    # 1. Scan for all Python files (Nodes)
    # Map relative path to absolute path for easy lookup later if needed, 
    # but primarily we need a set of valid relative paths to validate imports.
    valid_files = set()
    
    for dirpath, _, filenames in os.walk(root_path):
        for filename in filenames:
            if filename.endswith(".py"):
                full_path = os.path.join(dirpath, filename)
                rel_path = os.path.relpath(full_path, root_path).replace("\\", "/")
                
                valid_files.add(rel_path)
                nodes.append({
                    "id": rel_path,
                    "name": filename,
                    "type": "python"
                })

    # 2. Parse files for imports (Edges)
    for node in nodes:
        source_rel_path = node["id"]
        source_full_path = os.path.join(root_path, source_rel_path)
        
        try:
            with open(source_full_path, "r", encoding="utf-8") as f:
                tree = ast.parse(f.read(), filename=source_full_path)
                
            for node_ast in ast.walk(tree):
                imported_names = []
                
                if isinstance(node_ast, ast.Import):
                    for alias in node_ast.names:
                        imported_names.append(alias.name)
                elif isinstance(node_ast, ast.ImportFrom):
                    if node_ast.module:
                        imported_names.append(node_ast.module)
                        
                for imported_name in imported_names:
                    # Convert dotted name to path
                    # e.g. "utils" -> "utils.py"
                    # e.g. "my_pkg.utils" -> "my_pkg/utils.py"
                    
                    # Simple resolution strategy:
                    # 1. Check if it matches a file directly (e.g. utils -> utils.py)
                    potential_path_1 = imported_name.replace(".", "/") + ".py"
                    
                    # 2. Check if it matches a package init (e.g. my_pkg -> my_pkg/__init__.py)
                    potential_path_2 = imported_name.replace(".", "/") + "/__init__.py"
                    
                    target = None
                    if potential_path_1 in valid_files:
                        target = potential_path_1
                    elif potential_path_2 in valid_files:
                        target = potential_path_2
                        
                    # Also consider relative imports if we were being robust, but for now 
                    # we'll stick to the simple requirement: "assume all found imports that match another file"
                    
                    if target:
                        edges.append({
                            "source": source_rel_path,
                            "target": target
                        })
                        
        except Exception as e:
            print(f"Error analyzing file {source_rel_path}: {e}")
            continue

    return {"nodes": nodes, "edges": edges}
