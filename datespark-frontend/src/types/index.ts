export interface Idea {
  id: number;
  title: string;
  description: string;
  category: string;
  priceCategory: string; // "$", "$$", "$$$"
  location: string;
  mood: string;
  duration: string;
  weather: string;
  likes: number;
  dislikes: number;
  isActive: boolean;
  createdDate: string;
}

export interface IdeaVote {
  ideaId: number;
  isLike: boolean;
}

export interface IdeaFilters {
  category?: string;
  location?: string;
  mood?: string;
  duration?: string;
  weather?: string;
  maxPrice?: number;
  onlyActive?: boolean;
}

export interface AuthRequest {
  email: string;
  password: string;
  name?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: UserDto;
  couple?: CoupleDto;
}

export interface UserDto {
  id: number;
  email: string;
  name: string;
}

export interface CoupleDto {
  id: number;
  name: string;
  joinCode: string;
}