import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import './Profile.css';

const Profile: React.FC = () => {
  const [profile, setProfile] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [showJoinForm, setShowJoinForm] = useState(false);
  const [joinCode, setJoinCode] = useState('');
  const [editingName, setEditingName] = useState(false);
  const [userName, setUserName] = useState('');

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      const profileData = await api.getProfile();
      setProfile(profileData);
    } catch (error) {
      console.error('Error loading profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCouple = async () => {
    try {
      const result = await api.createCouple();
      if (result.success) {
        await loadProfile();
      } else {
        alert(result.message || 'Ошибка при создании пары');
      }
    } catch (error) {
      console.error('Error creating couple:', error);
      alert('Ошибка при создании пары');
    }
  };

  const handleJoinCouple = async () => {
    if (!joinCode.trim()) {
      alert('Введите код приглашения');
      return;
    }
    
    try {
      const result = await api.joinCouple(joinCode);
      if (result.success) {
        setShowJoinForm(false);
        setJoinCode('');
        await loadProfile();
      } else {
        alert(result.message || 'Ошибка при присоединении к паре');
      }
    } catch (error) {
      console.error('Error joining couple:', error);
      alert('Ошибка при присоединении к паре');
    }
  };

  const handleUpdateName = async () => {
    if (!userName.trim()) {
      alert('Введите имя');
      return;
    }
    
    try {
      const result = await api.updateProfile({ name: userName });
      if (result.success) {
        setEditingName(false);
        await loadProfile();
      } else {
        alert(result.message || 'Ошибка при обновлении имени');
      }
    } catch (error) {
      console.error('Error updating name:', error);
      alert('Ошибка при обновлении имени');
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    window.location.reload();
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('ru-RU');
  };

  if (loading) {
    return <div className="loading">Загрузка профиля...</div>;
  }

  if (!profile || !profile.success) {
    return (
      <div className="error">
        <p>Ошибка загрузки профиля</p>
        <button onClick={loadProfile}>Повторить</button>
      </div>
    );
  }

  return (
    <div className="profile">
      <div className="profile-header">
        <h1>👤 Профиль пары</h1>
      </div>

      <div className="profile-content">
        {/* Информация о пользователе */}
        <div className="profile-section">
          <h2>Ваш профиль</h2>
          <div className="user-info">
            <div className="avatar">
              {profile.user.avatar ? (
                <img src={profile.user.avatar} alt="Avatar" />
              ) : (
                <div className="avatar-placeholder">
                  {profile.user.name.charAt(0).toUpperCase()}
                </div>
              )}
            </div>
            <div className="user-details">
              {editingName ? (
                <div className="edit-name">
                  <input
                    type="text"
                    value={userName}
                    onChange={(e) => setUserName(e.target.value)}
                    placeholder="Введите имя"
                  />
                  <button onClick={handleUpdateName}>✓</button>
                  <button onClick={() => setEditingName(false)}>✕</button>
                </div>
              ) : (
                <h3 onClick={() => {
                  setUserName(profile.user.name);
                  setEditingName(true);
                }}>
                  {profile.user.name} ✏️
                </h3>
              )}
              <p>{profile.user.email}</p>
              <small>С {formatDate(profile.user.createdAt)}</small>
            </div>
          </div>
        </div>

        {/* Информация о паре */}
        <div className="profile-section">
          <h2>Ваша пара</h2>
          {profile.couple ? (
            <div className="couple-info">
              <div className="couple-header">
                <h3>{profile.couple.name}</h3>
                <div className="join-code">
                  <strong>Код приглашения:</strong>
                  <span className="code">{profile.couple.joinCode}</span>
                  <button 
                    className="copy-btn"
                    onClick={() => {
                      navigator.clipboard.writeText(profile.couple.joinCode);
                      alert('Код скопирован!');
                    }}
                  >
                    📋
                  </button>
                </div>
              </div>
              
              <div className="partners">
                <h4>Участники:</h4>
                {profile.partners.map((partner: any) => (
                  <div key={partner.id} className="partner">
                    <span className="partner-name">{partner.name}</span>
                    <span className={`partner-role ${partner.role}`}>
                      {partner.role === 'creator' ? '👑 Создатель' : '👥 Участник'}
                    </span>
                    <span className="partner-joined">
                      с {formatDate(partner.joinedAt)}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="no-couple">
              <p>Вы еще не создали пару</p>
              <button onClick={handleCreateCouple} className="create-couple-btn">
                🎉 Создать пару
              </button>
              
              <div className="join-section">
                <p>Или присоединитесь к существующей паре:</p>
                {showJoinForm ? (
                  <div className="join-form">
                    <input
                      type="text"
                      value={joinCode}
                      onChange={(e) => setJoinCode(e.target.value.toUpperCase())}
                      placeholder="Введите код приглашения"
                      maxLength={6}
                    />
                    <button onClick={handleJoinCouple}>Присоединиться</button>
                    <button onClick={() => setShowJoinForm(false)}>Отмена</button>
                  </div>
                ) : (
                  <button onClick={() => setShowJoinForm(true)} className="join-btn">
                    🔗 Присоединиться к паре
                  </button>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Действия */}
        <div className="profile-actions">
          <button onClick={handleLogout} className="logout-btn">
            🚪 Выйти из аккаунта
          </button>
        </div>
      </div>
    </div>
  );
};

export default Profile;