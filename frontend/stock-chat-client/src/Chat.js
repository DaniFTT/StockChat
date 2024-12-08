import React, { useState, useEffect } from 'react';
import {
    Box,
    Button,
    List,
    ListItem,
    ListItemText,
    Typography,
    Divider,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Snackbar,
    Alert,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';

const Chat = ({ user, onLogout }) => {
    const [chats, setChats] = useState([]);
    const [newChatName, setNewChatName] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [connection, setConnection] = useState(null);
    const [selectedChat, setSelectedChat] = useState(null);

    const [messageText, setMessageText] = useState('');
    const [messages, setMessages] = useState([]);

    const [notification, setNotification] = useState({ type: '', message: '' });
    const [showSnackbar, setShowSnackbar] = useState(false);

    const navigate = useNavigate();

    const handleSendMessage = async () => {
        if (!messageText.trim() || !selectedChat || !connection) return;

        try {
            await connection.invoke('SendMessage', selectedChat.id, messageText);
            setMessageText('');
        } catch (err) {
            console.error('Error sending message:', err);
        }
    };

    useEffect(() => {
        const connectToHub = async () => {
            if (connection) return;

            const newConnection = new HubConnectionBuilder()
                .withUrl('https://localhost:8081/chat', { withCredentials: true })
                .withAutomaticReconnect()
                .build();

            try {
                await newConnection.start();
                console.log('Connected to SignalR Hub');

                newConnection.on('NewChat', (chat) => {
                    setChats((prevChats) => {
                        const chatExists = prevChats.some((c) => c.id === chat.id);
                        if (!chatExists) {
                            return [chat, ...prevChats];
                        }
                        return prevChats;
                    });

                    if (chat.createdBy === user.email) {
                        setSelectedChat(chat);
                    }
                });

                newConnection.on('Notification', (type, message) => {
                    setNotification({ type, message });
                    setShowSnackbar(true);
                });

                setConnection(newConnection);
            } catch (error) {
                console.error('Error connecting to SignalR Hub:', error);
            }
        };

        connectToHub();

        return () => {
            if (connection) {
                connection.stop();
            }
        };
    }, [connection, user.email]);

    useEffect(() => {
        if (!connection || !selectedChat) return;

        const handleNewMessage = (userType, userName, text, createdAt) => {
            setMessages((prevMessages) => {
                const updatedMessages = [...prevMessages, { userType, user: { fullName: userName }, text, createdAt }];

                if (updatedMessages.length > 50) {
                    return updatedMessages.slice(updatedMessages.length - 50);
                }

                return updatedMessages;
            });
        };

        const handleGetLastMessages = (lastMessages) => {
            setMessages(lastMessages.slice(Math.max(lastMessages.length - 50, 0)));
        };

        connection.on('NewMessage', handleNewMessage);
        connection.on('GetLastMessages', handleGetLastMessages);

        connection
            .invoke('GetLastMessages', selectedChat.id)
            .catch((err) => console.error('Error fetching messages:', err));

        return () => {
            connection.off('NewMessage', handleNewMessage);
            connection.off('GetLastMessages', handleGetLastMessages);
        };
    }, [connection, selectedChat]);

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
                onLogout();
                navigate('/');
            } else {
                console.error('Logout failed');
            }
        } catch (err) {
            console.error(err);
        }
    };

    const handleAddChat = () => {
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setNewChatName('');
    };

    const handleCreateChat = async () => {
        if (!newChatName || !connection) return;

        try {
            await connection.invoke('CreateChat', newChatName);
            handleCloseModal();
        } catch (err) {
            console.error('Error creating chat:', err);
        }
    };

    const handleChatClick = async (chat) => {
        let currentChat = selectedChat;
        setSelectedChat(chat);
        setMessages([]);
        if (connection) {
            try {
                await connection.invoke('JoinChat', chat.id, currentChat?.id);
                await connection.invoke('GetLastMessages', chat.id);
            } catch (err) {
                console.error('Error joining or fetching messages:', err);
            }
        }
    };

    const getSeverity = (type) => {
        switch (type) {
            case 0:
                return 'success';
            case 1:
                return 'error';
            case 2:
                return 'info';
            default:
                return 'info';
        }
    };

    return (
        <Box sx={{ display: 'flex'}}>
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
                    Hello, {user.fullName}
                </Typography>

                <Button variant="contained" color="error" onClick={handleLogout} sx={{ mb: 4 }}>
                    Logout
                </Button>

                <Divider sx={{ width: '100%', mb: 2 }} />
                <Box
                    sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        width: '100%',
                        mb: 2,
                    }}
                >
                    <Typography variant="subtitle1">Chats</Typography>
                    <Button variant="contained" color="primary" onClick={handleAddChat}>
                        Add Chat
                    </Button>
                </Box>

                <List sx={{ width: '100%', overflowY: 'auto', flexGrow: 1 }}>
                    {chats.map((chat) => (
                        <ListItem key={chat.id} button={true} onClick={() => handleChatClick(chat)} selected={selectedChat && chat.id === selectedChat.id}>
                            <ListItemText primary={chat.chatName} />
                        </ListItem>
                    ))}
                </List>
            </Box>

            <Box sx={{ flexGrow: 1, p: 4, display: 'flex', flexDirection: 'column', height: '90vh' }}>
                {selectedChat ? (
                    <>
                        <Typography variant="h4" sx={{ mb: 2 }}>{selectedChat.chatName}</Typography>

                        <Box
                            sx={{
                                flexGrow: 1,
                                overflowY: 'auto',
                                border: '1px solid #ccc',
                                borderRadius: '8px',
                                padding: 2,
                                marginBottom: 2,
                                height: '70%',
                                display: 'flex',
                                flexDirection: 'column',
                            }}
                        >
                            {messages?.map((message, index) => (
                                <Box key={index} sx={{ mb: 1 }}>
                                    <Typography variant="body2" color={message.userType === 'Admin' ? 'error' : 'primary'}>
                                        <strong>{message.userType === 1 ? 'Admin' : (message.userType === 2 ? 'StockBot' : `${message.user.fullName}`)}</strong>: {message.text}
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary">
                                        {new Date(message.createdAt).toLocaleString(undefined, {
                                            dateStyle: 'short',
                                            timeStyle: 'medium',
                                        })}
                                    </Typography>
                                </Box>
                            ))}
                        </Box>

                        <Box sx={{ display: 'flex', gap: 2 }}>
                            <TextField
                                fullWidth
                                label="Type your message"
                                variant="outlined"
                                value={messageText}
                                onChange={(e) => setMessageText(e.target.value)}
                                onKeyPress={(e) => {
                                    if (e.key === 'Enter') {
                                        handleSendMessage();
                                        e.preventDefault();
                                    }
                                }}
                            />
                            <Button variant="contained" color="primary" onClick={handleSendMessage}>
                                Send
                            </Button>
                        </Box>
                    </>
                ) : (
                    <Typography variant="h4">Welcome to the Chat Application</Typography>
                )}
            </Box>

            <Dialog open={isModalOpen} onClose={handleCloseModal}>
                <DialogTitle>Create New Chat</DialogTitle>
                <DialogContent>
                    <TextField
                        fullWidth
                        label="Chat Name"
                        value={newChatName}
                        onChange={(e) => setNewChatName(e.target.value)}
                        sx={{ mt: 2 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseModal}>Cancel</Button>
                    <Button variant="contained" color="primary" onClick={handleCreateChat}>
                        Create
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={showSnackbar}
                autoHideDuration={6000}
                onClose={() => setShowSnackbar(false)}
                anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
            >
                <Alert
                    onClose={() => setShowSnackbar(false)}
                    severity={getSeverity(notification.type)}
                    sx={{ width: '100%' }}
                >
                    {notification.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};

export default Chat;
