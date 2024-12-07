import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginRegister from './LoginRegister';
import Chat from './Chat';
import { Snackbar, Alert } from '@mui/material';

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [user, setUser] = useState({});
    const [errors, setErrors] = useState([]);

    const fetchUser = async () => {
        try {
            const response = await fetch('https://localhost:8081/api/auth/current-user', {
                method: 'GET',
                credentials: 'include',
            });
            if (response.ok) {
                const data = await response.json();
                setUser(data);
                setIsLoggedIn(true);
            } else {
                setIsLoggedIn(false);
            }
        } catch (err) {
            console.error('Failed to fetch user info:', err);
            setIsLoggedIn(false);
        }
    };

    useEffect(() => {
        fetchUser();
    }, []);

    const handleLoginSuccess = (user) => {
        setUser(user);
        setIsLoggedIn(true);
    };


    const handleLogout = () => {
        setIsLoggedIn(false);
    };

    return (
        <Router>
            <Routes>
                {!isLoggedIn ? (
                    <Route
                        path="/"
                        element={<LoginRegister onLoginSuccess={handleLoginSuccess} setErrors={setErrors} />}
                    />
                ) : (
                        <Route path="/chat" element={<Chat user={user} onLogout={handleLogout} />} />
                )}

                <Route path="*" element={isLoggedIn ? <Navigate to="/chat" /> : <Navigate to="/" />} />
            </Routes>

            <Snackbar
                open={errors.length > 0}
                autoHideDuration={6000}
                onClose={() => setErrors([])}
                anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
            >
                <Alert onClose={() => setErrors([])} severity="error" sx={{ width: '100%' }}>
                    {errors.map((err, i) => (
                        <div key={i}>{err}</div>
                    ))}
                </Alert>
            </Snackbar>
        </Router>
    );
}

export default App;
