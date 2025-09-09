const DashboardHeader = ({ title, subtitle, error }) => (
  <div className="bg-white rounded-lg shadow-md p-6 mb-6 text-center">
    <h1 className="text-3xl font-bold">{title}</h1>
    <p className="text-gray-600">{subtitle}</p>
    {error && <p className="text-red-500 mt-2">{error}</p>}
  </div>
);
export default DashboardHeader;
