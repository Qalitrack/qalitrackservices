import { useState, useMemo } from "react";
import { Ban, Printer } from "lucide-react";
import jsPDF from "jspdf";

export default function TransactionList({ transactions, onDeactivate, onComplete }) {
  const [statusFilter, setStatusFilter] = useState("all");
  const [typeFilter, setTypeFilter] = useState("all");
  const [searchQuery, setSearchQuery] = useState("");

  const printTicket = (tx) => {
    const doc = new jsPDF();
    doc.text("Weighing Ticket", 20, 20);
    doc.text(`Plate: ${tx.plate}`, 20, 40);
    doc.text(`Driver: ${tx.driver}`, 20, 50);
    doc.text(`Order ID: ${tx.orderId}`, 20, 60);
    doc.text(`Type: ${tx.type}`, 20, 70);
    doc.text(`Weight 1: ${tx.w1} kg`, 20, 80);
    doc.text(`Weight 2: ${tx.w2 ?? "-"} kg`, 20, 90);
    doc.text(
      `Net: ${tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "-"} kg`,
      20,
      100
    );
    doc.text(`Date: ${new Date(tx.date).toLocaleString()}`, 20, 110);
    doc.save(`ticket_${tx.plate}_${tx.id}.pdf`);
  };

  // Apply filters
  const filteredTransactions = useMemo(() => {
    return transactions.filter((tx) => {
      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "inqueue" && !tx.w2 && !tx.deactivated) ||
        (statusFilter === "completed" && tx.w2) ||
        (statusFilter === "deactivated" && tx.deactivated);

      const matchesType =
        typeFilter === "all" || tx.type === typeFilter;

      const matchesSearch =
        !searchQuery ||
        tx.plate?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        tx.orderId?.toLowerCase().includes(searchQuery.toLowerCase());

      return matchesStatus && matchesType && matchesSearch;
    });
  }, [transactions, statusFilter, typeFilter, searchQuery]);

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600 mb-4">
        Transactions
      </h2>

      {/* Filters */}
      <div className="flex flex-wrap gap-2 mb-4">
        <input
          type="text"
          placeholder="Search Plate / Order ID"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="border rounded px-3 py-2"
        >
          <option value="all">All Statuses</option>
          <option value="inqueue">In Queue</option>
          <option value="completed">Completed</option>
          <option value="deactivated">Deactivated</option>
        </select>
        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value)}
          className="border rounded px-3 py-2"
        >
          <option value="all">All Types</option>
          <option value="inbound">Inbound</option>
          <option value="outbound">Outbound</option>
        </select>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full border">
          <thead className="bg-gray-100 text-left">
            <tr>
              <th className="px-3 py-2 border">Plate</th>
              <th className="px-3 py-2 border">Type</th>
              <th className="px-3 py-2 border">W1</th>
              <th className="px-3 py-2 border">W2</th>
              <th className="px-3 py-2 border">Net</th>
              <th className="px-3 py-2 border">Status</th>
              <th className="px-3 py-2 border">Date</th>
              <th className="px-3 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredTransactions.map((tx) => (
              <tr key={tx.id} className="hover:bg-gray-50">
                <td className="px-3 py-2 border">{tx.plate}</td>
                <td className="px-3 py-2 border">{tx.type}</td>
                <td className="px-3 py-2 border">{tx.w1}</td>
                <td className="px-3 py-2 border">{tx.w2 ?? "-"}</td>
                <td className="px-3 py-2 border">
                  {tx.w1 && tx.w2 ? tx.w1 - tx.w2 : "-"}
                </td>
                <td className="px-3 py-2 border">
                  {tx.deactivated
                    ? "Deactivated"
                    : tx.w2
                    ? "Completed"
                    : "In Queue"}
                </td>
                <td className="px-3 py-2 border">
                  {new Date(tx.date).toLocaleString()}
                </td>
                <td className="px-3 py-2 border space-x-2">
                  {!tx.w2 && !tx.deactivated && (
                    <button
                      className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                      onClick={() => onComplete(tx.id, 10800)}
                    >
                      Complete
                    </button>
                  )}
                  {!tx.deactivated && (
                    <button
                      className="px-3 py-1 bg-yellow-500 text-white rounded hover:bg-yellow-600 flex items-center gap-1"
                      onClick={() => onDeactivate(tx.id)}
                    >
                      <Ban size={14} /> Deactivate
                    </button>
                  )}
                  {tx.w2 && (
                    <button
                      className="px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-1"
                      onClick={() => printTicket(tx)}
                    >
                      <Printer size={14} /> Ticket
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {filteredTransactions.length === 0 && (
              <tr>
                <td
                  colSpan="8"
                  className="px-3 py-3 text-center text-gray-500"
                >
                  No transactions found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
