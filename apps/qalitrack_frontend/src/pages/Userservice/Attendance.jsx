import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { getAttendanceByInstanceId, getAllAttendance } from '../../api/helpers/UserService/Shifts/Attendance.js';
import { fetchShifts } from '../../api/helpers/UserService/Shifts/Shifts.js';
import { format } from 'date-fns';
import dayjs from 'dayjs';
import { ChevronLeftIcon, ChevronRightIcon, ChevronDoubleLeftIcon, ChevronDoubleRightIcon } from '@heroicons/react/20/solid';
import { FileDown, FileSpreadsheet } from 'lucide-react';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

// Helper function to format status
const getStatusText = (status) => {
    switch (status) {
        case 1: return 'Scheduled';
        case 2: return 'Present';
        case 3: return 'Absent';
        case 4: return 'Late';
        case 5: return 'Partial';
        default: return 'Unknown';
    }
};

// Helper function to get status class
const getStatusClass = (status) => {
    switch (status) {
        case 2: return 'bg-green-100 text-green-800';
        case 3: return 'bg-red-100 text-red-800';
        case 4: return 'bg-yellow-100 text-yellow-800';
        case 5: return 'bg-blue-100 text-blue-800';
        default: return 'bg-gray-100 text-gray-800';
    }
};

const formatBoolean = (value) => value ? 'Yes' : 'No';

const formatTime = (dateString) => {
    if (!dateString || isNaN(new Date(dateString))) return '--:-- --';
    return format(new Date(dateString), 'h:mm a');
};

const formatCreatedAt = (dateString) => {
    if (!dateString || isNaN(new Date(dateString))) return 'N/A';
    return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
};

// Shared branded PDF template — same house style as Shifts.jsx's report (logo,
// company header, colored badge, summary stat cards, watermark, footer) so every
// exported report in the app looks consistent instead of each page rolling its own.
const generateAttendancePdf = async ({ badgeTitle, subtitle, stats, rows, filename }) => {
    const settings = getTicketSettings();
    const companyName = settings.companyName || 'QALIBRATED SYSTEMS LTD';
    const companyAddr = settings.companyAddress || 'PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996';

    const doc = new jsPDF('landscape', 'mm', 'a4');
    const PW = doc.internal.pageSize.getWidth();
    const L = 14;
    const R = PW - 14;
    const TW = R - L;

    const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings);
    const black = [0, 0, 0];
    const gray = [107, 114, 128];
    const borderCol = [229, 231, 235];

    let circularLogo = null;
    try {
        const img = await new Promise((resolve, reject) => {
            const i = new Image();
            i.onload = () => resolve(i);
            i.onerror = reject;
            i.src = settings.companyLogo || logoSrc;
        });
        const sz = 200;
        const pad = sz * 0.06;
        const cv = document.createElement('canvas');
        cv.width = sz; cv.height = sz;
        const ctx = cv.getContext('2d');
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, sz, sz);
        const avail = sz - pad * 2;
        const aspect = img.naturalWidth / img.naturalHeight;
        const drawW = aspect >= 1 ? avail : avail * aspect;
        const drawH = aspect >= 1 ? avail / aspect : avail;
        ctx.drawImage(img, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH);
        circularLogo = cv.toDataURL('image/png');
    } catch (_) {}

    // Header
    if (circularLogo) doc.addImage(circularLogo, 'PNG', L, 5, 17, 17);
    doc.setFontSize(14); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
    doc.text(companyName, PW / 2, 11, { align: 'center' });
    doc.setFontSize(7.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
    doc.text(companyAddr, PW / 2, 16, { align: 'center' });

    // Badge
    const badgeW = 50;
    doc.setFillColor(...accent);
    doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, 'F');
    doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(...accentHeaderText);
    doc.text(badgeTitle, R - badgeW / 2, 9.5, { align: 'center' });
    doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
    doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, R, 16, { align: 'right' });

    if (subtitle) {
        doc.setFontSize(9); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
        doc.text(subtitle, L, 21);
    }

    // Amber divider
    doc.setDrawColor(...accent); doc.setLineWidth(0.8);
    doc.line(L, 23, R, 23);

    // Summary stats
    let y = 27;
    const statW = (TW - (stats.length - 1) * 4) / stats.length;
    stats.forEach((s, i) => {
        const bx = L + i * (statW + 4);
        doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
        doc.roundedRect(bx, y, statW, 10, 2, 2, 'FD');
        doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
        doc.text(s.label, bx + statW / 2, y + 3.8, { align: 'center' });
        doc.setFontSize(9); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
        doc.text(s.value, bx + statW / 2, y + 8.2, { align: 'center' });
    });
    y += 14;

    // Table
    autoTable(doc, {
        startY: y,
        margin: { left: L, right: L },
        head: [Object.keys(rows[0] || {})],
        body: rows.map((r) => Object.values(r)),
        styles: { fontSize: 7, cellPadding: 1.5, textColor: black, lineColor: borderCol },
        headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: 'bold', fontSize: 7.5, halign: 'center', lineColor: accentDark },
    });

    // Footer
    const footerY = doc.lastAutoTable.finalY + 4;
    doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
    doc.roundedRect(L, footerY, TW, 10, 2, 2, 'FD');
    if (circularLogo) doc.addImage(circularLogo, 'PNG', L + 2, footerY + 1, 8, 8);
    doc.setFontSize(7.5); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
    doc.text('Powered by Qalibrated Systems  |  www.qalibrated.co.ke', PW / 2, footerY + 5, { align: 'center' });
    doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
    doc.text('Inventing and Making Happen', PW / 2, footerY + 8.5, { align: 'center' });

    // Watermark on all pages
    if (circularLogo) {
        try {
            const wmSize = 90;
            const PH = doc.internal.pageSize.getHeight();
            const wmCanvas = document.createElement('canvas');
            wmCanvas.width = 200; wmCanvas.height = 200;
            const wmCtx = wmCanvas.getContext('2d');
            const wmImg = await new Promise((resolve, reject) => {
                const i = new Image(); i.onload = () => resolve(i); i.onerror = reject;
                i.src = circularLogo;
            });
            wmCtx.globalAlpha = 0.07;
            wmCtx.drawImage(wmImg, 0, 0, 200, 200);
            const wmData = wmCanvas.toDataURL('image/png');
            const totalPages = doc.internal.getNumberOfPages();
            for (let p = 1; p <= totalPages; p++) {
                doc.setPage(p);
                doc.addImage(wmData, 'PNG', PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
            }
        } catch (_) {}
    }

    doc.save(filename);
};

