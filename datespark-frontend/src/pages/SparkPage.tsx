import React from 'react';
import SwipeContainer from '../components/SwipeContainer'; // 🔥 убрал ../ если SparkPage в pages
import './SparkPage.css';

const SparkPage: React.FC = () => {
  return (
    <div className="spark-page">
      <header className="spark-header">
        <h1>DateSpark</h1>
        <p>Найдите идеальное свидание!</p>
      </header>
      
      <main className="spark-main">
        <SwipeContainer />
      </main>

      {/* 🔥 УБИРАЕМ ФУТЕР - теперь подсказки свайпов не нужны */}
    </div>
  );
};

export default SparkPage;