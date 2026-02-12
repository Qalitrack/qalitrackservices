export default function ReportsPagination({
  currentPage,
  totalRecords,
  pageSize,
  onPageChange,
}) {
  const totalPages = Math.ceil(totalRecords / pageSize);

  if (totalPages <= 1) return null;

  const goToPage = (page) => {
    if (page < 1 || page > totalPages) return;
    onPageChange(page);
  };

  // Only show a window of pages
  const getVisiblePages = () => {
    const pages = [];
    const maxVisible = 5;

    let start = Math.max(1, currentPage - 2);
    let end = Math.min(totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  };

  return (
    <div className="bg-amber-50/50 border-t border-amber-200 px-3 py-3">
      <div className="flex flex-col sm:flex-row items-center justify-between gap-3">
        {/* Page Info */}
        <p className="text-xs font-medium text-gray-700 whitespace-nowrap">
          Page <span className="font-bold text-amber-700">{currentPage}</span> of{" "}
          <span className="font-bold">{totalPages}</span>
        </p>

        {/* Controls */}
        <div className="flex flex-wrap items-center justify-center gap-2">
          <button
            onClick={() => goToPage(currentPage - 1)}
            disabled={currentPage === 1}
            className="border border-amber-200 px-3 py-1 rounded-lg text-xs font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors bg-white"
          >
            Previous
          </button>

          {getVisiblePages().map((page) => {
            const isActive = page === currentPage;

            return (
              <button
                key={page}
                onClick={() => goToPage(page)}
                className={`px-3 py-1 rounded-lg min-w-[32px] text-xs font-medium transition-all ${
                  isActive
                    ? "bg-gradient-to-r from-amber-500 to-amber-600 border border-amber-500 text-white shadow-sm"
                    : "bg-white border border-amber-200 hover:bg-amber-50 hover:border-amber-300"
                }`}
              >
                {page}
              </button>
            );
          })}

          <button
            onClick={() => goToPage(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="border border-amber-200 px-3 py-1 rounded-lg text-xs font-medium hover:bg-amber-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors bg-white"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}