const PaginationControls = ({ pagination, onPageChange, onPageSizeChange, hasPreviousPage, hasNextPage, totalCount, totalPages }) => (
    <div className="flex items-center justify-between px-4 py-3 bg-white border-t border-gray-200 sm:px-6">
        <div className="flex-1 flex justify-between sm:hidden">
            <button
                onClick={() => onPageChange(pagination.pageNumber - 1)}
                disabled={!hasPreviousPage}
                className={`relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md ${
                    hasPreviousPage ? 'bg-white text-gray-700 hover:bg-gray-50' : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                }`}
            >
                Previous
            </button>
            <button
                onClick={() => onPageChange(pagination.pageNumber + 1)}
                disabled={!hasNextPage}
                className={`ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md ${
                    hasNextPage ? 'bg-white text-gray-700 hover:bg-gray-50' : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                }`}
            >
                Next
            </button>
        </div>
        <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
                {typeof totalCount === 'number' ? (
                    <p className="text-sm text-gray-700">
                        Showing <span className="font-medium">{(pagination.pageNumber - 1) * pagination.pageSize + 1}</span> to{' '}
                        <span className="font-medium">{Math.min(pagination.pageNumber * pagination.pageSize, totalCount)}</span>{' '}
                        of <span className="font-medium">{totalCount}</span> results
                    </p>
                ) : (
                    <p className="text-sm text-gray-700">Page <span className="font-medium">{pagination.pageNumber}</span></p>
                )}
            </div>
            <div className="flex items-center space-x-4">
                <div className="flex items-center">
                    <label htmlFor="page-size" className="mr-2 text-sm text-gray-700">
                        Rows per page:
                    </label>
                    <select
                        id="page-size"
                        value={pagination.pageSize}
                        onChange={onPageSizeChange}
                        className="block w-full rounded-md border border-gray-300 shadow-sm focus:border-amber-500 focus:ring-amber-500 sm:text-sm"
                    >
                        <option value={5}>5</option>
                        <option value={10}>10</option>
                        <option value={20}>20</option>
                        <option value={50}>50</option>
                    </select>
                </div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    {typeof totalPages === 'number' && (
                        <button
                            onClick={() => onPageChange(1)}
                            disabled={!hasPreviousPage}
                            className={`relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium ${
                                hasPreviousPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">First</span>
                            <ChevronDoubleLeftIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                    )}
                    <button
                        onClick={() => onPageChange(pagination.pageNumber - 1)}
                        disabled={!hasPreviousPage}
                        className={`relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium ${
                            hasPreviousPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                        } ${typeof totalPages !== 'number' ? 'rounded-l-md' : ''}`}
                    >
                        <span className="sr-only">Previous</span>
                        <ChevronLeftIcon className="h-5 w-5" aria-hidden="true" />
                    </button>
                    <div className="px-4 py-2 bg-white text-sm font-medium text-gray-700 border-t border-b border-gray-300">
                        Page {pagination.pageNumber}{typeof totalPages === 'number' ? ` of ${totalPages}` : ''}
                    </div>
                    <button
                        onClick={() => onPageChange(pagination.pageNumber + 1)}
                        disabled={!hasNextPage}
                        className={`relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium ${
                            hasNextPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                        } ${typeof totalPages !== 'number' ? 'rounded-r-md' : ''}`}
                    >
                        <span className="sr-only">Next</span>
                        <ChevronRightIcon className="h-5 w-5" aria-hidden="true" />
                    </button>
                    {typeof totalPages === 'number' && (
                        <button
                            onClick={() => onPageChange(totalPages)}
                            disabled={!hasNextPage}
                            className={`relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium ${
                                hasNextPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">Last</span>
                            <ChevronDoubleRightIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                    )}
                </nav>
            </div>
        </div>
    </div>
);

// Default view: pick a shift, see (and export) attendance for just that shift —
// not a mixed list across every shift. GetAll has no shift filter or total-count
// server-side, so we page through everything once per shift selection and filter/
// paginate in memory; that also gives accurate totals instead of Prev/Next-only.
const ShiftAttendanceView = () => {
    const [shifts, setShifts] = useState([]);
    const [selectedShiftId, setSelectedShiftId] = useState('');
    const [allRecords, setAllRecords] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [exporting, setExporting] = useState(false);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 20 });

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const data = await fetchShifts(1, 200);
                if (cancelled) return;
                const list = data.items || [];
                setShifts(list);
                if (list.length > 0) setSelectedShiftId(list[0].id);
                else setLoading(false);
            } catch (err) {
                if (!cancelled) { setError('Failed to load shifts.'); setLoading(false); }
            }
        })();
        return () => { cancelled = true; };
    }, []);

    useEffect(() => {
        if (!selectedShiftId) return;
        let cancelled = false;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                let all = [];
                let pageNumber = 1;
                const pageSize = 100;
                let hasMore = true;
                while (hasMore) {
                    const batch = await getAllAttendance({ pageNumber, pageSize });
                    all = all.concat(batch);
                    hasMore = batch.length === pageSize;
                    pageNumber++;
                }
                if (cancelled) return;
                setAllRecords(all.filter((r) => r.shiftId === selectedShiftId));
                setPagination((prev) => ({ ...prev, pageNumber: 1 }));
            } catch (err) {
                if (!cancelled) setError('Failed to load attendance data. Please try again later.');
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();
        return () => { cancelled = true; };
    }, [selectedShiftId]);

    const totalCount = allRecords.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / pagination.pageSize));
    const pageItems = allRecords.slice(
        (pagination.pageNumber - 1) * pagination.pageSize,
        pagination.pageNumber * pagination.pageSize
    );

    const handlePageChange = (newPage) => {
        if (newPage >= 1 && newPage <= totalPages) setPagination((prev) => ({ ...prev, pageNumber: newPage }));
    };

    const handlePageSizeChange = (e) => {
        setPagination({ pageNumber: 1, pageSize: parseInt(e.target.value) });
    };

    const selectedShiftName = shifts.find((s) => s.id === selectedShiftId)?.name || 'Shift';

    const buildExportRows = (records) => records.map((record) => ({
        'Employee Name': record.employeeName || 'Unknown',
        'Employee Email': record.employeeEmail || 'N/A',
        'Clock In': formatTime(record.clockInTime),
        'Clock Out': formatTime(record.clockOutTime),
        'Status': getStatusText(record.status),
        'Late': formatBoolean(record.isLate),
        'Created At': formatCreatedAt(record.createdAt),
    }));

    const exportPDF = async () => {
        if (allRecords.length === 0) { alert('No attendance data to export.'); return; }
        setExporting(true);
        try {
            const presentCount = allRecords.filter((r) => r.status === 2).length;
            const lateCount = allRecords.filter((r) => r.isLate).length;
            const sanitizedShiftName = selectedShiftName.replace(/[^a-z0-9]/gi, '-').toLowerCase();
            await generateAttendancePdf({
                badgeTitle: 'ATTENDANCE REPORT',
                subtitle: selectedShiftName,
                stats: [
                    { label: 'TOTAL RECORDS', value: `${allRecords.length}` },
                    { label: 'PRESENT', value: `${presentCount}` },
                    { label: 'LATE', value: `${lateCount}` },
                ],
                rows: buildExportRows(allRecords),
                filename: `attendance-${sanitizedShiftName}-${dayjs().format('YYYY-MM-DD')}.pdf`,
            });
        } catch (err) {
            alert('Failed to generate PDF. Please try again.');
        } finally {
            setExporting(false);
        }
    };

    const exportExcel = () => {
        if (allRecords.length === 0) { alert('No attendance data to export.'); return; }
        setExporting(true);
        try {
            const ws = XLSX.utils.json_to_sheet(buildExportRows(allRecords));
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Attendance');
            const sanitizedShiftName = selectedShiftName.replace(/[^a-z0-9]/gi, '-').toLowerCase();
            XLSX.writeFile(wb, `attendance-${sanitizedShiftName}-${dayjs().format('YYYY-MM-DD')}.xlsx`);
        } catch (err) {
            alert('Failed to generate Excel file. Please try again.');
        } finally {
            setExporting(false);
        }
    };

    if (error) {
        return (
            <div className="p-6">
                <div className="bg-red-50 border-l-4 border-red-400 p-4">
                    <p className="text-sm text-red-700">{error}</p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-6">
            <div className="bg-white shadow overflow-hidden sm:rounded-lg">
                <div className="px-4 py-5 sm:px-6 border-b border-gray-200 flex flex-wrap items-start justify-between gap-4">
                    <div>
                        <h3 className="text-lg leading-6 font-medium text-gray-900">Shift Attendance</h3>
                        <p className="mt-1 text-sm text-gray-500">Pick a shift to see its attendance and export a report scoped to it.</p>
                    </div>
                    <div className="flex items-center gap-2 flex-wrap">
                        <select
                            value={selectedShiftId}
                            onChange={(e) => setSelectedShiftId(e.target.value)}
                            disabled={shifts.length === 0}
                            className="h-8 rounded-md border border-gray-300 text-sm px-2 focus:border-amber-500 focus:ring-amber-500"
                        >
                            {shifts.length === 0 && <option value="">No shifts available</option>}
                            {shifts.map((s) => (
                                <option key={s.id} value={s.id}>{s.name}</option>
                            ))}
                        </select>
                        <button
                            onClick={exportPDF}
                            disabled={exporting || allRecords.length === 0}
                            className="flex items-center gap-1.5 h-7 px-3 bg-amber-100 text-amber-900 border border-amber-300 rounded-md text-[10px] font-medium hover:bg-amber-200 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <FileDown size={13} />
                            PDF
                        </button>
                        <button
                            onClick={exportExcel}
                            disabled={exporting || allRecords.length === 0}
                            className="flex items-center gap-1.5 h-7 px-3 border border-gray-300 bg-white rounded-md text-[10px] font-medium hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <FileSpreadsheet size={13} />
                            Excel
                        </button>
                    </div>
                </div>
                {loading ? (
                    <div className="flex justify-center items-center h-64">
                        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
                    </div>
                ) : (
                    <>
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-300">
                                <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Clock In</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Clock Out</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Late</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created At</th>
                                </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                {pageItems.length === 0 ? (
                                    <tr>
                                        <td colSpan="6" className="px-6 py-4 text-center text-sm text-gray-500">No attendance records found for this shift.</td>
                                    </tr>
                                ) : (
                                    pageItems.map((record, index) => (
                                        <tr key={record.id || index} className="hover:bg-gray-50">
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                                <div>{record.employeeName || 'Unknown Employee'}</div>
                                                <div className="text-xs text-gray-500">{record.employeeEmail || 'N/A'}</div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatTime(record.clockInTime)}</td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatTime(record.clockOutTime)}</td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(record.status)}`}>
                                                    {getStatusText(record.status)}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatBoolean(record.isLate)}</td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatCreatedAt(record.createdAt)}</td>
                                        </tr>
                                    ))
                                )}
                                </tbody>
                            </table>
                        </div>
                        <PaginationControls
                            pagination={pagination}
                            onPageChange={handlePageChange}
                            onPageSizeChange={handlePageSizeChange}
                            hasPreviousPage={pagination.pageNumber > 1}
                            hasNextPage={pagination.pageNumber < totalPages}
                            totalCount={totalCount}
                            totalPages={totalPages}
                        />
                    </>
                )}
            </div>
        </div>
    );
};

// Instance-scoped view: attendance for one specific shift instance, reached by
// clicking "Attendance" on a row in ShiftInstances (via the instanceContext prop).
const InstanceAttendanceView = ({ instanceContext }) => {
    const { instanceData, instanceId, shiftName } = instanceContext;
    const [attendanceData, setAttendanceData] = useState({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 10 });

    useEffect(() => {
        const fetchAttendance = async () => {
            try {
                setLoading(true);
                const data = await getAttendanceByInstanceId(instanceId, pagination);
                if (data.items && data.items.length === 0) {
                    setError('No attendance records found for this shift instance.');
                } else {
                    setAttendanceData(data);
                    setError(null);
                }
            } catch (err) {
                if (err.response && err.response.status === 404) {
                    setError('No attendance records found for this shift instance.');
                } else {
                    setError('Failed to load attendance data. Please try again later.');
                }
            } finally {
                setLoading(false);
            }
        };
        fetchAttendance();
    }, [instanceId, pagination.pageNumber, pagination.pageSize]);

    const handlePageChange = (newPage) => {
        if (newPage >= 1 && newPage <= attendanceData.totalPages) {
            setPagination(prev => ({ ...prev, pageNumber: newPage }));
        }
    };

    const handlePageSizeChange = (e) => {
        setPagination({ pageNumber: 1, pageSize: parseInt(e.target.value) });
    };

    const [exportingExcel, setExportingExcel] = useState(false);

    const downloadExcel = async () => {
        if (!attendanceData.items || attendanceData.items.length === 0) {
            alert('No attendance data to export.');
            return;
        }
        setExportingExcel(true);
        try {
            const allData = await getAttendanceByInstanceId(instanceId, {
                pageNumber: 1,
                pageSize: attendanceData.totalCount || 1000
            });
            const allRecords = allData.items || [];
            if (allRecords.length === 0) {
                alert('No attendance data to export.');
                return;
            }

            const rows = allRecords.map((record) => ({
                'Employee Name': record.employeeName || 'Unknown',
                'Employee Email': record.employeeEmail || 'N/A',
                'Clock In': formatTime(record.clockInTime),
                'Clock Out': formatTime(record.clockOutTime),
                'Status': getStatusText(record.status),
                'Late': formatBoolean(record.isLate),
                'Created At': formatCreatedAt(record.createdAt),
            }));
            const ws = XLSX.utils.json_to_sheet(rows);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Attendance');
            const sanitizedShiftName = (shiftName || 'shift').replace(/[^a-z0-9]/gi, '-').toLowerCase();
            XLSX.writeFile(wb, `attendance-${sanitizedShiftName}.xlsx`);
        } catch (err) {
            alert('Failed to generate Excel file. Please try again.');
        } finally {
            setExportingExcel(false);
        }
    };

    const [exportingPdf, setExportingPdf] = useState(false);

    const downloadPDF = async () => {
        if (!attendanceData.items || attendanceData.items.length === 0) {
            alert('No attendance data to export.');
            return;
        }
        setExportingPdf(true);
        try {
            const allData = await getAttendanceByInstanceId(instanceId, {
                pageNumber: 1,
                pageSize: attendanceData.totalCount || 1000
            });
            const allRecords = allData.items || [];

            if (allRecords.length === 0) {
                alert('No attendance data to export.');
                return;
            }

            const presentCount = allRecords.filter((r) => r.status === 2).length;
            const lateCount = allRecords.filter((r) => r.isLate).length;
            const sanitizedShiftName = (shiftName || 'shift').replace(/[^a-z0-9]/gi, '-').toLowerCase();

            await generateAttendancePdf({
                badgeTitle: 'ATTENDANCE REPORT',
                subtitle: shiftName || 'Shift',
                stats: [
                    { label: 'TOTAL EMPLOYEES', value: `${attendanceData.totalCount || allRecords.length}` },
                    { label: 'PRESENT', value: `${presentCount}` },
                    { label: 'LATE', value: `${lateCount}` },
                ],
                rows: allRecords.map((record) => ({
                    'Employee Name': record.employeeName || 'Unknown',
                    'Employee Email': record.employeeEmail || 'N/A',
                    'Clock In': formatTime(record.clockInTime),
                    'Clock Out': formatTime(record.clockOutTime),
                    'Status': getStatusText(record.status),
                    'Late': formatBoolean(record.isLate),
                    'Created At': formatCreatedAt(record.createdAt),
                })),
                filename: `attendance-${sanitizedShiftName}-${dayjs().format('YYYY-MM-DD')}.pdf`,
            });
        } catch (error) {
            alert('Failed to generate PDF. Please try again.');
        } finally {
            setExportingPdf(false);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    if (error) {
        const isNoRecordsMessage = error.includes('No attendance records found');
        return (
            <div className="p-6">
                <div className={`border-l-4 p-4 rounded ${isNoRecordsMessage ? 'bg-blue-50 border-blue-400' : 'bg-red-50 border-red-400'}`}>
                    <p className={`text-sm ${isNoRecordsMessage ? 'text-blue-700' : 'text-red-700'}`}>{error}</p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-6">
            <div className="bg-white shadow overflow-hidden sm:rounded-lg">
                <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                    <div className="flex justify-between items-center">
                        <div>
                            <h3 className="text-lg leading-6 font-medium text-gray-900">
                                Shift Attendance {shiftName && `- ${shiftName}`}
                            </h3>
                            <div className="mt-2 flex items-center space-x-4">
                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                    {attendanceData.totalCount || 0} {attendanceData.totalCount === 1 ? 'Employee' : 'Employees'}
                                </span>
                                {instanceData?.scheduledDate && (
                                    <span className="text-xs text-gray-500">{format(new Date(instanceData.scheduledDate), 'PPP')}</span>
                                )}
                            </div>
                        </div>
                        <div className="flex gap-1.5 shrink-0">
                            <button
                                onClick={downloadPDF}
                                disabled={exportingPdf || !attendanceData.items || attendanceData.items.length === 0}
                                className="flex items-center gap-1.5 h-7 px-3 bg-amber-100 text-amber-900 border border-amber-300 rounded-md text-[10px] font-medium hover:bg-amber-200 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                <FileDown size={13} />
                                PDF
                            </button>
                            <button
                                onClick={downloadExcel}
                                disabled={exportingExcel || !attendanceData.items || attendanceData.items.length === 0}
                                className="flex items-center gap-1.5 h-7 px-3 border border-gray-300 bg-white rounded-md text-[10px] font-medium hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                <FileSpreadsheet size={13} />
                                Excel
                            </button>
                        </div>
                    </div>
                </div>
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-300">
                        <thead className="bg-gray-50">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee Name</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee Email</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Clock In Time</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Clock Out Time</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Is Late</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created At</th>
                        </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                        {attendanceData.items.length === 0 ? (
                            <tr>
                                <td colSpan="7" className="px-6 py-4 text-center text-sm text-gray-500">No attendance records found for this shift.</td>
                            </tr>
                        ) : (
                            attendanceData.items.map((record, index) => (
                                <tr key={record.employeeId || index} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{record.employeeName || 'Unknown Employee'}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{record.employeeEmail || 'N/A'}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatTime(record.clockInTime)}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatTime(record.clockOutTime)}</td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(record.status)}`}>
                                            {getStatusText(record.status)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatBoolean(record.isLate)}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatCreatedAt(record.createdAt)}</td>
                                </tr>
                            ))
                        )}
                        </tbody>
                    </table>
                </div>
                <PaginationControls
                    pagination={pagination}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                    hasPreviousPage={attendanceData.hasPreviousPage}
                    hasNextPage={attendanceData.hasNextPage}
                    totalCount={attendanceData.totalCount}
                    totalPages={attendanceData.totalPages}
                />
            </div>
        </div>
    );
};

// Accepts instanceContext as a prop (passed directly by the Shifts hub when a user
// clicks "Attendance" on a shift instance) and falls back to router state for any
// legacy/external link. With neither, it shows attendance across all shifts instead
// of bouncing the user back — this is a real page now, not just a click target.
const Attendance = ({ instanceContext }) => {
    const location = useLocation();
    const effectiveContext = instanceContext || (location.state?.instanceId ? location.state : null);

    if (effectiveContext?.instanceId) {
        return <InstanceAttendanceView instanceContext={effectiveContext} />;
    }
    return <ShiftAttendanceView />;
};

export default Attendance;
