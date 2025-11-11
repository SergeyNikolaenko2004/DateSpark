import React, { useState, useEffect } from 'react';
import IdeaCard from './IdeaCard';
import { api } from '../services/api';
import { Idea } from '../types';
import './SwipeContainer.css';

const SwipeContainer: React.FC = () => {
  const [currentIdea, setCurrentIdea] = useState<Idea | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchRandomIdea = async () => {
    try {
      setLoading(true);
      const idea = await api.getRandomIdea();
      setCurrentIdea(idea);
    } catch (error) {
      console.error('Error fetching idea:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSwipe = async (direction: 'left' | 'right') => {
    if (!currentIdea) return;

    // Отправляем голос на сервер
    await api.voteForIdea({
      ideaId: currentIdea.id,
      isLike: direction === 'right'
    });

    // Загружаем следующую идею
    fetchRandomIdea();
  };

  // Загружаем первую идею при монтировании
  useEffect(() => {
    fetchRandomIdea();
  }, []);

  if (loading) {
    return <div className="loading">Ищем идеи для вас... 💫</div>;
  }

  if (!currentIdea) {
    return (
      <div className="no-ideas">
        <h2>Идеи закончились! 🎉</h2>
        <p>Попробуйте изменить фильтры или добавить новые идеи</p>
      </div>
    );
  }

  return (
    <div className="swipe-container">
      <IdeaCard idea={currentIdea} onSwipe={handleSwipe} />
    </div>
  );
};

export default SwipeContainer;