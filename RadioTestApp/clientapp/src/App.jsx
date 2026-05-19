import { Routes, Route, NavLink, Navigate } from 'react-router-dom';
import { useState, useRef } from 'react';
import StationGrid from './StationGrid';

export default function App() {
  const [nowPlaying, setNowPlaying] = useState(null);
  const audioRef = useRef(null);

  const play = (station) => {
    setNowPlaying(station);
    const audio = audioRef.current;
    if (audio) {
      audio.src = station.stream;
      audio.play();
    }
  };

  return (
    <>
      <nav>
        <NavLink to="/egypt">🇪🇬 Egyptian</NavLink>
        <NavLink to="/rock">🎸 Rock</NavLink>
        <NavLink to="/classic">🎻 Classic</NavLink>
      </nav>
      <div className="content">
        <div className="player-bar">
          <div className="now-playing">
            {nowPlaying ? `▶ ${nowPlaying.name}` : 'Select a station to play'}
          </div>
          <audio ref={audioRef} controls />
        </div>
        <Routes>
          <Route path="/" element={<Navigate to="/egypt" replace />} />
          <Route path="/egypt" element={<StationGrid title="Egyptian Radio" apiUrl="/api/stations/egypt" activeId={nowPlaying?.id} onPlay={play} />} />
          <Route path="/rock" element={<StationGrid title="Rock Radio" apiUrl="/api/stations/rock" activeId={nowPlaying?.id} onPlay={play} />} />
          <Route path="/classic" element={<StationGrid title="Classic Radio" apiUrl="/api/stations/classic" activeId={nowPlaying?.id} onPlay={play} />} />
        </Routes>
      </div>
    </>
  );
}
