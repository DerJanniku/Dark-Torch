# Deployment Guide

Follow these steps to deploy and run DarkTorch on your local machine.

## Prerequisites

- **Docker**: Ensure Docker is installed and running.
- **Docker Compose**: Included with Docker Desktop.

## Installation & Running

1. **Navigate to the project root**:

    ```bash
    cd /path/to/Dark-Torch
    ```

2. **Start the application**:
    Run the following command to build the images and start the containers:

    ```bash
    docker-compose up --build
    ```

    *Add `-d` to run in detached mode (background).*

3. **Access the Application**:
    - **Frontend**: [http://localhost:80](http://localhost:80)
    - **Backend API Docs**: [http://localhost:5000/docs](http://localhost:5000/docs)

## Analyzing Your Code

To analyze your own Python project:

1. **Open the Dashboard**: Navigate to `http://localhost:80`.
2. **Import a Project**:
    - Click "Import via GitHub".
    - Enter the Repository URL (e.g., `https://github.com/user/repo.git`).
    - Enter a Project Name.
    - (Optional) Enter a Personal Access Token for private repositories.
3. **Analyze**: Click on the project card to view the dependency graph and start analyzing.
4. **Sync**: Click the "Sync" button on the project card to pull the latest changes from GitHub.

## Troubleshooting

- **Port Conflicts**: If ports 80 or 5000 are in use, modify the `ports` mapping in `docker-compose.yml`.
- **Database**: The SQLite database is stored in `./data/darktorch.db`. This folder is mounted to persist data across restarts.
