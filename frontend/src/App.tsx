import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Dashboard from './Dashboard';
import Visualization from './Visualization';

function App() {
    return (
        <Router>
            <div className="App" style={{ height: '100vh', backgroundColor: '#1e1e1e' }}>
                <Routes>
                    <Route path="/dashboard" element={<Dashboard />} />
                    <Route path="/project/:projectId" element={<Visualization />} />
                    <Route path="/" element={<Navigate to="/dashboard" replace />} />
                </Routes>
            </div>
        </Router>
    );
}

export default App;
