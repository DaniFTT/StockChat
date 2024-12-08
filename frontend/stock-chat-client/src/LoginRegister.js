import React, { useState } from 'react';
import { Box, Tabs, Tab, TextField, Button, Paper, Typography } from '@mui/material';

export default function LoginRegister({ onLoginSuccess, setErrors }) {
    const [tab, setTab] = useState(0);

    const [fullName, setFullName] = useState('');
    const [regEmail, setRegEmail] = useState('');
    const [regPassword, setRegPassword] = useState('');

    const [logEmail, setLogEmail] = useState('');
    const [logPassword, setLogPassword] = useState('');

    const handleTabChange = (event, newValue) => {
        setTab(newValue);
    };

    const handleRegister = async (e) => {
        e.preventDefault();
        setErrors([]);
        try {
            const response = await fetch('https://localhost:8081/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fullName, email: regEmail, password: regPassword }),
                credentials: 'include'
            });
            if (response.status === 200) {
                setTab(0);
            } else if (response.status === 400) {
                const data = await response.json();
                setErrors(data);
            } else {
                setErrors(['Unexpected error']);
            }
        } catch (err) {
            setErrors([err.message]);
        }
    };

    const handleLogin = async (e) => {
        e.preventDefault();
        setErrors([]);
        try {
            const response = await fetch('https://localhost:8081/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: logEmail, password: logPassword }),
                credentials: 'include'
            });

            if (response.status === 200) {
                const userData = await response.json();
                onLoginSuccess(userData);
            } else if (response.status === 400) {
                const data = await response.json();
                setErrors(data);
            } else {
                setErrors(['Unexpected error']);
            }
        } catch (err) {
            setErrors([err.message]);
        }
    };

    return (
        <Box
            sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '100vh',
                bgcolor: '#f5f5f5'
            }}
        >
            <Paper sx={{ p: 4, width: 400 }} elevation={3}>
                <Tabs value={tab} onChange={handleTabChange} variant="fullWidth" sx={{ mb: 2 }}>
                    <Tab label="Login" />
                    <Tab label="Register" />
                </Tabs>

                {tab === 0 && (
                    <Box component="form" onSubmit={handleLogin} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Typography variant="h5" textAlign="center">Login</Typography>
                        <TextField
                            label="Email"
                            type="email"
                            variant="outlined"
                            value={logEmail}
                            onChange={(e) => setLogEmail(e.target.value)}
                            required
                        />
                        <TextField
                            label="Password"
                            type="password"
                            variant="outlined"
                            value={logPassword}
                            onChange={(e) => setLogPassword(e.target.value)}
                            required
                        />
                        <Button type="submit" variant="contained" color="primary">Login</Button>
                    </Box>
                )}
                {tab === 1 && (
                    <Box component="form" onSubmit={handleRegister} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Typography variant="h5" textAlign="center">Register</Typography>
                        <TextField
                            label="Full Name"
                            variant="outlined"
                            value={fullName}
                            onChange={(e) => setFullName(e.target.value)}
                            required
                        />
                        <TextField
                            label="Email"
                            type="email"
                            variant="outlined"
                            value={regEmail}
                            onChange={(e) => setRegEmail(e.target.value)}
                            required
                        />
                        <TextField
                            label="Password"
                            type="password"
                            variant="outlined"
                            value={regPassword}
                            onChange={(e) => setRegPassword(e.target.value)}
                            required
                        />
                        <Button type="submit" variant="contained" color="primary">Register</Button>
                    </Box>
                )}
            </Paper>
        </Box>
    );
}