import { useState, useEffect, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import cytoscape from 'cytoscape'
import NotePanel from './NotePanel'

interface Node {
    id: string
    name: string
    type: string
}

interface Edge {
    source: string
    target: string
}

interface GraphData {
    nodes: Node[]
    edges: Edge[]
}

function Visualization() {
    const { projectId } = useParams<{ projectId: string }>();
    const cyRef = useRef<HTMLDivElement>(null)
    const cyInstance = useRef<cytoscape.Core | null>(null)
    const [graphData, setGraphData] = useState<GraphData | null>(null)
    const [selectedFile, setSelectedFile] = useState<string | null>(null)
    const [isPanelOpen, setIsPanelOpen] = useState(true) // Default open sidebar

    useEffect(() => {
        const fetchGraphData = async () => {
            if (!projectId) return;
            try {
                const response = await fetch('http://localhost:5000/api/analyze', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ project_id: parseInt(projectId) }),
                })
                const data = await response.json()
                setGraphData(data)
            } catch (error) {
                console.error('Error fetching graph data:', error)
            }
        }

        fetchGraphData()
    }, [projectId])

    useEffect(() => {
        if (graphData && cyRef.current) {
            const elements = [
                ...graphData.nodes.map((node) => ({
                    data: { id: node.id, label: node.name, type: node.type },
                })),
                ...graphData.edges.map((edge) => ({
                    data: { source: edge.source, target: edge.target },
                })),
            ]

            const cy = cytoscape({
                container: cyRef.current,
                elements: elements,
                style: [
                    {
                        selector: 'node',
                        style: {
                            'background-color': '#252526',
                            'label': 'data(label)',
                            'color': '#D4D4D4',
                            'text-valign': 'center',
                            'text-halign': 'center',
                            'width': 'label',
                            'height': 'label',
                            'padding': '12px',
                            'shape': 'round-rectangle',
                            'border-width': 2,
                            'border-color': '#B5CEA8', // Default green
                            'font-family': 'Segoe UI, sans-serif',
                            'font-size': '14px'
                        },
                    },
                    {
                        selector: 'node[type="important"]', // Example conditional styling
                        style: {
                            'border-color': '#DCDCAA', // Orange
                        }
                    },
                    {
                        selector: 'edge',
                        style: {
                            'width': 2,
                            'line-color': '#4EC9B0',
                            'target-arrow-color': '#4EC9B0',
                            'target-arrow-shape': 'triangle',
                            'curve-style': 'bezier',
                            'arrow-scale': 1.2
                        },
                    },
                    {
                        selector: ':selected',
                        style: {
                            'background-color': '#37373d',
                            'border-color': '#4EC9B0',
                            'border-width': 3,
                            'color': '#fff'
                        }
                    }
                ],
                layout: {
                    name: 'cose',
                    animate: false,
                    padding: 50,
                    nodeDimensionsIncludeLabels: true
                },
            })

            cyInstance.current = cy

            cy.on('tap', 'node', (evt) => {
                const node = evt.target
                setSelectedFile(node.id())
                if (!isPanelOpen) setIsPanelOpen(true)
            })

            cy.on('tap', (evt) => {
                if (evt.target === cy) {
                    setSelectedFile(null)
                }
            })

            // Hover effects
            cy.on('mouseover', 'node', () => {
                document.body.style.cursor = 'pointer';
            });
            cy.on('mouseout', 'node', () => {
                document.body.style.cursor = 'default';
            });
        }
    }, [graphData])

    return (
        <div style={{ width: '100%', height: '100vh', display: 'flex', flexDirection: 'column', backgroundColor: 'var(--bg-primary)' }}>
            {/* Top Bar */}
            <div style={{
                padding: '10px 20px',
                backgroundColor: 'var(--bg-secondary)',
                borderBottom: '1px solid var(--border-color)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between'
            }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
                    <Link to="/dashboard" style={{ color: 'var(--text-primary)', textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '5px' }}>
                        <span style={{ color: 'var(--accent)', fontSize: '1.2em' }}>&larr;</span> Dashboard
                    </Link>
                    <div style={{ width: '1px', height: '20px', backgroundColor: 'var(--border-color)' }}></div>
                    <span style={{ color: 'var(--text-secondary)' }}>Project ID: <span style={{ color: 'var(--text-primary)' }}>{projectId}</span></span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <img src="/darktorch_logo.png" alt="Logo" style={{ height: '24px', opacity: 0.8 }} />
                </div>
            </div>

            {/* Main Content */}
            <div style={{ flex: 1, display: 'flex', overflow: 'hidden' }}>
                {/* Canvas Area */}
                <div style={{ flex: 1, position: 'relative' }}>
                    {!graphData && <div style={{ padding: '40px', color: 'var(--text-secondary)', textAlign: 'center' }}>Loading graph data...</div>}
                    <div ref={cyRef} style={{ width: '100%', height: '100%' }} />
                </div>

                {/* Sidebar */}
                <div style={{
                    width: '350px',
                    backgroundColor: 'var(--bg-secondary)',
                    borderLeft: '1px solid var(--border-color)',
                    display: 'flex',
                    flexDirection: 'column'
                }}>
                    <NotePanel
                        isOpen={isPanelOpen}
                        onClose={() => setIsPanelOpen(false)}
                        filePath={selectedFile || ''}
                    />
                </div>
            </div>
        </div>
    )
}

export default Visualization
