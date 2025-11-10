import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [apiMessage, setApiMessage] = useState<string>('');

  useEffect(() => {
    // ЗАМЕНИ НА РАБОЧИЙ URL:
    fetch('https://datespark-api.onrender.com/api/test')
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