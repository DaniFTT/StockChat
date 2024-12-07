import React, { useState, useEffect } from 'react';
import LoginRegister from './LoginRegister';
import Chat from './Chat';
import { Snackbar, Alert } from '@mui/material';

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [userName, setUserName] = useState('');
    const [errors, setErrors] = useState([]);

    const fetchUserName = async () => {
        try {
            const response = await fetch('https://localhost:8081/api/auth/current-user', {
                method: 'GET',
                credentials: 'include',
            });
            if (response.ok) {
                const data = await response.json();
                setUserName(data.FullName);
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
        fetchUserName();
    }, []);

    const handleLoginSuccess = () => {
        fetchUserName();
    };

    return (
        <>
            {!isLoggedIn ? (
                <LoginRegister onLoginSuccess={handleLoginSuccess} setErrors={setErrors} />
            ) : (
                <Chat userName={userName} />
            )}

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
        </>
    );
}

export default App;
