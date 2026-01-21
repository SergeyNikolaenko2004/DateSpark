import React, { useState, useEffect, useRef } from 'react';
import IdeaCard from './IdeaCard';
import { api } from '../services/api';
import { Idea, IdeaFilters } from '../types';
import './SwipeContainer.css';

const FILTER_OPTIONS = {
  categories: ['Романтическое', 'Активное', 'Творческое', 'Приключение', 'Релакс', 'Еда', 'Культура'],
  locations: ['Дома', 'На улице', 'В городе', 'Природа'],
  moods: ['Романтичное', 'Веселое', 'Расслабленное', 'Приключенческое', 'Уютное', 'Экзотическое'],
  weather: ['Любая', 'Солнечно', 'Дождь', 'Снег', 'Облачно', 'Тепло', 'Холодно'],
  priceCategories: [
    { value: 1, label: '$' },
    { value: 2, label: '$$' },
    { value: 3, label: '$$$' }
  ]
};

const SwipeContainer: React.FC = () => {
  const [currentIdea, setCurrentIdea] = useState<Idea | null>(null);
  const [loading, setLoading] = useState(true);
  const [swipeDirection, setSwipeDirection] = useState<'left' | 'right' | null>(null);
  const [isSwiping, setIsSwiping] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [activeFilters, setActiveFilters] = useState<IdeaFilters>({});
  
  const cardRef = useRef<HTMLDivElement>(null);
  const startX = useRef(0);
  const currentX = useRef(0);

  const fetchRandomIdea = async (filters?: IdeaFilters) => {
    try {
      setLoading(true);
      const idea = await api.getRandomIdea(filters);
      setCurrentIdea(idea);
      setSwipeDirection(null);
    } catch (error) {
      console.error('Error fetching idea:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (filterType: keyof IdeaFilters, value: any) => {
    const newFilters = { ...activeFilters };
    
    if (value === '' || value === null) {
      delete newFilters[filterType];
    } else {
      newFilters[filterType] = value;
    }
    
    setActiveFilters(newFilters);
    fetchRandomIdea(newFilters);
  };

  const handleResetFilters = () => {
    setActiveFilters({});
    fetchRandomIdea({});
  };

const handleSwipe = async (direction: 'left' | 'right') => {
  if (!currentIdea) return;

  setSwipeDirection(direction);
  
  setTimeout(async () => {
    // 1. Сначала голосуем
    const voteSuccess = await api.voteForIdea({
      ideaId: currentIdea.id,
      isLike: direction === 'right'
    });

    // 2. АВТОМАТИЧЕСКОЕ ДОБАВЛЕНИЕ В ДОСКУ ПРИ ЛАЙКЕ
    if (direction === 'right' && voteSuccess) {
      try {
        // Проверяем, можно ли добавить (не дублировать)
        const canAdd = await api.canCreateFromIdea(currentIdea.id);
        if (canAdd) {
          await api.createAdventureFromIdea(currentIdea.id);
          console.log('✅ Идея "' + currentIdea.title + '" добавлена в доску!');
        } else {
          console.log('ℹ️ Идея уже была добавлена ранее');
        }
      } catch (err: any) {
        console.log('⚠️ Не удалось добавить в доску:', err.message);
      }
    }

    // 3. Загружаем следующую идею
    fetchRandomIdea(activeFilters);
  }, 300); // Анимация свайпа
};

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
    const rotate = offsetX * 0.1;
    cardRef.current.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current) return;
    const offsetX = currentX.current - startX.current;
    cardRef.current.style.transform = '';
    setIsSwiping(false);
    if (offsetX > 100) handleSwipe('right');
    else if (offsetX < -100) handleSwipe('left');
  };

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
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current) return;
    const offsetX = currentX.current - startX.current;
    cardRef.current.style.transform = '';
    setIsSwiping(false);
    if (offsetX > 100) handleSwipe('right');
    else if (offsetX < -100) handleSwipe('left');
  };

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

  useEffect(() => {
    fetchRandomIdea(activeFilters);
  }, []);

  if (loading) {
    return <div className="loading">Ищем идеи для вас... 💫</div>;
  }

  if (!currentIdea) {
    return (
      <div className="no-ideas">
        <h2>Идеи закончились!</h2>
        <p>Попробуйте изменить фильтры </p>
        <button className="reset-filters-btn" onClick={handleResetFilters}>
          Сбросить фильтры
        </button>
      </div>
    );
  }

  return (
    <div className="swipe-container">
      <div className="filters-header">
        <button 
          className={`filters-toggle ${showFilters ? 'active' : ''}`}
          onClick={() => setShowFilters(!showFilters)}
        >
          Фильтры 
          {Object.keys(activeFilters).length > 0 && (
            <span className="active-filters-count">
              {Object.keys(activeFilters).length}
            </span>
          )}
        </button>
        
        {Object.keys(activeFilters).length > 0 && (
          <button className="reset-filters-btn" onClick={handleResetFilters}>
            ❌ Сбросить
          </button>
        )}
      </div>

      {showFilters && (
        <div className="filters-panel">
          <div className="filter-group">
            <label>Категория</label>
            <select 
              value={activeFilters.category || ''}
              onChange={(e) => handleFilterChange('category', e.target.value)}
            >
              <option value="">Все категории</option>
              {FILTER_OPTIONS.categories.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
          </div>

          {/* Фильтр по локации */}
          <div className="filter-group">
            <label>Локация</label>
            <select 
              value={activeFilters.location || ''}
              onChange={(e) => handleFilterChange('location', e.target.value)}
            >
              <option value="">Все локации</option>
              {FILTER_OPTIONS.locations.map(loc => (
                <option key={loc} value={loc}>{loc}</option>
              ))}
            </select>
          </div>

          {/* Фильтр по настроению */}
          <div className="filter-group">
            <label>Настроение</label>
            <select 
              value={activeFilters.mood || ''}
              onChange={(e) => handleFilterChange('mood', e.target.value)}
            >
              <option value="">Любое настроение</option>
              {FILTER_OPTIONS.moods.map(mood => (
                <option key={mood} value={mood}>{mood}</option>
              ))}
            </select>
          </div>

          {/* Фильтр по погоде */}
          <div className="filter-group">
            <label>Погода</label>
            <select 
              value={activeFilters.weather || ''}
              onChange={(e) => handleFilterChange('weather', e.target.value)}
            >
              <option value="">Любая погода</option>
              {FILTER_OPTIONS.weather.map(weather => (
                <option key={weather} value={weather}>{weather}</option>
              ))}
            </select>
          </div>

          {/* Фильтр по бюджету */}
          <div className="filter-group">
            <label>Бюджет</label>
            <select 
              value={activeFilters.priceCategory || ''}
              onChange={(e) => handleFilterChange('priceCategory', e.target.value ? parseInt(e.target.value) : null)}
            >
              <option value="">Любой бюджет</option>
              {FILTER_OPTIONS.priceCategories.map(price => (
                <option key={price.value} value={price.value}>{price.label}</option>
              ))}
            </select>
          </div>
        </div>
      )}

      {/* Карточка идеи */}
      <div 
        ref={cardRef}
        className={`idea-card-wrapper ${swipeDirection ? `swipe-${swipeDirection}` : ''}`}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
      >
        <IdeaCard idea={currentIdea} onSwipe={handleSwipe} />
      </div>
    </div>
  );
};

export default SwipeContainer;