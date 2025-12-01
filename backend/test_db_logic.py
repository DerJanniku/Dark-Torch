import sys
import os
import sqlite3

# Add current directory to path
sys.path.append(os.getcwd())
import database

# Monkeypatch DB_PATH for testing
database.DB_PATH = "test_darktorch_isolated.db"

from database import init_db, get_db_connection

def test_db_logic():
    print("Initializing DB...")
    init_db()
    
    conn = get_db_connection()
    cursor = conn.cursor()
    
    print("Testing Insert...")
    file_path = "test/file.py"
    content = "Initial content"
    
    # Logic from create_or_update_note
    cursor.execute("SELECT id FROM notes WHERE file_path = ?", (file_path,))
    existing = cursor.fetchone()
    
    if existing:
        print("FAIL: Note should not exist yet")
        return
    else:
        cursor.execute("INSERT INTO notes (file_path, content) VALUES (?, ?)", (file_path, content))
        note_id = cursor.lastrowid
        conn.commit()
        print(f"Inserted note with ID: {note_id}")

    print("Testing Select...")
    cursor.execute("SELECT * FROM notes WHERE file_path = ?", (file_path,))
    note = cursor.fetchone()
    
    if not note or note['content'] != content:
        print("FAIL: Content mismatch on select")
        return
    print(f"Selected: {dict(note)}")

    print("Testing Update...")
    new_content = "Updated content"
    
    # Logic from create_or_update_note (update path)
    cursor.execute("SELECT id FROM notes WHERE file_path = ?", (file_path,))
    existing = cursor.fetchone()
    
    if existing:
        cursor.execute("UPDATE notes SET content = ? WHERE file_path = ?", (new_content, file_path))
        conn.commit()
        print("Updated note")
    else:
        print("FAIL: Note should exist for update")
        return

    cursor.execute("SELECT * FROM notes WHERE file_path = ?", (file_path,))
    updated_note = cursor.fetchone()
    
    if updated_note['content'] != new_content:
        print("FAIL: Content mismatch on update")
        return
    print(f"Updated Note: {dict(updated_note)}")

    conn.close()
    print("SUCCESS: Database logic verified.")

    # Cleanup
    if os.path.exists("test_darktorch_isolated.db"):
        os.remove("test_darktorch_isolated.db")

if __name__ == "__main__":
    test_db_logic()
