// src/components/CameraGrid.jsx
export default function CameraGrid() {
  const cameras = ["Camera 1", "Camera 2", "Camera 3", "Camera 4"];

  return (
    <div className="grid grid-cols-2 gap-4">
      {cameras.map((label, idx) => (
        <div
          key={idx}
          className="h-40 flex items-center justify-center border rounded bg-gray-100 text-gray-600"
        >
          {label} (placeholder)
        </div>
      ))}
    </div>
  );
}
