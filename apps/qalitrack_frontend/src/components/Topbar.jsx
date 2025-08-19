export default function Topbar(){
  return (
    <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-4">
      <div className="font-medium">Home / Dashboard</div>
      <div className="flex items-center gap-3">
        <input className="border rounded px-3 py-1 h-9" placeholder="Search"/>
        <button className="h-9 px-3 border rounded">Alerts</button>
        <div className="h-9 px-3 border rounded flex items-center">User</div>
      </div>
    </header>
  );
}
