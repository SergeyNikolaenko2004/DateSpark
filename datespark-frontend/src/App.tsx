import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [apiMessage, setApiMessage] = useState<string>('');

  useEffect(() => {
    // Временно укажи локальный URL бэкенда. Позже заменим на переменную окружения.
    fetch('https://localhost:5138/api/test')
      .then(response => response.json())
      .then(data => setApiMessage(data.message))
      .catch(error => setApiMessage('Failed to connect to API: ' + error.message));
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>💝 DateSpark</h1>
        <p>Старт нашего проекта!</p>
        <p>Сообщение от бэкенда: <strong>{apiMessage}</strong></p>
      </header>
    </div>
  );
}

export default App;