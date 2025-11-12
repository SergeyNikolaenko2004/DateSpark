import { Idea, IdeaVote, IdeaFilters, AuthRequest, AuthResponse } from '../types';

const API_BASE = 'https://datespark-api.onrender.com/api';

// 🔥 ФУНКЦИЯ ДЛЯ ПОЛУЧЕНИЯ ТОКЕНА
const getToken = (): string | null => {
  return localStorage.getItem('authToken'); // или как ты хранишь токен
};

export const api = {
  async getRandomIdea(filters?: IdeaFilters): Promise<Idea | null> {
    try {
      // 🔥 ПРАВИЛЬНОЕ ФОРМИРОВАНИЕ QUERY PARAMS
      const params = new URLSearchParams();
      
      if (filters) {
        Object.entries(filters).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            params.append(key, value.toString());
          }
        });
      }
      
      const queryString = params.toString();
      const url = `${API_BASE}/spark/random${queryString ? `?${queryString}` : ''}`;
      
      console.log('Fetching idea from:', url); // Для дебага
      
      const response = await fetch(url);
      
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
      
      console.log('=== VOTE DEBUG ===');
      console.log('Token exists:', !!token);
      console.log('Vote data:', { ideaId: vote.ideaId, isLike: vote.isLike });

      if (!token) {
        console.error('❌ No token found for voting!');
        return false;
      }

      const response = await fetch(`${API_BASE}/spark/vote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          ideaId: vote.ideaId,
          isLike: vote.isLike
        }),
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('❌ Vote failed:', {
          status: response.status,
          statusText: response.statusText,
          error: errorText
        });
        return false;
      }
      
      console.log('✅ Vote successful!');
      return true;
    } catch (error) {
      console.error('❌ API Error:', error);
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
      
      if (result.success && result.token) {
        localStorage.setItem('authToken', result.token);
        console.log('Token saved:', result.token.substring(0, 20) + '...'); // Логируем часть токена
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
      
      // 🔥 УБЕДИСЬ ЧТО ТОКЕН СОХРАНЯЕТСЯ ПРАВИЛЬНО
      if (result.success && result.token) {
        localStorage.setItem('authToken', result.token);
        console.log('Token saved:', result.token.substring(0, 20) + '...'); // Логируем часть токена
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