import React from 'react';
import { Idea } from '../types';
import './IdeaCard.css';

interface IdeaCardProps {
  idea: Idea;
  onSwipe: (direction: 'left' | 'right') => void;
}

const IdeaCard: React.FC<IdeaCardProps> = ({ idea, onSwipe }) => {
  const getPriceSymbol = (priceCategory: number): string => {
    switch (priceCategory) {
      case 1: return '$';
      case 2: return '$$';
      case 3: return '$$$';
      default: return '$$';
    }
  };

  const getPriceHint = (priceCategory: number): string => {
    switch (priceCategory) {
      case 1: return 'Дешево/бесплатно';
      case 2: return 'Средняя цена';
      case 3: return 'Дорого';
      default: return 'Средняя цена';
    }
  };

  const priceSymbol = getPriceSymbol(idea.priceCategory);
  const priceHint = getPriceHint(idea.priceCategory);

  return (
    <div className="idea-card">
      <div className="card-header">
        <h2 className="card-title">{idea.title}</h2>
      </div>

      <div className="card-category-wrapper">
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
            <span className="detail-label">🌤️</span>
            {idea.weather}
          </div>
          <div className="detail-item price-item">
            <span className="detail-label">💰</span>
            <span className="price-category">
              {priceSymbol}
              <span className="price-hint">({priceHint})</span>
            </span>
          </div>
        </div>
      </div>

      <div className="idea-card-actions">
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