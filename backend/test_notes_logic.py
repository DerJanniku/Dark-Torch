import sys
import os
import sqlite3
from pydantic import BaseModel

# Mocking the environment for local test
# We need to override DB_PATH in database module or ensure the directory exists
# Since we can't easily monkeypatch the imported module variable from here without reloading,
# we will just ensure the directory /app/data exists or change the path in database.py temporarily?
# No, let's just use a local path for testing by modifying the imported variable if possible, 
# or just creating the directory locally.
# Since I am on Windows, /app/data is not valid. I should probably make database.py use an env var or default.

# For this test, I will modify database.py to use a local file if not in docker, 
# or I will just monkeypatch it here.

sys.path.append(os.getcwd())
import database

# Monkeypatch DB_PATH for testing
database.DB_PATH = "test_darktorch.db"

from main import create_or_update_note, get_note, get_all_notes, NoteIn, on_startup

def test_notes_logic():
    print("Initializing DB...")
    on_startup()
    
    print("Testing Create Note...")
    note_in = NoteIn(file_path="test/file.py", content="This is a test note.")
    created = create_or_update_note(note_in)
    print(f"Created: {created}")
    
    if created["content"] != "This is a test note.":
        print("FAIL: Content mismatch on create")
        return

    print("Testing Get Note...")
    fetched = get_note("test/file.py")
    print(f"Fetched: {fetched}")
    
    if fetched["content"] != "This is a test note.":
        print("FAIL: Content mismatch on get")
        return

    print("Testing Update Note...")
    update_in = NoteIn(file_path="test/file.py", content="Updated content.")
    updated = create_or_update_note(update_in)
    print(f"Updated: {updated}")
    
    if updated["content"] != "Updated content.":
        print("FAIL: Content mismatch on update")
        return
        
    print("Testing Get All Notes...")
    all_notes = get_all_notes()
    print(f"All Notes: {all_notes}")
    
    if len(all_notes) != 1:
        print("FAIL: Expected 1 note")
        return

    print("SUCCESS: Notes API logic verified.")
    
    # Cleanup
    if os.path.exists("test_darktorch.db"):
        os.remove("test_darktorch.db")

if __name__ == "__main__":
    test_notes_logic()
