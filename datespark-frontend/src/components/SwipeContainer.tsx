import React, { useState, useEffect, useRef } from 'react';
import IdeaCard from './IdeaCard';
import { api } from '../services/api';
import { Idea } from '../types';
import './SwipeContainer.css';

const SwipeContainer: React.FC = () => {
  const [currentIdea, setCurrentIdea] = useState<Idea | null>(null);
  const [loading, setLoading] = useState(true);
  const [swipeDirection, setSwipeDirection] = useState<'left' | 'right' | null>(null);
  const [isSwiping, setIsSwiping] = useState(false);
  const cardRef = useRef<HTMLDivElement>(null);
  const startX = useRef(0);
  const currentX = useRef(0);

  const fetchRandomIdea = async () => {
    try {
      setLoading(true);
      const idea = await api.getRandomIdea();
      setCurrentIdea(idea);
      setSwipeDirection(null);
    } catch (error) {
      console.error('Error fetching idea:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSwipe = async (direction: 'left' | 'right') => {
    if (!currentIdea) return;

    setSwipeDirection(direction);
    
    // Ждем завершения анимации перед загрузкой следующей карточки
    setTimeout(async () => {
      // Отправляем голос на сервер
      await api.voteForIdea({
        ideaId: currentIdea.id,
        isLike: direction === 'right'
      });

      // Загружаем следующую идею
      fetchRandomIdea();
    }, 300);
  };

  // Обработчики свайпов для тач-устройств
  const handleTouchStart = (e: React.TouchEvent) => {
    setIsSwiping(true);
    startX.current = e.touches[0].clientX;
    currentX.current = startX.current;
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const touch = e.touches[0];
    currentX.current = touch.clientX;
    const offsetX = currentX.current - startX.current;
    const rotate = offsetX * 0.1; // Коэффициент вращения

    cardRef.current.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;
    
    // ❌ УБРАЛИ изменение фона
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const offsetX = currentX.current - startX.current;

    // Сбрасываем трансформацию
    cardRef.current.style.transform = '';
    setIsSwiping(false);

    // Определяем направление свайпа
    if (offsetX > 100) {
      handleSwipe('right'); // Свайп вправо = лайк
    } else if (offsetX < -100) {
      handleSwipe('left'); // Свайп влево = дизлайк
    }
  };

  // Обработчики для desktop (drag & drop)
  const handleMouseDown = (e: React.MouseEvent) => {
    setIsSwiping(true);
    startX.current = e.clientX;
    currentX.current = startX.current;
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current) return;

    currentX.current = e.clientX;
    const offsetX = currentX.current - startX.current;
    const rotate = offsetX * 0.1;

    cardRef.current.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;
    
    // ❌ УБРАЛИ изменение фона
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const offsetX = currentX.current - startX.current;

    cardRef.current.style.transform = '';
    setIsSwiping(false);

    if (offsetX > 100) {
      handleSwipe('right');
    } else if (offsetX < -100) {
      handleSwipe('left');
    }
  };

  // Снимаем обработчики при отпускании мыши вне карточки
  useEffect(() => {
    const handleGlobalMouseUp = () => {
      setIsSwiping(false);
      if (cardRef.current) {
        cardRef.current.style.transform = '';
      }
    };

    document.addEventListener('mouseup', handleGlobalMouseUp);
    return () => document.removeEventListener('mouseup', handleGlobalMouseUp);
  }, []);

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
      <div 
        ref={cardRef}
        className={`idea-card-wrapper ${swipeDirection ? `swipe-${swipeDirection}` : ''}`}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp} // Сброс при выходе за пределы карточки
      >
        <IdeaCard idea={currentIdea} onSwipe={handleSwipe} />
      </div>
    </div>
  );
};

export default SwipeContainer;