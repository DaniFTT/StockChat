import React, { useState, useEffect } from 'react';
import { Box, Button, List, ListItem, ListItemText, Typography, Divider } from '@mui/material';
import { useNavigate } from 'react-router-dom';

const Chat = ({ userName }) => {
    const [chats, setChats] = useState([]);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchChats = async () => {
            try {
                const response = await fetch('https://localhost:8081/api/chats', {
                    method: 'GET',
                    credentials: 'include',
                });

                if (response.ok) {
                    const data = await response.json();
                    setChats(data);
                } else {
                    console.error('Failed to fetch chats');
                }
            } catch (err) {
                console.error(err);
            }
        };

        fetchChats();
    }, []);

    const handleLogout = async () => {
        try {
            const response = await fetch('https://localhost:8081/api/auth/logout', {
                method: 'POST',
                credentials: 'include',
            });

            if (response.status === 204) {
                navigate('/login');
            } else {
                console.error('Logout failed');
            }
        } catch (err) {
            console.error(err);
        }
    };

    return (
        <Box sx={{ display: 'flex', height: '100vh' }}>
            <Box
                sx={{
                    width: 300,
                    bgcolor: '#f5f5f5',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    p: 2,
                    boxShadow: '2px 0 5px rgba(0,0,0,0.1)',
                }}
            >
                <Typography variant="h6" sx={{ mb: 2 }}>
                    Olá, {userName}
                </Typography>

                <Button variant="contained" color="error" onClick={handleLogout} sx={{ mb: 4 }}>
                    Logout
                </Button>

                <Divider sx={{ width: '100%', mb: 2 }} />

                <Typography variant="subtitle1" sx={{ mb: 1 }}>
                    Chats
                </Typography>
                <List sx={{ width: '100%' }}>
                    {chats.map((chat) => (
                        <ListItem key={chat.id} button>
                            <ListItemText primary={chat.name} />
                        </ListItem>
                    ))}
                </List>
            </Box>

            <Box sx={{ flexGrow: 1, p: 4 }}>
                <Typography variant="h4">Welcome to the Chat Application</Typography>
            </Box>
        </Box>
    );
};

export default Chat;
