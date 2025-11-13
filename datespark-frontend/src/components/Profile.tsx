// components/Profile.tsx
import React from 'react';
import './Profile.css';

const Profile: React.FC = () => {
  const handleLogout = () => {
    localStorage.removeItem('authToken');
    window.location.reload();
  };

  return (
    <div className="profile">
      <h1>👤 Профиль пары</h1>
      <div className="profile-info">
        <p>Здесь будет информация о вашей паре</p>
        <button className="logout-btn" onClick={handleLogout}>
          Выйти
        </button>
      </div>
    </div>
  );
};

export default Profile;