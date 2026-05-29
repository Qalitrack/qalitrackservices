import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { getAttendanceByInstanceId } from '../../api/helpers/UserService/Shifts/Attendance.js';
import { format } from 'date-fns';
import { ChevronLeftIcon, ChevronRightIcon, ChevronDoubleLeftIcon, ChevronDoubleRightIcon } from '@heroicons/react/20/solid';
import jsPDF from 'jspdf';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

const Attendance = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const [attendanceData, setAttendanceData] = useState({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10
    });

    const { instanceData, instanceId, shiftName } = location.state || {};

    useEffect(() => {
        if (!instanceId) {
            navigate(-1);
        }
    }, [instanceId, navigate]);

    useEffect(() => {
        const fetchAttendance = async () => {
            if (!instanceId) {
                setError('No instance ID provided');
                setLoading(false);
                return;
            }
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
            setPagination(prev => ({
                ...prev,
                pageNumber: newPage
            }));
        }
    };

    const handlePageSizeChange = (e) => {
        const newSize = parseInt(e.target.value);
        setPagination({
            pageNumber: 1,
            pageSize: newSize
        });
    };

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

    // Helper function to format boolean as Yes/No
    const formatBoolean = (value) => value ? 'Yes' : 'No';

    // Helper function to format time
    const formatTime = (dateString) => {
        if (!dateString || isNaN(new Date(dateString))) return '--:-- --';
        return format(new Date(dateString), 'h:mm a');
    };

    // Helper function to format date for createdAt
    const formatCreatedAt = (dateString) => {
        if (!dateString || isNaN(new Date(dateString))) return 'N/A';
        return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
    };

    // Download PDF function
    const downloadPDF = async () => {
        if (!attendanceData.items || attendanceData.items.length === 0) {
            alert('No attendance data to download.');
            return;
        }

        try {
            // Show loading state
            const button = document.activeElement;
            const originalText = button.textContent;
            button.textContent = 'Generating PDF...';
            button.disabled = true;

            // Fetch all attendance records
            const allData = await getAttendanceByInstanceId(instanceId, {
                pageNumber: 1,
                pageSize: attendanceData.totalCount || 1000
            });

            const allRecords = allData.items || [];

            if (allRecords.length === 0) {
                alert('No attendance data to download.');
                button.textContent = originalText;
                button.disabled = false;
                return;
            }

        const { primary: accent, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(getTicketSettings());
        const doc = new jsPDF('landscape');
        const pageWidth = doc.internal.pageSize.getWidth();
        const margin = 10;
        const startY = 20;
        let yPosition = startY;
        const rowHeight = 8;
        
        // Define column widths
        const colWidths = [40, 50, 30, 30, 25, 20, 35];
        const totalWidth = colWidths.reduce((a, b) => a + b, 0);

        // Title with shift name
        doc.setFontSize(18);
        doc.setFont(undefined, 'bold');
        doc.setTextColor(44, 62, 80);
        doc.text(`Attendance Report - ${shiftName || 'Shift'}`, margin, yPosition);
        yPosition += 10;

        // Subtitle with date and employee count
        doc.setFontSize(10);
        doc.setFont(undefined, 'normal');
        doc.setTextColor(100, 100, 100);
        doc.text(`Total Employees: ${attendanceData.totalCount}`, margin, yPosition);
        yPosition += 15;

        // Table setup
        const headerY = yPosition;

        // Headers
        doc.setFontSize(10);
        doc.setFont(undefined, 'bold');
        doc.setFillColor(...accent);
        doc.rect(margin, headerY, totalWidth, rowHeight + 2, 'F');
        doc.setTextColor(...accentHeaderText);

        let xPos = margin + 2;
        const headers = ['Employee Name', 'Employee Email', 'Clock In', 'Clock Out', 'Status', 'Late', 'Created At'];
        headers.forEach((header, idx) => {
            doc.text(header, xPos, headerY + 6);
            xPos += colWidths[idx];
        });

        yPosition += rowHeight + 2;

        // Reset text color for rows
        doc.setTextColor(44, 62, 80);
        doc.setFont(undefined, 'normal');

        // Rows
        allRecords.forEach((record, rowIndex) => {
            // Check if we need a new page
            if (yPosition > 180) {
                doc.addPage('landscape');
                yPosition = startY;
            }

            const rowY = yPosition;
            const isEvenRow = rowIndex % 2 === 0;

            // Row background (alternating)
            if (isEvenRow) {
                doc.setFillColor(...accentLight);
                doc.rect(margin, rowY, totalWidth, rowHeight, 'F');
            }

            doc.setFontSize(9);
            
            xPos = margin + 2;
            const rowData = [
                record.employeeName || 'Unknown',
                record.employeeEmail || 'N/A',
                formatTime(record.clockInTime),
                formatTime(record.clockOutTime),
                getStatusText(record.status),
                formatBoolean(record.isLate),
                formatCreatedAt(record.createdAt)
            ];

            rowData.forEach((data, idx) => {
                // Truncate text if too long
                const maxWidth = colWidths[idx] - 4;
                const text = doc.splitTextToSize(data, maxWidth)[0];
                doc.text(text, xPos, rowY + 6);
                xPos += colWidths[idx];
            });

            // Draw row borders
            doc.setDrawColor(189, 195, 199);
            doc.setLineWidth(0.2);
            doc.line(margin, rowY + rowHeight, margin + totalWidth, rowY + rowHeight);

            yPosition += rowHeight;
        });

        // Draw outer border
        doc.setDrawColor(44, 62, 80);
        doc.setLineWidth(0.5);
        doc.rect(margin, headerY, totalWidth, yPosition - headerY);

        // Save the PDF
        const sanitizedShiftName = (shiftName || 'shift').replace(/[^a-z0-9]/gi, '-').toLowerCase();
        doc.save(`attendance-${sanitizedShiftName}.pdf`);

            // Reset button state
            button.textContent = originalText;
            button.disabled = false;

        } catch (error) {
            alert('Failed to generate PDF. Please try again.');
            
            // Reset button state
            const button = document.activeElement;
            button.textContent = 'Download PDF';
            button.disabled = false;
        }
    };

    const PaginationControls = () => (
        <div className="flex items-center justify-between px-4 py-3 bg-white border-t border-gray-200 sm:px-6">
            <div className="flex-1 flex justify-between sm:hidden">
                <button
                    onClick={() => handlePageChange(pagination.pageNumber - 1)}
                    disabled={!attendanceData.hasPreviousPage}
                    className={`relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md ${
                        attendanceData.hasPreviousPage ? 'bg-white text-gray-700 hover:bg-gray-50' : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                    }`}
                >
                    Previous
                </button>
                <button
                    onClick={() => handlePageChange(pagination.pageNumber + 1)}
                    disabled={!attendanceData.hasNextPage}
                    className={`ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md ${
                        attendanceData.hasNextPage ? 'bg-white text-gray-700 hover:bg-gray-50' : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                    }`}
                >
                    Next
                </button>
            </div>
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                    <p className="text-sm text-gray-700">
                        Showing <span className="font-medium">{(pagination.pageNumber - 1) * pagination.pageSize + 1}</span> to{' '}
                        <span className="font-medium">
                            {Math.min(pagination.pageNumber * pagination.pageSize, attendanceData.totalCount)}
                        </span>{' '}
                        of <span className="font-medium">{attendanceData.totalCount}</span> results
                    </p>
                </div>
                <div className="flex items-center space-x-4">
                    <div className="flex items-center">
                        <label htmlFor="page-size" className="mr-2 text-sm text-gray-700">
                            Rows per page:
                        </label>
                        <select
                            id="page-size"
                            value={pagination.pageSize}
                            onChange={handlePageSizeChange}
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-amber-500 focus:ring-amber-500 sm:text-sm"
                        >
                            <option value={5}>5</option>
                            <option value={10}>10</option>
                            <option value={20}>20</option>
                            <option value={50}>50</option>
                        </select>
                    </div>
                    <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                        <button
                            onClick={() => handlePageChange(1)}
                            disabled={!attendanceData.hasPreviousPage}
                            className={`relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium ${
                                attendanceData.hasPreviousPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">First</span>
                            <ChevronDoubleLeftIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                        <button
                            onClick={() => handlePageChange(pagination.pageNumber - 1)}
                            disabled={!attendanceData.hasPreviousPage}
                            className={`relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium ${
                                attendanceData.hasPreviousPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">Previous</span>
                            <ChevronLeftIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                        <div className="px-4 py-2 bg-white text-sm font-medium text-gray-700 border-t border-b border-gray-300">
                            Page {pagination.pageNumber} of {attendanceData.totalPages || 1}
                        </div>
                        <button
                            onClick={() => handlePageChange(pagination.pageNumber + 1)}
                            disabled={!attendanceData.hasNextPage}
                            className={`relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium ${
                                attendanceData.hasNextPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">Next</span>
                            <ChevronRightIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                        <button
                            onClick={() => handlePageChange(attendanceData.totalPages)}
                            disabled={!attendanceData.hasNextPage}
                            className={`relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium ${
                                attendanceData.hasNextPage ? 'text-gray-500 hover:bg-gray-50' : 'text-gray-300 cursor-not-allowed'
                            }`}
                        >
                            <span className="sr-only">Last</span>
                            <ChevronDoubleRightIcon className="h-5 w-5" aria-hidden="true" />
                        </button>
                    </nav>
                </div>
            </div>
        </div>
    );

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    // Check for empty attendance data
    if (attendanceData.items && attendanceData.items.length === 0) {
        return (
            <div className="p-6">
                <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded">
                    <div className="flex items-center">
                        <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-blue-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h2a1 1 0 100-2h-2V9z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm text-blue-700">No attendance records found for this shift instance.</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (error) {
        const isNoRecordsMessage = error.includes('No attendance records found');
        
        if (isNoRecordsMessage) {
            return (
                <div className="p-6">
                    <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded">
                        <div className="flex items-center">
                            <div className="flex-shrink-0">
                                <svg className="h-5 w-5 text-blue-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h2a1 1 0 100-2h-2V9z" clipRule="evenodd" />
                                </svg>
                            </div>
                            <div className="ml-3">
                                <p className="text-sm text-blue-700">{error}</p>
                            </div>
                        </div>
                    </div>
                </div>
            );
        }
        
        return (
            <div className="p-6">
                <div className="bg-red-50 border-l-4 border-red-400 p-4">
                    <div className="flex">
                        <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm text-red-700">{error}</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (instanceData && attendanceData.items.length === 0) {
        return (
            <div className="p-6">
                <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
                    <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                        <h3 className="text-lg leading-6 font-medium text-gray-900">
                            Shift Attendance
                        </h3>
                    </div>
                    <div className="px-4 py-5 sm:p-6">
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <div>
                                <dt className="text-sm font-medium text-gray-500">Status</dt>
                                <dd className="mt-1 text-sm text-gray-900">
                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                                        instanceData.status === 3 ? 'bg-green-100 text-green-800' :
                                            instanceData.status === 4 ? 'bg-red-100 text-red-800' :
                                                'bg-blue-100 text-blue-800'
                                    }`}>
                                        {instanceData.status === 3 ? 'Completed' :
                                            instanceData.status === 4 ? 'Cancelled' : 'Scheduled'}
                                    </span>
                                </dd>
                            </div>
                        </div>
                    </div>
                    <PaginationControls />
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
                            </div>
                        </div>
                        <button
                            onClick={downloadPDF}
                            disabled={!attendanceData.items || attendanceData.items.length === 0}
                            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                                attendanceData.items && attendanceData.items.length > 0
                                    ? 'bg-amber-500 hover:bg-amber-600 text-white'
                                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                            }`}
                        >
                            Download PDF
                        </button>
                    </div>
                </div>
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-300">
                        <thead className="bg-gray-50">
                        <tr>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Employee Name
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Employee Email
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Clock In Time
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Clock Out Time
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Status
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Is Late
                            </th>
                            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Created At
                            </th>
                        </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                        {attendanceData.items.length === 0 ? (
                            <tr>
                                <td colSpan="8" className="px-6 py-4 text-center text-sm text-gray-500">
                                    No attendance records found for this shift.
                                </td>
                            </tr>
                        ) : (
                            attendanceData.items.map((record, index) => (
                                <tr key={record.employeeId || index} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                        {record.employeeName || 'Unknown Employee'}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                        {record.employeeEmail || 'N/A'}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                        {formatTime(record.clockInTime)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                        {formatTime(record.clockOutTime)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(record.status)}`}>
                                                {getStatusText(record.status)}
                                            </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                        {formatBoolean(record.isLate)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                        {formatCreatedAt(record.createdAt)}
                                    </td>
                                </tr>
                            ))
                        )}
                        </tbody>
                    </table>
                </div>
                <PaginationControls />
            </div>
        </div>
    );
};

export default Attendance;