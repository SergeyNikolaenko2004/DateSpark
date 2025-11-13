import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import SparkPage from './pages/SparkPage';
import { LoginForm } from './components/LoginForm'; // ✅ named export
import { RegisterForm } from './components/RegisterForm'; // ✅ named export
import AdventureBoard from './components/AdventureBoard'; // ✅ default export
import Profile from './components/Profile'; // ✅ default export
import BottomNavigation from './components/BottomNavigation'; // ✅ default export
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
    <Router>
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