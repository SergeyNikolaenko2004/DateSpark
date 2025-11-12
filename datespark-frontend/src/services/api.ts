import { Idea, IdeaVote, IdeaFilters, AuthRequest, AuthResponse } from '../types';

const API_BASE = 'https://datespark-api.onrender.com/api';

// 🔥 ФУНКЦИЯ ДЛЯ ПОЛУЧЕНИЯ ТОКЕНА
const getToken = (): string | null => {
  return localStorage.getItem('authToken'); // или как ты хранишь токен
};

export const api = {
  async getRandomIdea(filters?: IdeaFilters): Promise<Idea | null> {
    try {
      const token = getToken();
      const queryParams = filters ? `?${new URLSearchParams(filters as any)}` : '';
      
      const response = await fetch(`${API_BASE}/spark/random${queryParams}`, {
        headers: {
          'Authorization': `Bearer ${token}` // 🔥 ДОБАВЬ ТОКЕН
        }
      });
      
      if (!response.ok) {
        if (response.status === 404) return null;
        throw new Error('Failed to fetch idea');
      }
      
      return await response.json();
    } catch (error) {
      console.error('API Error:', error);
      return null;
    }
  },

  async voteForIdea(vote: IdeaVote): Promise<boolean> {
    try {
      const token = getToken();
      
      const response = await fetch(`${API_BASE}/spark/vote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` // 🔥 ДОБАВЬ ТОКЕН
        },
        body: JSON.stringify({
          ideaId: vote.ideaId,
          isLike: vote.isLike
          // userId НЕ отправляем - он в токене
        }),
      });
      
      return response.ok;
    } catch (error) {
      console.error('API Error:', error);
      return false;
    }
  },

  async register(userData: AuthRequest): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData),
      });
      const result = await response.json();
      
      // 🔥 СОХРАНЯЕМ ТОКЕН ПРИ УСПЕШНОЙ РЕГИСТРАЦИИ
      if (result.success && result.token) {
        localStorage.setItem('authToken', result.token);
      }
      
      return result;
    } catch (error) {
      return { success: false, message: 'Network error' };
    }
  },

  async login(userData: AuthRequest): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE}/auth/login`, {
        method: 'POST', 
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData),
      });
      const result = await response.json();
      
      // 🔥 СОХРАНЯЕМ ТОКЕН ПРИ УСПЕШНОМ ВХОДЕ
      if (result.success && result.token) {
        localStorage.setItem('authToken', result.token);
      }
      
      return result;
    } catch (error) {
      return { success: false, message: 'Network error' };
    }
  },
  async createCouple(): Promise<AuthResponse> {
    try {
      const token = getToken();
      const response = await fetch(`${API_BASE}/auth/create-couple`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` // 🔥 ДОБАВЬ ТОКЕН
        }
      });
      return await response.json();
    } catch (error) {
      return { success: false, message: 'Network error' };
    }
  },

  async joinCouple(joinCode: string): Promise<AuthResponse> {
    try {
      const token = getToken();
      const response = await fetch(`${API_BASE}/auth/join-couple`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` // 🔥 ДОБАВЬ ТОКЕН
        },
        body: JSON.stringify({ joinCode })
      });
      return await response.json();
    } catch (error) {
      return { success: false, message: 'Network error' };
    }
  }


};