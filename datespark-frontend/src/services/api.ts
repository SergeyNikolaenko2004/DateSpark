import { Idea, IdeaVote, IdeaFilters, AuthRequest, AuthResponse } from '../types';

const API_BASE = 'https://datespark-api.onrender.com/api';

export const api = {
  async getRandomIdea(filters?: IdeaFilters): Promise<Idea | null> {
    try {
      const queryParams = filters ? `?${new URLSearchParams(filters as any)}` : '';
      const response = await fetch(`${API_BASE}/spark/random${queryParams}`);
      
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
      const response = await fetch(`${API_BASE}/spark/vote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(vote),
      });
      
      return response.ok;
    } catch (error) {
      console.error('API Error:', error);
      return false;
    }
  },

    // 🔥 НОВЫЕ МЕТОДЫ АУТЕНТИФИКАЦИИ
  async register(userData: AuthRequest): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData),
      });
      return await response.json();
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
      return await response.json();
    } catch (error) {
      return { success: false, message: 'Network error' };
    }
  }

};