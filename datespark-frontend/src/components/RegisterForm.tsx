// components/RegisterForm.tsx
import React, { useState } from 'react';
import { api } from '../services/api';
import './RegisterForm.css';

interface RegisterFormProps {
  onSwitchToLogin: () => void;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onSwitchToLogin }) => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    const result = await api.register({ name, email, password });
    
    if (result.success) {
      localStorage.setItem('authToken', result.token!);
      window.location.reload();
    } else {
      setError(result.message);
    }
    setLoading(false);
  };

  return (
    <div className="register-container">
      <div className="register-form">
        <h2 className="register-title">Регистрация в DateSpark</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <input
              type="text"
              placeholder="Имя"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="form-input"
              required
            />
          </div>
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
            className="register-button"
            disabled={loading}
          >
            {loading ? 'Регистрация...' : 'Зарегистрироваться'}
          </button>
        </form>
        <div className="switch-form-text">
          Уже есть аккаунт?{' '}
          <button onClick={onSwitchToLogin} className="switch-button">
            Войти
          </button>
        </div>
      </div>
    </div>
  );
};