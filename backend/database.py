import sqlite3
import os

DB_PATH = "/app/data/darktorch.db"

def get_db_connection():
    # Ensure directory exists if running locally for testing outside docker
    # In docker, /app/data is a volume.
    # If running locally, we might need to adjust or ensure the dir exists.
    db_dir = os.path.dirname(DB_PATH)
    if not os.path.exists(db_dir) and db_dir != "":
        try:
            os.makedirs(db_dir)
        except OSError:
            pass # Might be permission issue or already exists
            
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row
    return conn

def init_db():
    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.executescript('''
        CREATE TABLE IF NOT EXISTS notes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            file_path TEXT NOT NULL UNIQUE,
            content TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS projects (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            local_path TEXT NOT NULL UNIQUE,
            github_url TEXT,
            last_analysis TEXT
        );
    ''')
    conn.commit()
    conn.close()
