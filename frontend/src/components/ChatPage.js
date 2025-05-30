// src/components/ChatPage.js
import React, { useState, useContext, useEffect, useRef } from 'react';
import AuthContext from '../contexts/AuthContext';
import { generateText } from '../services/api';
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';

export default function ChatPage() {
  const { token } = useContext(AuthContext);
  const [prompt, setPrompt] = useState('');
  const [messages, setMessages] = useState([]); 
  const scrollRef = useRef();

  // автоскролл вниз
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages]);

  async function handleSend(e) {
    e.preventDefault();
    const text = prompt.trim();
    if (!text) return;
    setMessages(msgs => [...msgs, { sender: 'user', text }]);
    setPrompt('');
    try {
      const botText = await generateText(token, text);
      setMessages(msgs => [...msgs, { sender: 'bot', text: botText }]);
    } catch (err) {
      setMessages(msgs => [...msgs, { sender: 'bot', text: 'Ошибка: ' + err.message }]);
    }
  }

  return (
    <Container
      maxWidth="md"
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: 'calc(100vh - 64px)',
        py: 2,
        bgcolor: '#f5f5f5',
      }}
    >
      {/* Окно сообщений */}
      <Box
        ref={scrollRef}
        sx={{
          flex: 1,
          p: 2,
          mb: 2,
          overflowY: 'auto',
          backgroundColor: '#fff',
          borderRadius: 2,
          boxShadow: 1,
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {messages.map((m, i) => (
          <Box
            key={i}
            sx={{
              alignSelf: m.sender === 'user' ? 'flex-end' : 'flex-start',
              backgroundColor:
                m.sender === 'user' ? 'primary.main' : 'grey.200',
              color:
                m.sender === 'user' ? 'common.white' : 'text.primary',
              p: 1.5,
              borderRadius: 2,
              maxWidth: '80%',
              mb: 1,
              boxShadow: 0.5,
            }}
          >
            <Typography
              variant="caption"
              sx={{ fontWeight: 'bold', display: 'block', mb: 0.5 }}
            >
              {m.sender === 'user' ? 'Вы' : 'Бот'}
            </Typography>
            <Typography variant="body1">{m.text}</Typography>
          </Box>
        ))}
      </Box>

      {/* Поле ввода + кнопка */}
      <Box component="form" onSubmit={handleSend} sx={{ display: 'flex' }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Введите сообщение..."
          value={prompt}
          onChange={e => setPrompt(e.target.value)}
          onKeyPress={e => {
            if (e.key === 'Enter' && !e.shiftKey) handleSend(e);
          }}
          sx={{
            backgroundColor: '#fff',
            borderRadius: 2,
          }}
        />
        <Button
          type="submit"
          variant="contained"
          endIcon={<SendIcon />}
          sx={{
            ml: 1,
            borderRadius: 2,
            textTransform: 'none',
            px: 3,
          }}
        >
          Отправить
        </Button>
      </Box>
    </Container>
  );
}
