export interface Idea {
  id: number;
  title: string;
  description: string;
  category: string;
  price: number;
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