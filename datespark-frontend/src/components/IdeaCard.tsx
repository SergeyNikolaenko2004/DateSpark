import React from 'react';
import { Idea } from '../types';
import './IdeaCard.css';

interface IdeaCardProps {
  idea: Idea;
  onSwipe: (direction: 'left' | 'right') => void;
}

const IdeaCard: React.FC<IdeaCardProps> = ({ idea, onSwipe }) => {
  // Функция для отображения подсказки по цене
  const getPriceHint = (priceCategory: string) => {
    switch (priceCategory) {
      case '$': return 'Дешево/бесплатно';
      case '$$': return 'Средняя цена';
      case '$$$': return 'Дорого';
      default: return 'Средняя цена';
    }
  };

  return (
    <div className="idea-card">
      <div className="card-header">
        <h2 className="card-title">{idea.title}</h2>
        <span className="card-category">{idea.category}</span>
      </div>
      
      <div className="card-content">
        <p className="card-description">{idea.description}</p>
        
        <div className="card-details">
          <div className="detail-item">
            <span className="detail-label">📍</span>
            {idea.location}
          </div>
          <div className="detail-item">
            <span className="detail-label">💖</span>
            {idea.mood}
          </div>
          <div className="detail-item">
            <span className="detail-label">⏱️</span>
            {idea.duration}
          </div>
          <div className="detail-item">
            <span className="detail-label">🌤️</span>
            {idea.weather}
          </div>
          <div className="detail-item price-item">
            <span className="detail-label">💰</span>
            <span className="price-category">
              {idea.priceCategory}
              <span className="price-hint">({getPriceHint(idea.priceCategory)})</span>
            </span>
          </div>
        </div>
      </div>

      <div className="card-actions">
        <button 
          className="btn-dislike"
          onClick={() => onSwipe('left')}
        >
          ❌
        </button>
        <button 
          className="btn-like"
          onClick={() => onSwipe('right')}
        >
          ❤️
        </button>
      </div>
    </div>
  );
};

export default IdeaCard;