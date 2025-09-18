import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
} from "recharts";

const COLORS = ["#34d399", "#fbbf24", "#f87171", "#60a5fa"];

const AnalyticsSection = ({ products = [], users = [] }) => {
  // --- Quick Metrics ---
  const totalProducts = products.length;
  const totalUsers = users.length;
  const activeProducts = products.filter((p) => p.status === "Active").length;
  const lowStockProducts = products.filter((p) => p.status === "Low Stock").length;
  const inactiveProducts = products.filter((p) => p.status === "Inactive").length;

  // Pie chart: Product Status
  const productStatusData = [
    { name: "Active", value: activeProducts },
    { name: "Low Stock", value: lowStockProducts },
    { name: "Inactive", value: inactiveProducts },
  ];

  // Bar chart: Products per Category
  const categoryCount = products.reduce((acc, p) => {
    acc[p.category] = (acc[p.category] || 0) + 1;
    return acc;
  }, {});
  const categoryData = Object.keys(categoryCount).map((cat) => ({
    category: cat,
    count: categoryCount[cat],
  }));

  // User Insights
  const activeUsers = users.filter((u) => u.status === "Active").length;
  const pendingUsers = users.filter((u) => u.status === "Pending").length;

  return (
    <section className="bg-white rounded-lg shadow-md p-6 mb-8">
      <h2 className="text-2xl font-bold mb-4"> Analytics Overview</h2>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        <div className="bg-green-100 p-4 rounded-lg text-center">
          <p className="text-sm text-green-700">Total Products</p>
          <h3 className="text-2xl font-bold text-green-800">{totalProducts}</h3>
        </div>
        <div className="bg-blue-100 p-4 rounded-lg text-center">
          <p className="text-sm text-blue-700">Total Users</p>
          <h3 className="text-2xl font-bold text-blue-800">{totalUsers}</h3>
        </div>
        <div className="bg-yellow-100 p-4 rounded-lg text-center">
          <p className="text-sm text-yellow-700">Low Stock Items</p>
          <h3 className="text-2xl font-bold text-yellow-800">{lowStockProducts}</h3>
        </div>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Pie Chart: Product Status */}
        <div className="bg-gray-50 p-4 rounded-lg shadow">
          <h4 className="text-lg font-semibold text-center mb-4">Product Status</h4>
          <ResponsiveContainer width="100%" height={250}>
            <PieChart>
              <Pie
                data={productStatusData}
                dataKey="value"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={80}
                label
              >
                {productStatusData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>

        {/* Bar Chart: Products per Category */}
        <div className="bg-gray-50 p-4 rounded-lg shadow">
          <h4 className="text-lg font-semibold text-center mb-4">Products by Category</h4>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={categoryData}>
              <XAxis dataKey="category" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="count" fill="#60a5fa" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* User Insights */}
      <div className="mt-8 text-center">
        <p className="text-gray-700">
          Active users:{" "}
          <span className="font-semibold text-green-600">{activeUsers}</span> | Pending
          users:{" "}
          <span className="font-semibold text-yellow-600">{pendingUsers}</span>
        </p>
      </div>
    </section>
  );
};

export default AnalyticsSection;
