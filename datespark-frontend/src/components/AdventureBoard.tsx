// components/AdventureBoard.tsx
import React from 'react';
import './AdventureBoard.css';

const AdventureBoard: React.FC = () => {
  return (
    <div className="adventure-board">
      <h1>📋 Adventure Board</h1>
      <p>Скоро здесь будет канбан-доска для планирования свиданий!</p>
      {/* Позже добавим канбан-доску с колонками */}
    </div>
  );
};

export default AdventureBoard;