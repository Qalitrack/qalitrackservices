import { useMemo, useState } from "react";
import ReportsTable from "../ReportsTable";
import ReportsPagination from "../ReportsPagination";
import { ChevronLeft } from "lucide-react";

export default function CustomerReport({ transactions = [], loading }) {
  const [selectedCustomer, setSelectedCustomer] = useState(null);

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
  // KPIs
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
    <div className="bg-white border rounded-lg p-4 space-y-4">
      {/* KPIs */}
      {!selectedCustomer && (
        <div className="grid grid-cols-3 gap-3">
          <CompactStat label="Customers" value={totalCustomers} />
          <CompactStat label="Transactions" value={totalTransactions} />
          <CompactStat
            label="Net Weight"
            value={totalWeight.toLocaleString()}
          />
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
            compact
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
        <>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setSelectedCustomer(null)}
              className="flex items-center gap-1 text-xs border rounded px-2 py-1"
            >
              <ChevronLeft size={14} /> Back
            </button>

            <h3 className="text-sm font-semibold">
              {selectedCustomer} — Loads
            </h3>
          </div>

          <ReportsTable
            transactions={customerRows}
            loading={loading}
            showColumns={["destinationName", "commodityName", "netWeight"]}
            compact
          />
        </>
      )}
    </div>
  );
}

function CompactStat({ label, value }) {
  return (
    <div className="bg-yellow-50 border rounded px-3 py-2">
      <p className="text-[11px] text-gray-500 uppercase">{label}</p>
      <p className="text-lg font-semibold leading-tight">{value}</p>
    </div>
  );
}
