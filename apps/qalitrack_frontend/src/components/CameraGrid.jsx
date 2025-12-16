// src/components/CameraGrid.jsx
import React from'react';
import {CAMERA_CONFIG}from'../config';

const {cameraId,streamUrl,snapshotUrl,platesUrl}=CAMERA_CONFIG;

// ────────────────────── LIVE STREAM CARD ──────────────────────
const LiveStreamCard=()=>{
  const [key,setKey]=React.useState(0);
  const handleError=()=>setKey(k=>k+1);

  return(
    <div className="relative bg-black rounded-3xl overflow-hidden shadow-2xl">
      <div className="aspect-video bg-gray-950">
        <img
          key={key}
          src={`${streamUrl}?t=${Date.now()}`}
          alt="Live Stream"
          className="w-full h-full object-cover"
          onError={handleError}
        />
        <div className="absolute top-4 left-4 bg-amber-500 text-black px-6 py-3 rounded-full text-xl font-black animate-pulse">
          LIVE
        </div>
      </div>
      <div className="p-4 text-center">
        <h3 className="text-xl font-bold text-amber-400">Continuous Stream</h3>
      </div>
    </div>
  );
};

// ────────────────────── SNAPSHOT CARD ──────────────────────
const SnapshotCard=()=>{
  const [tick,setTick]=React.useState(0);

  React.useEffect(()=>{
    const id=setInterval(()=>setTick(t=>t+1),1500);
    return()=>clearInterval(id);
  },[]);

  return(
    <div className="relative bg-black rounded-3xl overflow-hidden shadow-2xl">
      <div className="aspect-video bg-gray-950">
        <img
          src={`${snapshotUrl}?t=${tick}`}
          alt="Latest Snapshot"
          className="w-full h-full object-cover"
        />
        <div className="absolute top-4 left-4 bg-green-500 text-black px-6 py-3 rounded-full text-xl font-black">
          SNAPSHOT
        </div>
      </div>
      <div className="p-4 text-center">
        <h3 className="text-xl font-bold text-green-400">Latest Frame</h3>
        <p className="text-xs text-gray-500">Every 1.5s</p>
      </div>
    </div>
  );
};

// ────────────────────── PLATE CARD ──────────────────────
const PlateCard=()=>{
  const [plate,setPlate]=React.useState(null);

  React.useEffect(()=>{
    // Define the async function properly
    const fetchPlate=async()=>{
      try{
        const res=await fetch(platesUrl,{cache:'no-store'});
        if(!res.ok)return;

        const data=await res.json();

        let latest=null;
        if(Array.isArray(data)){
          latest=data
            .filter(p=>p.cameraId===cameraId)
            .sort((a,b)=>new Date(b.timestamp)-new Date(a.timestamp))[0]||null;
        }else if(data&&data.cameraId===cameraId){
          latest=data;
        }

        setPlate(latest);
      }catch(e){
        // silent fail – will retry
      }
    };

    fetchPlate();                    // initial call
    const id=setInterval(fetchPlate,2000);

    return()=>clearInterval(id);
  },[cameraId,platesUrl]); // dependencies are stable anyway

  return(
    <div className="relative bg-black rounded-3xl overflow-hidden shadow-2xl flex flex-col">
      <div className="aspect-video bg-gradient-to-br from-purple-900 to-black flex items-center justify-center">
        {plate?(
          <div className="text-center">
            <div className="text-6xl md:text-8xl font-black text-amber-500 tracking-widest font-mono drop-shadow-2xl">
              {plate.plateNumber||plate.plate}
            </div>
            <div className="text-xl text-amber-300 mt-3">
              {(plate.confidence*100).toFixed(1)}% confidence
            </div>
          </div>
        ):(
          <div className="text-2xl text-gray-600">Waiting for vehicle...</div>
        )}
        <div className="absolute top-4 left-4 bg-purple-600 text-white px-6 py-3 rounded-full text-xl font-black">
          PLATE
        </div>
      </div>
      <div className="p-6 text-center bg-gradient-to-t from-purple-950 to-transparent">
        <p className="text-gray-400">
          {plate?new Date(plate.timestamp).toLocaleTimeString():'Real-time ANPR'}
        </p>
      </div>
    </div>
  );
};

// ────────────────────── MAIN COMPONENT – PERFECT FOR 5-COL TABLE CELL ──────────────────────
export default function CameraGrid(){
  return(
    <div className="w-full h-full flex flex-col min-w-0">
      
      <h2 className="text-3xl md:text-4xl font-black text-center text-transparent bg-clip-text bg-gradient-to-r from-amber-500 to-pink-500 mb-6 tracking-wider shrink-0">
        NPR1 • LIVE
      </h2>

      {/* Stacks nicely in narrow columns */}
      <div className="space-y-4">
        <LiveStreamCard/>
        <SnapshotCard/>
        <PlateCard/>
      </div>
    </div>
  );
}