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
  const [hasCouple, setHasCouple] = useState<boolean | null>(null);
  const [profile, setProfile] = useState<any>(null);
  const [collapsedColumns, setCollapsedColumns] = useState<Record<AdventureStatus, boolean>>({
    [AdventureStatus.Liked]: false,
    [AdventureStatus.Planned]: false,
    [AdventureStatus.InProgress]: false,
    [AdventureStatus.Completed]: false
  });

  const statuses = [
    { status: AdventureStatus.Liked, title: 'Лайкнутые', symbol: '💡' },
    { status: AdventureStatus.Planned, title: 'Запланированные', symbol: '📅' },
    { status: AdventureStatus.InProgress, title: 'В процессе', symbol: '🚀' },
    { status: AdventureStatus.Completed, title: 'Выполненные', symbol: '✅' }
  ];

  useEffect(() => {
    loadProfileAndAdventures();
  }, []);

  const loadProfileAndAdventures = async () => {
    try {
      setLoading(true);
      const profileData = await api.getProfile();
      setProfile(profileData);

      if (!profileData.couple) {
        setHasCouple(false);
        setAdventures([]);
        setError('У вас нет пары для использования доски свиданий');
        return;
      }

      setHasCouple(true);
      const adventuresData = await api.getCoupleAdventures();
      setAdventures(adventuresData);
      setError(null);
    } catch {
      setHasCouple(false);
      setError('Не удалось загрузить доску свиданий');
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
      await loadProfileAndAdventures();
      alert('Приключение добавлено!');
    } catch {
      alert('Ошибка при добавлении приключения');
    }
  };

  const handleUpdateStatus = async (adventureId: number, newStatus: AdventureStatus) => {
    try {
      await api.updateAdventureStatus(adventureId, newStatus);
      await loadProfileAndAdventures();
    } catch {
      alert('Ошибка при обновлении статуса');
    }
  };

  const handleDelete = async (adventureId: number) => {
    try {
      await api.deleteAdventure(adventureId);
      await loadProfileAndAdventures();
    } catch {
      alert('Ошибка при удалении');
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
      case AdventureStatus.Liked: return 'Запланировать';
      case AdventureStatus.Planned: return 'Начать';
      case AdventureStatus.InProgress: return 'Завершить';
      case AdventureStatus.Completed: return 'Вернуть';
      default: return 'Далее';
    }
  };

  const toggleColumn = (status: AdventureStatus) => {
    setCollapsedColumns(prev => ({
      ...prev,
      [status]: !prev[status]
    }));
  };

  if (loading) {
    return (
      <div className="adventure-board-page">
        <div className="loading">Загрузка доски свиданий...</div>
      </div>
    );
  }

  if (hasCouple === false) {
    return (
      <div className="adventure-board-page">
        <header className="adventure-header">
          <h1>Доска свиданий</h1>
          <p>Планируйте и отслеживайте ваши свидания вместе</p>
        </header>

        <main className="adventure-main">
          <div className="no-couple-message">
            <div className="message-content">
              <h3>У вас еще нет пары</h3>
              <p>Чтобы использовать доску приключений, нужно создать пару или присоединиться к существующей</p>

              <div className="couple-actions">
                <button
                  className="create-couple-btn"
                  onClick={() => window.location.href = '/profile'}
                >
                  Перейти в профиль
                </button>

                <p className="or-text">или</p>

                <div className="quick-actions">
                  <button
                    className="quick-create-btn"
                    onClick={async () => {
                      try {
                        await api.createCouple('Наша пара');
                        alert('Пара создана! Теперь можно использовать доску свиданий.');
                        await loadProfileAndAdventures();
                      } catch {
                        alert('Ошибка при создании пары');
                      }
                    }}
                  >
                    Быстро создать пару
                  </button>

                  <button
                    className="join-couple-btn"
                    onClick={() => {
                      const joinCode = prompt('Введите код приглашения (6 символов):');
                      if (joinCode && joinCode.trim()) {
                        api.joinCouple(joinCode.trim())
                          .then(() => {
                            alert('Вы присоединились к паре!');
                            loadProfileAndAdventures();
                          })
                          .catch(() => alert('Ошибка при присоединении'));
                      }
                    }}
                  >
                    👥 Присоединиться по коду
                  </button>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    );
  }

  if (error && hasCouple === true) {
    return (
      <div className="adventure-board-page">
        <div className="error">
          <p>{error}</p>
          <button onClick={loadProfileAndAdventures}>Повторить</button>
        </div>
      </div>
    );
  }

  return (
    <div className="adventure-board-page">
      <header className="adventure-header">
        <h1>Доска свиданий</h1>
        <p>Планируйте и отслеживайте ваши свидания вместе</p>
        {profile?.couple && (
          <div className="couple-info-banner">
            Пара: <strong>{profile.couple.name}</strong> | Код: <code>{profile.couple.joinCode}</code>
          </div>
        )}
      </header>

      <main className="adventure-main">
        <div className="board-actions">
          <button
            className="add-adventure-btn"
            onClick={() => setShowAddForm(!showAddForm)}
          >
            {showAddForm ? '✕ Отмена' : '✎ Добавить свидание'}
          </button>
        </div>

        {showAddForm && (
          <div className="add-adventure-form">
            <input
              type="text"
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              placeholder="Название свидания*"
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
            <div key={column.status} className={`kanban-column ${collapsedColumns[column.status] ? 'collapsed' : 'expanded'}`}>
              <div className="column-header">
                <button 
                  className="column-toggle"
                  onClick={() => toggleColumn(column.status)}
                  aria-label={collapsedColumns[column.status] ? 'Развернуть колонку' : 'Свернуть колонку'}
                >
                  <h3>{column.title}</h3>
                  <span className="toggle-icon">
                    {collapsedColumns[column.status] ? '▶' : '▼'}
                  </span>
                </button>
                <div className="column-header-right">
                  <span className="count-badge">
                    {getAdventuresByStatus(column.status).length}
                  </span>
                </div>
              </div>

              {!collapsedColumns[column.status] && (
                <div className="cards-container">
                  {getAdventuresByStatus(column.status).map((adventure) => (
                    <div key={adventure.id} className="adventure-card">
                      <div className="card-header">
                        <h4>{adventure.title}</h4>
                        {adventure.createdByUserName && adventure.createdByUserName !== profile?.user?.name && (
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
                          <span className="date-label">Запланировано:</span>
                          <span>{formatDate(adventure.plannedDate)}</span>
                        </div>
                      )}

                      {adventure.completedDate && column.status === AdventureStatus.Completed && (
                        <div className="card-date">
                          <span className="date-label">Выполнено:</span>
                          <span>{formatDate(adventure.completedDate)}</span>
                        </div>
                      )}

                      {adventure.notes && (
                        <div className="card-notes">
                          <span className="notes-label">Заметки:</span>
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
                            Начать сейчас
                          </button>
                        )}

                        <button
                          onClick={() => handleDelete(adventure.id)}
                          className="delete-btn"
                        >
                          Удалить
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
              )}
            </div>
          ))}
        </div>
      </main>
    </div>
  );
};

export default AdventureBoard;