import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import ImportModal from './components/ImportModal';

interface Project {
    id: number;
    name: string;
    local_path: string;
    github_url: string | null;
    last_analysis: string | null;
}

const Dashboard: React.FC = () => {
    const [projects, setProjects] = useState<Project[]>([]);
    const [loading, setLoading] = useState(true);
    const [isImportModalOpen, setIsImportModalOpen] = useState(false);

    const fetchProjects = () => {
        setLoading(true);
        fetch('http://localhost:5000/api/projects/')
            .then(res => res.json())
            .then(data => {
                setProjects(data);
                setLoading(false);
            })
            .catch(err => {
                console.error('Error fetching projects:', err);
                setLoading(false);
            });
    };

    useEffect(() => {
        fetchProjects();
    }, []);

    const handleSync = (e: React.MouseEvent, projectId: number) => {
        e.preventDefault(); // Prevent navigation
        e.stopPropagation();

        fetch(`http://localhost:5000/api/projects/${projectId}/sync`, { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                if (data.status === 'success') {
                    alert(data.message);
                    fetchProjects(); // Refresh list to update timestamps if needed
                } else {
                    alert('Sync failed: ' + data.detail);
                }
            })
            .catch(err => {
                console.error('Error syncing project:', err);
                alert('Error syncing project');
            });
    };

    const handleDelete = (e: React.MouseEvent, projectId: number, projectName: string) => {
        e.preventDefault();
        e.stopPropagation();

        if (window.confirm(`Are you sure you want to delete the project "${projectName}"? This will delete the database entry and the local files.`)) {
            fetch(`http://localhost:5000/api/projects/${projectId}`, { method: 'DELETE' })
                .then(res => res.json())
                .then(data => {
                    if (data.status === 'deleted') {
                        fetchProjects();
                    } else {
                        alert('Delete failed');
                    }
                })
                .catch(err => {
                    console.error('Error deleting project:', err);
                    alert('Error deleting project');
                });
        }
    };

    return (
        <div style={{ padding: '40px', color: 'var(--text-primary)', fontFamily: 'Segoe UI, sans-serif', maxWidth: '1200px', margin: '0 auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '40px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
                    <img src="/darktorch_logo.png" alt="DarkTorch Logo" style={{ height: '40px' }} />
                    <h1 style={{ margin: 0, fontSize: '2em', fontWeight: '600' }}>DarkTorch</h1>
                </div>
                <button style={{
                    padding: '12px 24px',
                    backgroundColor: 'transparent',
                    color: 'var(--accent)',
                    border: '1px solid var(--accent)',
                    borderRadius: '4px',
                    cursor: 'pointer',
                    fontSize: '16px',
                    fontWeight: '500',
                    transition: 'all 0.2s ease'
                }}
                    onClick={() => setIsImportModalOpen(true)}
                    onMouseOver={(e) => e.currentTarget.style.backgroundColor = 'rgba(78, 201, 176, 0.1)'}
                    onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                >
                    Import via GitHub
                </button>
            </div>

            {loading ? (
                <div style={{ color: 'var(--text-secondary)' }}>Loading projects...</div>
            ) : (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '20px' }}>
                    {projects.map(project => (
                        <Link to={`/project/${project.id}`} key={project.id} style={{ textDecoration: 'none', color: 'inherit' }}>
                            <div style={{
                                backgroundColor: 'var(--bg-secondary)',
                                padding: '25px',
                                borderRadius: '8px',
                                border: '1px solid transparent',
                                transition: 'all 0.2s ease',
                                cursor: 'pointer',
                                height: '100%',
                                display: 'flex',
                                flexDirection: 'column',
                                justifyContent: 'space-between'
                            }}
                                onMouseOver={(e) => {
                                    e.currentTarget.style.transform = 'translateY(-2px)';
                                    e.currentTarget.style.borderColor = 'var(--accent)';
                                }}
                                onMouseOut={(e) => {
                                    e.currentTarget.style.transform = 'none';
                                    e.currentTarget.style.borderColor = 'transparent';
                                }}
                            >
                                <div>
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <h2 style={{ margin: '0 0 10px 0', fontSize: '1.4em', color: 'var(--text-primary)' }}>{project.name}</h2>
                                        <div style={{ display: 'flex', gap: '5px' }}>
                                            {project.github_url && (
                                                <button
                                                    onClick={(e) => handleSync(e, project.id)}
                                                    style={{
                                                        padding: '4px 8px',
                                                        fontSize: '0.8em',
                                                        backgroundColor: 'transparent',
                                                        border: '1px solid var(--text-secondary)',
                                                        color: 'var(--text-secondary)',
                                                        borderRadius: '4px',
                                                        cursor: 'pointer'
                                                    }}
                                                    title="Sync with GitHub"
                                                >
                                                    ↻ Sync
                                                </button>
                                            )}
                                            <button
                                                onClick={(e) => handleDelete(e, project.id, project.name)}
                                                style={{
                                                    padding: '4px 8px',
                                                    fontSize: '0.8em',
                                                    backgroundColor: 'transparent',
                                                    border: '1px solid #f48771',
                                                    color: '#f48771',
                                                    borderRadius: '4px',
                                                    cursor: 'pointer'
                                                }}
                                                title="Delete Project"
                                            >
                                                🗑️
                                            </button>
                                        </div>
                                    </div>
                                    <p style={{ margin: '0 0 15px 0', color: 'var(--text-secondary)', fontSize: '0.9em', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                        {project.local_path}
                                    </p>
                                </div>
                                <div style={{ fontSize: '0.85em', color: 'var(--text-secondary)', borderTop: '1px solid var(--border-color)', paddingTop: '15px' }}>
                                    Last Analysis: <span style={{ color: 'var(--text-primary)' }}>{project.last_analysis ? new Date(project.last_analysis).toLocaleString() : 'Never'}</span>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            )}

            <ImportModal
                isOpen={isImportModalOpen}
                onClose={() => setIsImportModalOpen(false)}
                onImportSuccess={fetchProjects}
            />
        </div>
    );
};

export default Dashboard;
