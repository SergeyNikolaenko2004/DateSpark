import { Idea, IdeaVote, IdeaFilters, AuthRequest, AuthResponse, AdventureCard, AdventureStatus } from '../types';

const API_BASE = 'https://datespark-api.onrender.com/api';

const getToken = (): string | null => {
  return localStorage.getItem('authToken');
};

interface UserInfo {
  id: number;
  email: string;
  name: string;
  avatar?: string;
  createdAt: string;
}

interface CoupleInfo {
  id: number;
  name: string;
  joinCode: string;
  createdAt: string;
}

interface PartnerInfo {
  id: number;
  name: string;
  email: string;
  role: string;
  joinedAt: string;
}

interface ProfileResponse {
  success: boolean;
  user: UserInfo;
  couple?: CoupleInfo;
  partners: PartnerInfo[];
}

interface UpdateProfileRequest {
  name?: string;
  avatar?: string;
}

const handleAuthError = (): void => {
  localStorage.removeItem('authToken');
  window.location.href = '/';
};

export const api = {
  async getRandomIdea(filters?: IdeaFilters): Promise<Idea | null> {
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

    const response = await fetch(url);

    if (!response.ok) {
      if (response.status === 404) return null;
      throw new Error('Failed to fetch idea');
    }

    return await response.json();
  },

  async voteForIdea(vote: IdeaVote): Promise<boolean> {
    const token = getToken();
    if (!token) {
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

    return response.ok;
  },

  async register(userData: AuthRequest): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData),
    });
    const result = await response.json();

    if (result.success && result.token) {
      localStorage.setItem('authToken', result.token);
    }

    return result;
  },

  async login(userData: AuthRequest): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData),
    });
    const result = await response.json();

    if (result.success && result.token) {
      localStorage.setItem('authToken', result.token);
    }

    return result;
  },

  async createCouple(coupleName?: string): Promise<AuthResponse> {
    const token = getToken();
    const body = coupleName ? { coupleName } : {};

    const response = await fetch(`${API_BASE}/auth/create-couple`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(body)
    });
    return await response.json();
  },

  async updateCouple(coupleName: string): Promise<AuthResponse> {
    const token = getToken();
    const response = await fetch(`${API_BASE}/auth/update-couple`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ coupleName })
    });
    return await response.json();
  },

  async joinCouple(joinCode: string): Promise<AuthResponse> {
    const token = getToken();
    const response = await fetch(`${API_BASE}/auth/join-couple`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ joinCode })
    });
    return await response.json();
  },

  async getProfile(): Promise<ProfileResponse> {
    const token = getToken();

    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/profile`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to fetch profile: ${response.status}`);
    }

    return await response.json();
  },

  async updateProfile(profileData: UpdateProfileRequest): Promise<AuthResponse> {
    const token = getToken();
    if (!token) {
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/profile/user`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(profileData)
    });

    if (!response.ok) {
      throw new Error(`Failed to update profile: ${response.status}`);
    }

    return await response.json();
  },

  async getCoupleAdventures(): Promise<AdventureCard[]> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/couple`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to fetch adventures: ${response.status}`);
    }

    return await response.json();
  },

  async getAdventuresByStatus(status: AdventureStatus): Promise<AdventureCard[]> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/couple/status/${status}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to fetch adventures: ${response.status}`);
    }

    return await response.json();
  },

  async createAdventureFromIdea(ideaId: number): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/from-idea`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ ideaId })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to create adventure`);
    }

    return await response.json();
  },

  async createAdventureManual(title: string, description: string): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/manual`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ title, description })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to create adventure`);
    }

    return await response.json();
  },

  async updateAdventureStatus(adventureId: number, status: AdventureStatus): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/${adventureId}/status`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ status })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to update status`);
    }

    return await response.json();
  },

  async updateAdventureDate(adventureId: number, plannedDate?: string): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/${adventureId}/date`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ plannedDate })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to update date`);
    }

    return await response.json();
  },

  async updateAdventureNotes(adventureId: number, notes: string): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/${adventureId}/notes`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ notes })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to update notes`);
    }

    return await response.json();
  },

  async completeAdventure(adventureId: number, photoUrl: string, notes: string): Promise<AdventureCard> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/${adventureId}/complete`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ photoUrl, notes })
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to complete adventure`);
    }

    return await response.json();
  },

  async deleteAdventure(adventureId: number): Promise<boolean> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/${adventureId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to delete adventure`);
    }

    const result = await response.json();
    return result.success === true;
  },

  async canCreateFromIdea(ideaId: number): Promise<boolean> {
    const token = getToken();
    if (!token) {
      handleAuthError();
      throw new Error('No authentication token');
    }

    const response = await fetch(`${API_BASE}/adventures/can-create/${ideaId}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (response.status === 401) {
      handleAuthError();
      throw new Error('Session expired');
    }

    if (!response.ok) {
      throw new Error(`Failed to check if can create`);
    }

    const result = await response.json();
    return result.canCreate === true;
  }
};