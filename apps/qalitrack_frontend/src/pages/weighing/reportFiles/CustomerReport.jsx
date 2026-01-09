import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function CustomerReport({ transactions = [], loading }) {
  const [selectedCustomer, setSelectedCustomer] = useState(null);

  // Internal pagination
  const [currentPage, setCurrentPage] = useState(1);
  const PAGE_SIZE = 5;

  // =========================
  // Summary
  // =========================
  const customerSummary = useMemo(() => {
    const map = {};

    transactions.forEach((tx) => {
      const customer = tx.destinationName || "Unknown Customer";

      if (!map[customer]) {
        map[customer] = {
          id: customer,
          destinationName: customer,
          count: 0,
          netWeight: 0,
        };
      }

      map[customer].count += 1;
      map[customer].netWeight += Number(tx.netWeight) || 0;
    });

    return Object.values(map);
  }, [transactions]);

  // =========================
  // KPIs (NOT PAGINATED)
  // =========================
  const totalCustomers = customerSummary.length;
  const totalTransactions = customerSummary.reduce((s, c) => s + c.count, 0);
  const totalWeight = customerSummary.reduce((s, c) => s + c.netWeight, 0);

  // =========================
  // Paginated summary
  // =========================
  const paginatedSummary = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return customerSummary.slice(start, start + PAGE_SIZE);
  }, [customerSummary, currentPage]);

  // =========================
  // Drill-down rows
  // =========================
  const customerRows = useMemo(() => {
    if (!selectedCustomer) return [];
    return transactions
      .filter(
        (t) => (t.destinationName || "Unknown Customer") === selectedCustomer
      )
      .map((t) => ({
        id: t.id,
        destinationName: t.destinationName || "-",
        commodityName: t.commodityName || "-",
        netWeight: t.netWeight || 0,
      }));
  }, [selectedCustomer, transactions]);

  return (
    <div className="bg-white border rounded-lg p-6">
      {/* HEADER */}
      <div className="mb-6">
        <h2 className="text-2xl font-semibold">Customer Report</h2>
        <p className="text-sm text-gray-600">
          Weight breakdown per customer
        </p>
      </div>

      {/* KPIs */}
      {!selectedCustomer && (
        <div className="flex gap-6 mb-6 flex-wrap">
          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Customers</p>
            <p className="text-xl font-semibold">{totalCustomers}</p>
          </div>

          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Transactions</p>
            <p className="text-xl font-semibold">{totalTransactions}</p>
          </div>

          <div className="bg-yellow-100 p-4 rounded shadow flex-1 min-w-[150px]">
            <p className="text-gray-500 text-sm">Total Net Weight (kg)</p>
            <p className="text-xl font-semibold">{totalWeight.toFixed(2)}</p>
          </div>
        </div>
      )}

      {/* SUMMARY TABLE */}
      {!selectedCustomer && (
        <>
          <ReportsTable
            transactions={paginatedSummary}
            loading={loading}
            showColumns={["destinationName", "netWeight"]}
            onRowClick={(row) => setSelectedCustomer(row.destinationName)}
          />

          <ReportsPagination
            currentPage={currentPage}
            totalRecords={customerSummary.length}
            pageSize={PAGE_SIZE}
            onPageChange={setCurrentPage}
          />
        </>
      )}

      {/* DRILL-DOWN */}
      {selectedCustomer && (
        <div>
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setSelectedCustomer(null)}
              className="flex items-center gap-1 text-sm border rounded px-3 py-1"
            >
              <ChevronLeft size={16} /> Back
            </button>

            <h3 className="text-xl font-semibold">
              {selectedCustomer} — Loads
            </h3>
          </div>

          <ReportsTable
            transactions={customerRows}
            loading={loading}
            showColumns={["destinationName", "commodityName", "netWeight"]}
          />
        </div>
      )}
    </div>
  );
}
