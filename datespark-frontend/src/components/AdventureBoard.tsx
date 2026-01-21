import React, { useState, useEffect } from 'react';
import { AdventureCard, AdventureStatus } from '../types';
import { api } from '../services/api';
import './AdventureBoard.css';

const AdventureBoard: React.FC = () => {
  const [adventures, setAdventures] = useState<AdventureCard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');

  // Статусы для колонок
  const statuses = [
    { status: AdventureStatus.Liked, title: '💡 Лайкнутые', symbol: '💡' },
    { status: AdventureStatus.Planned, title: '📅 Запланированные', symbol: '📅' },
    { status: AdventureStatus.InProgress, title: '🚀 В процессе', symbol: '🚀' },
    { status: AdventureStatus.Completed, title: '✅ Выполненные', symbol: '✅' }
  ];

  useEffect(() => {
    loadAdventures();
  }, []);

  const loadAdventures = async () => {
    try {
      setLoading(true);
      const data = await api.getCoupleAdventures();
      setAdventures(data);
      setError(null);
    } catch (err: any) {
      setError('Не удалось загрузить доску приключений');
      console.error('Error loading adventures:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddManual = async () => {
    if (!newTitle.trim()) {
      alert('Введите название');
      return;
    }

    try {
      await api.createAdventureManual(newTitle, newDescription);
      setNewTitle('');
      setNewDescription('');
      setShowAddForm(false);
      await loadAdventures();
      alert('Приключение добавлено!');
    } catch (err: any) {
      alert('Ошибка при добавлении: ' + err.message);
    }
  };

  const handleUpdateStatus = async (adventureId: number, newStatus: AdventureStatus) => {
    try {
      await api.updateAdventureStatus(adventureId, newStatus);
      await loadAdventures();
    } catch (err: any) {
      alert('Ошибка при обновлении статуса: ' + err.message);
    }
  };

  const handleDelete = async (adventureId: number) => {
    if (!window.confirm('Удалить это приключение?')) return;

    try {
      await api.deleteAdventure(adventureId);
      await loadAdventures();
    } catch (err: any) {
      alert('Ошибка при удалении: ' + err.message);
    }
  };

  const getAdventuresByStatus = (status: AdventureStatus) => {
    return adventures.filter(adventure => adventure.status === status);
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Не указано';
    return new Date(dateString).toLocaleDateString('ru-RU');
  };

  const getNextStatus = (currentStatus: AdventureStatus): AdventureStatus => {
    switch (currentStatus) {
      case AdventureStatus.Liked: return AdventureStatus.Planned;
      case AdventureStatus.Planned: return AdventureStatus.InProgress;
      case AdventureStatus.InProgress: return AdventureStatus.Completed;
      case AdventureStatus.Completed: return AdventureStatus.Liked;
      default: return AdventureStatus.Liked;
    }
  };

  const getStatusButtonText = (currentStatus: AdventureStatus): string => {
    switch (currentStatus) {
      case AdventureStatus.Liked: return '📅 Запланировать';
      case AdventureStatus.Planned: return '🚀 Начать';
      case AdventureStatus.InProgress: return '✅ Завершить';
      case AdventureStatus.Completed: return '💡 Вернуть';
      default: return 'Далее';
    }
  };

  if (loading) {
    return (
      <div className="adventure-board-page">
        <div className="loading">Загрузка доски приключений...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="adventure-board-page">
        <div className="error">
          <p>{error}</p>
          <button onClick={loadAdventures}>Повторить</button>
        </div>
      </div>
    );
  }

  return (
    <div className="adventure-board-page">
      <header className="adventure-header">
        <h1>📋 Доска приключений</h1>
        <p>Планируйте и отслеживайте ваши свидания вместе</p>
      </header>
      
      <main className="adventure-main">
        <div className="board-actions">
          <button 
            className="add-adventure-btn"
            onClick={() => setShowAddForm(!showAddForm)}
          >
            {showAddForm ? '✕ Отмена' : '✎ Добавить приключение'}
          </button>
        </div>

        {showAddForm && (
          <div className="add-adventure-form">
            <input
              type="text"
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              placeholder="Название приключения*"
              maxLength={100}
            />
            <textarea
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              placeholder="Описание (необязательно)"
              rows={3}
            />
            <div className="form-buttons">
              <button onClick={handleAddManual} className="submit-btn">
                ✎ Добавить
              </button>
              <button onClick={() => setShowAddForm(false)} className="cancel-btn">
                ✕ Отмена
              </button>
            </div>
          </div>
        )}

        <div className="kanban-board">
          {statuses.map((column) => (
            <div key={column.status} className="kanban-column">
              <div className="column-header">
                <h3>{column.title}</h3>
                <span className="count-badge">
                  {getAdventuresByStatus(column.status).length}
                </span>
              </div>
              
              <div className="cards-container">
                {getAdventuresByStatus(column.status).map((adventure) => (
                  <div key={adventure.id} className="adventure-card">
                    <div className="card-header">
                      <h4>{adventure.title}</h4>
                      {adventure.createdByUserName && (
                        <span className="creator-badge">
                          от {adventure.createdByUserName}
                        </span>
                      )}
                    </div>
                    
                    {adventure.description && (
                      <p className="card-description">{adventure.description}</p>
                    )}
                    
                    {adventure.plannedDate && column.status !== AdventureStatus.Completed && (
                      <div className="card-date">
                        <span className="date-label">📅 Запланировано:</span>
                        <span>{formatDate(adventure.plannedDate)}</span>
                      </div>
                    )}
                    
                    {adventure.completedDate && column.status === AdventureStatus.Completed && (
                      <div className="card-date">
                        <span className="date-label">✅ Выполнено:</span>
                        <span>{formatDate(adventure.completedDate)}</span>
                      </div>
                    )}
                    
                    {adventure.notes && (
                      <div className="card-notes">
                        <span className="notes-label">💬 Заметки:</span>
                        <p>{adventure.notes}</p>
                      </div>
                    )}
                    
                    <div className="card-actions">
                      <button
                        onClick={() => handleUpdateStatus(adventure.id, getNextStatus(adventure.status))}
                        className="status-btn"
                      >
                        {getStatusButtonText(adventure.status)}
                      </button>
                      
                      {adventure.status !== AdventureStatus.Completed && adventure.plannedDate && (
                        <button
                          onClick={() => handleUpdateStatus(adventure.id, AdventureStatus.InProgress)}
                          className="progress-btn"
                        >
                          🚀 Начать сейчас
                        </button>
                      )}
                      
                      <button
                        onClick={() => handleDelete(adventure.id)}
                        className="delete-btn"
                      >
                        ✕ Удалить
                      </button>
                    </div>
                    
                    {adventure.ideaId && (
                      <div className="card-footer">
                        <span className="idea-source">Из лайкнутой идеи</span>
                      </div>
                    )}
                  </div>
                ))}
                
                {getAdventuresByStatus(column.status).length === 0 && (
                  <div className="empty-column">
                    <p>Пока ничего нет</p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
};

export default AdventureBoard;