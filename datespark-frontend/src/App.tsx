import React, { useState } from 'react';
import SparkPage from './pages/SparkPage';
import { LoginForm } from './components/LoginForm';
import { RegisterForm } from './components/RegisterForm';
import './App.css';

function App() {
  const token = localStorage.getItem('token');
  const [isLogin, setIsLogin] = useState(true);

  if (token) {
    return <SparkPage />;
  }

  return isLogin ? (
    <LoginForm onSwitchToRegister={() => setIsLogin(false)} />
  ) : (
    <RegisterForm onSwitchToLogin={() => setIsLogin(true)} />
  );
}

export default App;