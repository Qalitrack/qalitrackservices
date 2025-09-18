const DataTable = ({ type, data, loading, onCreate, onEdit, onDelete }) => {
  const headers =
    type === "products"
      ? ["Image", "Name", "Category", "Stock", "Price", "Status", "Description"]
      : ["Name", "Email", "Role", "Status"];

  return (
    <section className="bg-white rounded-lg shadow p-6">
      {/* Top Bar */}
      <div className="flex flex-col sm:flex-row justify-between mb-4 gap-4">
        <input
          type="text"
          placeholder={`Search ${type}...`}
          className="border rounded-md px-3 py-2 w-full sm:w-auto"
        />
        <button
          onClick={onCreate}
          className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-md"
        >
          + Create {type.slice(0, -1)}
        </button>
      </div>

      {/* Loading */}
      {loading && <p className="text-center text-gray-500">Loading...</p>}

      {/* Table */}
      {!loading && (
        <div className="overflow-x-auto">
          <table className="min-w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-100">
                {headers.map((h) => (
                  <th key={h} className="px-4 py-2 text-left font-semibold">
                    {h}
                  </th>
                ))}
                <th className="px-4 py-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.length ? (
                data.map((item) => (
                  <tr key={item._id} className="border-b hover:bg-gray-50">
                    {type === "products" ? (
                      <>
                        <td className="px-4 py-2">
                          <img
                            src={item.image}
                            alt={item.name}
                            className="w-12 h-12 object-cover rounded"
                          />
                        </td>
                        <td className="px-4 py-2">{item.name}</td>
                        <td className="px-4 py-2">{item.category}</td>
                        <td className="px-4 py-2">{item.stock}</td>
                        <td className="px-4 py-2">{item.price}</td>
                        <td className="px-4 py-2">{item.status}</td>
                        <td className="px-4 py-2">{item.description}</td>
                      </>
                    ) : (
                      <>
                        <td className="px-4 py-2">{item.name}</td>
                        <td className="px-4 py-2">{item.email}</td>
                        <td className="px-4 py-2">{item.role}</td>
                        <td className="px-4 py-2">{item.status}</td>
                      </>
                    )}
                    <td className="px-4 py-2 space-x-2">
                      <button
                        onClick={() => onEdit(item)}
                        className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => onDelete(item._id)}
                        className="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={headers.length + 1} className="text-center py-4">
                    No {type} found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
};

export default DataTable;
