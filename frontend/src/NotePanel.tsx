import React, { useState, useEffect } from 'react';

interface NotePanelProps {
    isOpen: boolean;
    onClose: () => void;
    filePath: string;
}

const NotePanel: React.FC<NotePanelProps> = ({ isOpen, onClose, filePath }) => {
    const [noteContent, setNoteContent] = useState('');
    const [aiQuery, setAiQuery] = useState('');
    const [aiResponse, setAiResponse] = useState('');
    const [activeSection, setActiveSection] = useState<'notes' | 'ai' | null>('notes');

    useEffect(() => {
        if (filePath) {
            // Fetch existing note
            fetch(`http://localhost:5000/api/notes/${encodeURIComponent(filePath)}`)
                .then(res => {
                    if (res.ok) return res.json();
                    return { content: '' };
                })
                .then(data => setNoteContent(data.content))
                .catch(err => console.error(err));
        } else {
            setNoteContent('');
        }
        setAiResponse('');
        setAiQuery('');
    }, [filePath]);

    const handleSaveNote = () => {
        fetch('http://localhost:5000/api/notes/', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ file_path: filePath, content: noteContent })
        })
            .then(res => {
                if (res.ok) alert('Note saved!');
                else alert('Error saving note');
            })
            .catch(err => console.error(err));
    };

    const handleAskAI = () => {
        if (!aiQuery) return;
        setAiResponse('Thinking...');
        fetch('http://localhost:5000/api/ai/query', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ file_path: filePath, query: aiQuery })
        })
            .then(res => res.json())
            .then(data => setAiResponse(data.response))
            .catch(err => {
                console.error(err);
                setAiResponse('Error querying AI');
            });
    };

    if (!isOpen) return null;

    return (
        <div style={{ height: '100%', display: 'flex', flexDirection: 'column', color: 'var(--text-primary)' }}>
            <div style={{
                padding: '15px',
                borderBottom: '1px solid var(--border-color)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                backgroundColor: 'var(--bg-secondary)'
            }}>
                <h3 style={{ margin: 0, fontSize: '1.1em', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: '250px' }}>
                    {filePath || 'No file selected'}
                </h3>
                <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', fontSize: '1.2em' }}>&times;</button>
            </div>

            <div style={{ flex: 1, overflowY: 'auto' }}>
                {/* Notes Section */}
                <div style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <button
                        onClick={() => setActiveSection(activeSection === 'notes' ? null : 'notes')}
                        style={{
                            width: '100%',
                            padding: '10px 15px',
                            textAlign: 'left',
                            background: 'var(--bg-secondary)',
                            border: 'none',
                            color: 'var(--text-primary)',
                            cursor: 'pointer',
                            fontWeight: '600',
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center'
                        }}
                    >
                        <span>📝 Notes</span>
                        <span>{activeSection === 'notes' ? '▼' : '▶'}</span>
                    </button>
                    {activeSection === 'notes' && (
                        <div style={{ padding: '15px' }}>
                            <textarea
                                style={{
                                    width: '100%',
                                    height: '200px',
                                    backgroundColor: 'var(--bg-primary)',
                                    color: 'var(--text-primary)',
                                    border: '1px solid var(--border-color)',
                                    borderRadius: '4px',
                                    padding: '10px',
                                    resize: 'vertical',
                                    fontFamily: 'inherit'
                                }}
                                value={noteContent}
                                onChange={(e) => setNoteContent(e.target.value)}
                                placeholder="Add your notes here..."
                                disabled={!filePath}
                            />
                            <button
                                onClick={handleSaveNote}
                                disabled={!filePath}
                                style={{
                                    marginTop: '10px',
                                    padding: '8px 16px',
                                    backgroundColor: 'var(--accent)',
                                    color: '#1e1e1e', // Dark text on bright accent
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: filePath ? 'pointer' : 'not-allowed',
                                    fontWeight: '600',
                                    opacity: filePath ? 1 : 0.5
                                }}
                            >
                                Save Note
                            </button>
                        </div>
                    )}
                </div>

                {/* AI Helper Section */}
                <div style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <button
                        onClick={() => setActiveSection(activeSection === 'ai' ? null : 'ai')}
                        style={{
                            width: '100%',
                            padding: '10px 15px',
                            textAlign: 'left',
                            background: 'var(--bg-secondary)',
                            border: 'none',
                            color: 'var(--text-primary)',
                            cursor: 'pointer',
                            fontWeight: '600',
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center'
                        }}
                    >
                        <span>🤖 AI Helper</span>
                        <span>{activeSection === 'ai' ? '▼' : '▶'}</span>
                    </button>
                    {activeSection === 'ai' && (
                        <div style={{ padding: '15px' }}>
                            <div style={{ marginBottom: '10px' }}>
                                <input
                                    type="text"
                                    style={{
                                        width: '100%',
                                        padding: '8px',
                                        backgroundColor: 'var(--bg-primary)',
                                        color: 'var(--text-primary)',
                                        border: '1px solid var(--border-color)',
                                        borderRadius: '4px',
                                        marginBottom: '10px'
                                    }}
                                    value={aiQuery}
                                    onChange={(e) => setAiQuery(e.target.value)}
                                    placeholder="Ask about this code..."
                                    disabled={!filePath}
                                    onKeyDown={(e) => e.key === 'Enter' && handleAskAI()}
                                />
                                <button
                                    onClick={handleAskAI}
                                    disabled={!filePath}
                                    style={{
                                        width: '100%',
                                        padding: '8px 16px',
                                        backgroundColor: 'transparent',
                                        color: 'var(--accent)',
                                        border: '1px solid var(--accent)',
                                        borderRadius: '4px',
                                        cursor: filePath ? 'pointer' : 'not-allowed',
                                        fontWeight: '600',
                                        opacity: filePath ? 1 : 0.5
                                    }}
                                >
                                    Ask AI
                                </button>
                            </div>
                            {aiResponse && (
                                <div style={{
                                    backgroundColor: 'var(--bg-primary)',
                                    padding: '10px',
                                    borderRadius: '4px',
                                    border: '1px solid var(--border-color)',
                                    fontSize: '0.9em',
                                    whiteSpace: 'pre-wrap'
                                }}>
                                    <strong>AI:</strong> {aiResponse}
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default NotePanel;
