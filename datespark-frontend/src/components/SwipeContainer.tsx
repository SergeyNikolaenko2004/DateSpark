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
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const touch = e.touches[0];
    const card = cardRef.current;
    const cardRect = card.getBoundingClientRect();
    const cardCenterX = cardRect.left + cardRect.width / 2;
    const touchX = touch.clientX;

    // Вычисляем смещение для анимации
    const offsetX = touchX - cardCenterX;
    const rotate = offsetX * 0.1; // Коэффициент вращения

    card.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;
    
    // Изменяем фон в зависимости от направления
    if (offsetX > 50) {
      card.style.backgroundColor = '#e8f5e8'; // Зеленый для лайка
    } else if (offsetX < -50) {
      card.style.backgroundColor = '#ffe8e8'; // Красный для дизлайка
    } else {
      card.style.backgroundColor = 'white';
    }
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const card = cardRef.current;
    const cardRect = card.getBoundingClientRect();
    const cardCenterX = cardRect.left + cardRect.width / 2;
    const touchX = e.changedTouches[0].clientX;
    const offsetX = touchX - cardCenterX;

    // Сбрасываем трансформацию
    card.style.transform = '';
    card.style.backgroundColor = 'white';
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
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const card = cardRef.current;
    const cardRect = card.getBoundingClientRect();
    const cardCenterX = cardRect.left + cardRect.width / 2;
    const mouseX = e.clientX;

    const offsetX = mouseX - cardCenterX;
    const rotate = offsetX * 0.1;

    card.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;
    
    if (offsetX > 50) {
      card.style.backgroundColor = '#e8f5e8';
    } else if (offsetX < -50) {
      card.style.backgroundColor = '#ffe8e8';
    } else {
      card.style.backgroundColor = 'white';
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current) return;

    const card = cardRef.current;
    const cardRect = card.getBoundingClientRect();
    const cardCenterX = cardRect.left + cardRect.width / 2;
    const mouseX = e.clientX;
    const offsetX = mouseX - cardCenterX;

    card.style.transform = '';
    card.style.backgroundColor = 'white';
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
        cardRef.current.style.backgroundColor = 'white';
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
        
        {/* Индикаторы свайпа */}
        <div className="swipe-indicators">
          <div className="indicator-left">❌</div>
          <div className="indicator-right">❤️</div>
        </div>
      </div>
    </div>
  );
};

export default SwipeContainer;