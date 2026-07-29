import React from 'react';

export default function Loader({ text = 'Loading...' }) {
  return (
    <div className="loading-screen">
      <div className="loading-pulse">
        <svg width="64px" height="48px">
          <polyline points="0.157 23.954, 14 23.954, 21.843 48, 43 0, 50 24, 64 24" id="back" />
          <polyline points="0.157 23.954, 14 23.954, 21.843 48, 43 0, 50 24, 64 24" id="front" />
        </svg>
      </div>
      {text && <p className="loader-text">{text}</p>}
    </div>
  );
}
