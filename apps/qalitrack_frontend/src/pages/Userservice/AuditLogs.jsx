import React, { useState, useEffect } from 'react';
import { ScrollText } from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { fetchAuditLogs } from '../../api/helpers/UserService/AuditLogs/auditLogs.js';
import TablePagination from '../../components/TablePagination';
import PageHeader from '../../components/PageHeader.jsx';

const METHOD_STYLES = {
    POST: 'bg-green-100 text-green-700 border-green-300',
    PUT: 'bg-blue-100 text-blue-700 border-blue-300',
    PATCH: 'bg-blue-100 text-blue-700 border-blue-300',
    DELETE: 'bg-red-100 text-red-700 border-red-300',
};

const statusStyle = (status) => {
    if (status >= 200 && status < 300) return 'bg-green-100 text-green-700 border-green-300';
    if (status >= 400 && status < 500) return 'bg-amber-100 text-amber-700 border-amber-300';
    if (status >= 500) return 'bg-red-100 text-red-700 border-red-300';
    return 'bg-gray-100 text-gray-700 border-gray-300';
};

const AuditLogs = () => {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(20);
    const [totalCount, setTotalCount] = useState(0);

    const loadLogs = async (pageNumber) => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchAuditLogs(pageNumber, pageSize);
            setLogs(data.items);
            setTotalCount(data.totalCount);
        } catch (err) {
            setError(err.message || 'Failed to fetch audit logs.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadLogs(page);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page]);

    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <div className="shrink-0 flex items-center justify-between gap-3 px-3 py-2 border-b border-gray-200 bg-gray-50">
                <p className="text-[11px] text-gray-500">Every create/update/delete request captured at the gateway — read-only traffic isn't logged</p>
                <button
                    onClick={() => loadLogs(page)}
                    className="h-7 px-3 text-xs font-semibold rounded border border-amber-300 text-amber-700 bg-white hover:bg-amber-50 transition-colors shrink-0"
                >
                    Refresh
                </button>
            </div>

            {error && (
                <div className="mx-4 mt-3 px-4 py-2 rounded-md text-sm font-medium border bg-red-50 border-red-200 text-red-700">
                    {error}
                </div>
            )}

            {/* Table */}
            <div className="flex-1 overflow-auto bg-white">
                {loading && logs.length === 0 ? (
                    <div className="flex items-center justify-center h-full">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div>
                    </div>
                ) : logs.length === 0 ? (
                    <div className="flex items-center justify-center h-full text-gray-500 text-sm">No audit logs found.</div>
                ) : (
                    <>
                        <table className="w-full compact-table">
                            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                                <tr>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Timestamp</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Method</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Path</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Status</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">User</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">IP Address</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-right uppercase tracking-wide">Duration</th>
                                </tr>
                            </thead>
                            <tbody>
                                {logs.map((log, index) => (
                                    <tr
                                        key={log.id}
                                        className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                                            index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
                                        }`}
                                    >
                                        <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">{(page - 1) * pageSize + index + 1}</td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600 whitespace-nowrap">
                                            {log.createdAt ? format(parseISO(log.createdAt), 'dd MMM HH:mm:ss') : '-'}
                                        </td>
                                        <td className="px-3 py-2 text-center">
                                            <span className={`inline-block px-2 py-0.5 rounded-full text-[9px] font-semibold border ${METHOD_STYLES[log.method] || 'bg-gray-100 text-gray-700 border-gray-300'}`}>
                                                {log.method}
                                            </span>
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-800 font-mono">
                                            {log.path}{log.queryString || ''}
                                        </td>
                                        <td className="px-3 py-2 text-center">
                                            <span className={`inline-block px-2 py-0.5 rounded-full text-[9px] font-semibold border ${statusStyle(log.statusCode)}`}>
                                                {log.statusCode}
                                            </span>
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-700">
                                            {log.userName || <span className="text-gray-400">Anonymous</span>}
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600 font-mono">{log.ipAddress || '-'}</td>
                                        <td className="px-3 py-2 text-[10px] text-right text-gray-600">{log.durationMs}ms</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>

                        {/* Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                        <TablePagination
                            page={page}
                            totalPages={totalPages}
                            onPageChange={setPage}
                            itemCount={totalCount}
                            itemLabel="log entries total"
                        />
                    </>
                )}
            </div>

            <style>{`
                .compact-table {
                  font-size: 10px;
                }
                .compact-table thead tr th {
                  padding: 6px 12px;
                  font-weight: 700;
                  font-size: 9px;
                  line-height: 1.2;
                }
                .compact-table tbody tr td {
                  padding: 6px 12px;
                  line-height: 1.3;
                }
            `}</style>
        </div>
    );
};

export default AuditLogs;
