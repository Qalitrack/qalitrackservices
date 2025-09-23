'use client';

import { MapContainer, TileLayer, Marker, Popup, Tooltip, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { useEffect } from 'react';

// Coordinates for QSL Centre
const position: [number, number] = [-1.359227, 36.937984];

// Red bouncing pin
const redIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.7.1/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
  className: 'marker-bounce',
});

// Fix map disappearing on zoom
function FixMapSize() {
  const map = useMap();
  useEffect(() => {
    setTimeout(() => map.invalidateSize(), 100);
  }, [map]);
  return null;
}

// Zoom buttons
function ZoomButtons() {
  const map = useMap();
  useEffect(() => {
    L.control.zoom({ position: 'bottomright' }).addTo(map);
  }, [map]);
  return null;
}

interface ContactMapProps {
  isMobile: boolean;
}

export default function ContactMap({ isMobile }: ContactMapProps) {
  const navigateUrl = `https://www.google.com/maps/dir/?api=1&destination=${position[0]},${position[1]}`;
  
  return (
    <div className="h-[500px] md:h-[500px] rounded-2xl shadow-lg overflow-hidden border-2 border-amber-400">
      <MapContainer center={position} zoom={17} scrollWheelZoom={!isMobile} zoomControl={false} className="h-full w-full">
        <FixMapSize />
        <TileLayer
          url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
          attribution='© <a href="https://www.openstreetmap.org/copyright">OSM</a> © <a href="https://carto.com/">CARTO</a>'
        />
        <Marker position={position} icon={redIcon}>
          <Tooltip permanent direction="top" offset={[0, -10]} opacity={1}>
            <span className="font-semibold text-amber-600">
              Qalibrated Systems Limited
            </span>
          </Tooltip>
          <Popup>
            <div className="p-3 rounded-lg shadow-md border border-gray-200 bg-white">
              <h3 className="text-amber-500 font-bold mb-1">
                📍 Qalibrated Systems Limited
              </h3>
              <p className="text-gray-700 text-sm mb-3">
                QSL Centre, Mombasa Road, Nairobi
              </p>
              <a
                href={navigateUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-block bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold py-1 px-3 rounded shadow transition"
              >
                🚗 Navigate
              </a>
            </div>
          </Popup>
        </Marker>
        <ZoomButtons />
      </MapContainer>

      {/* Bouncing pin animation */}
      <style jsx>{`
        .marker-bounce {
          animation: bounce 1.5s infinite;
        }
        @keyframes bounce {
          0%, 20%, 50%, 80%, 100% { transform: translateY(0); }
          40% { transform: translateY(-8px); }
          60% { transform: translateY(-4px); }
        }
      `}</style>
    </div>
  );
}