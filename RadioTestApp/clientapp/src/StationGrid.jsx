import { useState, useEffect } from 'react';

export default function StationGrid({ title, apiUrl, activeId, onPlay }) {
  const [stations, setStations] = useState(null);
  const [error, setError] = useState(false);

  useEffect(() => {
    setStations(null);
    setError(false);
    fetch(apiUrl)
      .then(r => r.json())
      .then(setStations)
      .catch(() => setError(true));
  }, [apiUrl]);

  return (
    <>
      <h1>{title}</h1>
      <ul className="station-grid">
        {error && <li className="loading">Failed to load stations.</li>}
        {!stations && !error && <li className="loading">Loading stations...</li>}
        {stations && stations.map(s => (
          <li key={s.id} className={s.id === activeId ? 'active' : ''} onClick={() => onPlay(s)}>
            {s.logo
              ? <img src={s.logo} alt="" onError={e => { e.target.outerHTML = '<div class="placeholder">📻</div>'; }} />
              : <div className="placeholder">📻</div>}
            <span>{s.name}</span>
          </li>
        ))}
      </ul>
    </>
  );
}
