export interface Idea {
  id: number;
  title: string;
  description: string;
  category: string;
  priceCategory: number;
  location: string;
  mood: string;
  duration: string;
  weather: string;
  likes: number;
  dislikes: number;
  isActive: boolean;
  createdDate: string;
}

export interface IdeaFilters {
  category?: string;
  location?: string;
  mood?: string;
  duration?: string;
  weather?: string;
  priceCategory?: number;
  onlyActive?: boolean;
}

export interface IdeaVote {
  ideaId: number;
  isLike: boolean;
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

export enum AdventureStatus {
  Liked = 0,      // 💡
  Planned = 1,    // 📅
  InProgress = 2, // 🚀
  Completed = 3   // ✅
}

export interface AdventureCard {
  id: number;
  ideaId?: number;
  coupleId: number;
  createdByUserId: number;
  createdByUserName: string;
  title: string;
  description: string;
  status: AdventureStatus;
  statusSymbol: string;
  plannedDate?: string;
  completedDate?: string;
  notes: string;
  photoUrl: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAdventureFromIdeaRequest {
  ideaId: number;
}

export interface CreateAdventureManualRequest {
  title: string;
  description: string;
}

export interface UpdateAdventureStatusRequest {
  status: AdventureStatus;
}

export interface UpdateAdventureDateRequest {
  plannedDate?: string;
}