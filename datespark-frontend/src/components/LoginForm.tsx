// components/LoginForm.tsx
import React, { useState } from 'react';
import { api } from '../services/api';
import './LoginForm.css';

interface LoginFormProps {
  onSwitchToRegister: () => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSwitchToRegister }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    const result = await api.login({ email, password });
    
    if (result.success) {
      localStorage.setItem('authToken', result.token!);
      window.location.reload(); 
    } else {
      setError(result.message);
    }
    setLoading(false);
  };

  return (
    <div className="login-container">
      <div className="login-form">
        <h2 className="login-title">Вход в DateSpark</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="form-input"
              required
            />
          </div>
          <div className="form-group">
            <input
              type="password"
              placeholder="Пароль"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="form-input"
              required
            />
          </div>
          {error && <div className="form-error">{error}</div>}
          <button 
            type="submit" 
            className="login-button"
            disabled={loading}
          >
            {loading ? 'Вход...' : 'Войти'}
          </button>
        </form>
        <div className="switch-form-text">
          Нет аккаунта?{' '}
          <button onClick={onSwitchToRegister} className="switch-button">
            Зарегистрироваться
          </button>
        </div>
      </div>
    </div>
  );
};