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

  // 🔹 Only show a window of pages
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
    <div className="sticky bottom-0 bg-white border-t mt-6 pt-4 pb-3 px-2">
      <div className="flex flex-col sm:flex-row items-center justify-between gap-3 max-w-full">
        {/* Page Info */}
        <p className="text-sm text-gray-600 whitespace-nowrap">
          Page <span className="font-medium">{currentPage}</span> of{" "}
          <span className="font-medium">{totalPages}</span>
        </p>

        {/* Controls */}
        <div className="flex flex-wrap items-center justify-center gap-2">
          <button
            onClick={() => goToPage(currentPage - 1)}
            disabled={currentPage === 1}
            className="px-3 py-1 border rounded disabled:opacity-40"
          >
            Prev
          </button>

          {getVisiblePages().map((page) => {
            const isActive = page === currentPage;

            return (
              <button
                key={page}
                onClick={() => goToPage(page)}
                className={`px-3 py-1 border rounded min-w-[36px] ${
                  isActive
                    ? "bg-yellow-400 border-yellow-400 font-medium"
                    : "bg-white"
                }`}
              >
                {page}
              </button>
            );
          })}

          <button
            onClick={() => goToPage(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="px-3 py-1 border rounded disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  );
}
