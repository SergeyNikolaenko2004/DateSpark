import React, { useState, useEffect, useRef } from 'react';
import IdeaCard from './IdeaCard';
import { api } from '../services/api';
import { Idea, IdeaFilters } from '../types';
import './SwipeContainer.css';

const FILTER_OPTIONS = {
  categories: ['Романтическое', 'Активное', 'Творческое', 'Приключение', 'Релакс', 'Еда', 'Культура'],
  locations: ['Дома', 'На улице', 'В городе', 'Природа'],
  moods: ['Романтичное', 'Веселое', 'Расслабленное', 'Приключенческое', 'Уютное', 'Экзотическое'],
  weather: ['Любая', 'Солнечно', 'Снег'],
  priceCategories: [
    { value: 1, label: '$' },
    { value: 2, label: '$$' },
    { value: 3, label: '$$$' }
  ]
};

const SwipeContainer: React.FC = () => {
  const [currentIdea, setCurrentIdea] = useState<Idea | null>(null);
  const [nextIdea, setNextIdea] = useState<Idea | null>(null); // 🔥 ПРЕДЗАГРУЗКА
  const [loading, setLoading] = useState(true);
  const [swipeDirection, setSwipeDirection] = useState<'left' | 'right' | null>(null);
  const [isSwiping, setIsSwiping] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [activeFilters, setActiveFilters] = useState<IdeaFilters>({});
  const [isProcessingSwipe, setIsProcessingSwipe] = useState(false); // 🔥 БЛОКИРОВКА ПОВТОРНЫХ СВАЙПОВ

  const cardRef = useRef<HTMLDivElement>(null);
  const startX = useRef(0);
  const currentX = useRef(0);

  // 🔥 ФУНКЦИЯ ПРЕДЗАГРУЗКИ СЛЕДУЮЩЕЙ ИДЕИ
  const preloadNextIdea = async (filters?: IdeaFilters) => {
    try {
      const idea = await api.getRandomIdea(filters);
      setNextIdea(idea);
    } catch (error) {
      console.error('Error preloading idea:', error);
      setNextIdea(null);
    }
  };

  const fetchRandomIdea = async (filters?: IdeaFilters) => {
    try {
      setLoading(true);

      // 🔥 Используем предзагруженную идею если есть
      if (nextIdea) {
        setCurrentIdea(nextIdea);
        setNextIdea(null);
        setSwipeDirection(null);

        // Предзагружаем следующую идею на фоне
        preloadNextIdea(filters);
      } else {
        const idea = await api.getRandomIdea(filters);
        setCurrentIdea(idea);
        setSwipeDirection(null);

        // Предзагружаем следующую идею на фоне
        preloadNextIdea(filters);
      }
    } catch (error) {
      console.error('Error fetching idea:', error);
      setCurrentIdea(null);
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
    if (!currentIdea || isProcessingSwipe) return;

    setIsProcessingSwipe(true);
    setSwipeDirection(direction);

    try {
      // Параллельные промисы
      const promises: Promise<any>[] = [
        api.voteForIdea({
          ideaId: currentIdea.id,
          isLike: direction === 'right'
        })
      ];

      // Если лайк - добавляем создание приключения
      if (direction === 'right') {
        promises.push(
          (async () => {
            try {
              const canAdd = await api.canCreateFromIdea(currentIdea.id);
              if (canAdd) {
                return await api.createAdventureFromIdea(currentIdea.id);
              }
              return null;
            } catch (err) {
              console.error('Error creating adventure:', err);
              return null;
            }
          })()
        );
      }

      // Ждем завершения всех операций
      await Promise.all(promises);

    } catch (error) {
      console.error('Error processing swipe:', error);
    } finally {
      setTimeout(() => {
        fetchRandomIdea(activeFilters);
        setIsProcessingSwipe(false);
      }, 100);
    }
  };

  // 🔥 ОПТИМИЗИРОВАННЫЕ ОБРАБОТЧИКИ СВАЙПА
  const handleTouchStart = (e: React.TouchEvent) => {
    if (isProcessingSwipe) return; // 🔥 БЛОКИРОВКА ВО ВРЕМЯ ОБРАБОТКИ
    setIsSwiping(true);
    startX.current = e.touches[0].clientX;
    currentX.current = startX.current;
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current || isProcessingSwipe) return;
    const touch = e.touches[0];
    currentX.current = touch.clientX;
    const offsetX = currentX.current - startX.current;
    const rotate = offsetX * 0.1;
    cardRef.current.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;

    // 🔥 ВИЗУАЛЬНЫЙ ФИДБЭК ДЛЯ ПОЛЬЗОВАТЕЛЯ
    if (offsetX > 50) {
      cardRef.current.style.boxShadow = '0 10px 30px rgba(39, 174, 96, 0.3)';
    } else if (offsetX < -50) {
      cardRef.current.style.boxShadow = '0 10px 30px rgba(255, 107, 107, 0.3)';
    } else {
      cardRef.current.style.boxShadow = '0 10px 30px rgba(0, 0, 0, 0.1)';
    }
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (!isSwiping || !cardRef.current || isProcessingSwipe) return;
    const offsetX = currentX.current - startX.current;
    cardRef.current.style.transform = '';
    cardRef.current.style.boxShadow = '0 10px 30px rgba(0, 0, 0, 0.1)';
    setIsSwiping(false);

    // 🔥 УМЕНЬШИЛИ ПОРОГ ДЛЯ БОЛЕЕ ОТЗЫВЧИВОГО СВАЙПА
    if (offsetX > 80) handleSwipe('right');
    else if (offsetX < -80) handleSwipe('left');
  };

  const handleMouseDown = (e: React.MouseEvent) => {
    if (isProcessingSwipe) return;
    setIsSwiping(true);
    startX.current = e.clientX;
    currentX.current = startX.current;
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current || isProcessingSwipe) return;
    currentX.current = e.clientX;
    const offsetX = currentX.current - startX.current;
    const rotate = offsetX * 0.1;
    cardRef.current.style.transform = `translateX(${offsetX}px) rotate(${rotate}deg)`;

    if (offsetX > 50) {
      cardRef.current.style.boxShadow = '0 10px 30px rgba(39, 174, 96, 0.3)';
    } else if (offsetX < -50) {
      cardRef.current.style.boxShadow = '0 10px 30px rgba(255, 107, 107, 0.3)';
    }
  };

  const handleMouseUp = (e: React.MouseEvent) => {
    if (!isSwiping || !cardRef.current || isProcessingSwipe) return;
    const offsetX = currentX.current - startX.current;
    cardRef.current.style.transform = '';
    cardRef.current.style.boxShadow = '0 10px 30px rgba(0, 0, 0, 0.1)';
    setIsSwiping(false);
    if (offsetX > 80) handleSwipe('right');
    else if (offsetX < -80) handleSwipe('left');
  };

  useEffect(() => {
    const handleGlobalMouseUp = () => {
      if (cardRef.current) {
        cardRef.current.style.transform = '';
        cardRef.current.style.boxShadow = '0 10px 30px rgba(0, 0, 0, 0.1)';
      }
      setIsSwiping(false);
    };

    document.addEventListener('mouseup', handleGlobalMouseUp);
    return () => document.removeEventListener('mouseup', handleGlobalMouseUp);
  }, []);

  useEffect(() => {
    fetchRandomIdea(activeFilters);
  }, []);

  if (loading && !currentIdea) {
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
          disabled={isProcessingSwipe} // 🔥 БЛОКИРОВКА ВО ВРЕМЯ СВАЙПА
        >
          Фильтры
          {Object.keys(activeFilters).length > 0 && (
            <span className="active-filters-count">
              {Object.keys(activeFilters).length}
            </span>
          )}
        </button>

        {Object.keys(activeFilters).length > 0 && (
          <button
            className="reset-filters-btn"
            onClick={handleResetFilters}
            disabled={isProcessingSwipe}
          >
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
              disabled={isProcessingSwipe}
            >
              <option value="">Все категории</option>
              {FILTER_OPTIONS.categories.map(cat => (
                <option key={cat} value={cat}>{cat}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label>Локация</label>
            <select
              value={activeFilters.location || ''}
              onChange={(e) => handleFilterChange('location', e.target.value)}
              disabled={isProcessingSwipe}
            >
              <option value="">Все локации</option>
              {FILTER_OPTIONS.locations.map(loc => (
                <option key={loc} value={loc}>{loc}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label>Настроение</label>
            <select
              value={activeFilters.mood || ''}
              onChange={(e) => handleFilterChange('mood', e.target.value)}
              disabled={isProcessingSwipe}
            >
              <option value="">Любое настроение</option>
              {FILTER_OPTIONS.moods.map(mood => (
                <option key={mood} value={mood}>{mood}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label>Погода</label>
            <select
              value={activeFilters.weather || ''}
              onChange={(e) => handleFilterChange('weather', e.target.value)}
              disabled={isProcessingSwipe}
            >
              <option value="">Любая погода</option>
              {FILTER_OPTIONS.weather.map(weather => (
                <option key={weather} value={weather}>{weather}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label>Бюджет</label>
            <select
              value={activeFilters.priceCategory || ''}
              onChange={(e) => handleFilterChange('priceCategory', e.target.value ? parseInt(e.target.value) : null)}
              disabled={isProcessingSwipe}
            >
              <option value="">Любой бюджет</option>
              {FILTER_OPTIONS.priceCategories.map(price => (
                <option key={price.value} value={price.value}>{price.label}</option>
              ))}
            </select>
          </div>
        </div>
      )}

      {/* 🔥 ИНДИКАТОР ЗАГРУЗКИ ПОВЕРХ КАРТОЧКИ */}
      {isProcessingSwipe && (
        <div className="swipe-processing-overlay">
          <div className="spinner"></div>
        </div>
      )}

      {/* Карточка идеи */}
      <div
        ref={cardRef}
        className={`idea-card-wrapper ${swipeDirection ? `swipe-${swipeDirection}` : ''} ${isProcessingSwipe ? 'processing' : ''}`}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        style={{ pointerEvents: isProcessingSwipe ? 'none' : 'auto' }}
      >
        <IdeaCard idea={currentIdea} onSwipe={handleSwipe} />
      </div>

      {/* 🔥 ИНДИКАТОР ПРЕДЗАГРУЗКИ */}
      {nextIdea && !loading && (
        <div className="preload-indicator">
          Следующая идея готова ✓
        </div>
      )}
    </div>
  );
};

export default SwipeContainer;