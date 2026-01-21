import React, { useState } from 'react';
import { HashRouter as Router, Routes, Route } from 'react-router-dom'; // 🔥 Изменили здесь
import SparkPage from './pages/SparkPage';
import { LoginForm } from './components/LoginForm';
import { RegisterForm } from './components/RegisterForm';
import AdventureBoard from './components/AdventureBoard';
import Profile from './components/Profile';
import BottomNavigation from './components/BottomNavigation';
import './App.css';

function App() {
  const token = localStorage.getItem('authToken');
  const [isLogin, setIsLogin] = useState(true);

  if (!token) {
    return isLogin ? (
      <LoginForm onSwitchToRegister={() => setIsLogin(false)} />
    ) : (
      <RegisterForm onSwitchToLogin={() => setIsLogin(true)} />
    );
  }

  return (
    <Router> {/* Теперь это HashRouter */}
      <div className="app">
        <main className="app-content">
          <Routes>
            <Route path="/" element={<SparkPage />} />
            <Route path="/board" element={<AdventureBoard />} />
            <Route path="/profile" element={<Profile />} />
          </Routes>
        </main>
        <BottomNavigation />
      </div>
    </Router>
  );
}

export default App;