import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';

import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import ChatPage from './components/ChatPage';
import PrivateRoute from './components/PrivateRoute';

export default function App() {
  return (
    <Box sx={{ flexGrow: 1 }}>
      {/* Шапка с навигацией */}
      <AppBar position="static" elevation={4}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            LLM Platform
          </Typography>
          <Button
            color="inherit"
            component={RouterLink}
            to="/login"
            sx={{ textTransform: 'none' }}
          >
            Войти
          </Button>
          <Button
            color="inherit"
            component={RouterLink}
            to="/register"
            sx={{ textTransform: 'none' }}
          >
            Регистрация
          </Button>
          <Button
            color="inherit"
            component={RouterLink}
            to="/chat"
            sx={{ textTransform: 'none' }}
          >
            Чат
          </Button>
        </Toolbar>
      </AppBar>

      {/* Контент страниц */}
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<PrivateRoute />}>
          <Route path="/chat" element={<ChatPage />} />
        </Route>
        <Route path="*" element={<Typography p={2}>Страница не найдена</Typography>} />
      </Routes>
    </Box>
  );
}
