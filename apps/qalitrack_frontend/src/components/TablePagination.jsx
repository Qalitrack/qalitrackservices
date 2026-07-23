export default function TablePagination({ page, totalPages, onPageChange, itemCount, itemLabel }) {
  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
      <span className="text-[10px] text-gray-600 font-medium">
        Page <span className="font-semibold text-amber-600">{page}</span> of{" "}
        <span className="font-semibold text-amber-600">{totalPages}</span>
        {itemCount != null && (
          <>
            {" • "}
            <span className="font-semibold text-amber-600">{itemCount}</span> {itemLabel}
          </>
        )}
      </span>
      <div className="flex items-center gap-1">
        <button
          onClick={() => onPageChange(Math.max(1, page - 1))}
          disabled={page === 1}
          className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
        >
          Previous
        </button>
        {pages.map((n) => (
          <button
            key={n}
            onClick={() => onPageChange(n)}
            className={`w-6 h-6 text-[11px] font-semibold rounded transition-all ${
              n === page
                ? "bg-gradient-to-br from-amber-500 to-amber-600 text-white shadow-sm"
                : "border border-gray-300 hover:border-amber-500 hover:text-amber-600"
            }`}
          >
            {n}
          </button>
        ))}
        <button
          onClick={() => onPageChange(Math.min(totalPages, page + 1))}
          disabled={page === totalPages}
          className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
        >
          Next
        </button>
      </div>
    </div>
  );
}
