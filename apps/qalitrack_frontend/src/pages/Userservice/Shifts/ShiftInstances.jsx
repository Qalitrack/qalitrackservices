import React, { useState, useEffect } from 'react';
import { format, parseISO } from 'date-fns';
import { useNavigate } from 'react-router-dom';
import { fetchShiftInstances } from '../../../api/helpers/UserService/Shifts/Shifts';
import { EyeIcon } from '@heroicons/react/24/outline';
import jsPDF from 'jspdf';

const ShiftInstances = ({ shiftId }) => {
    const navigate = useNavigate();
    const [instances, setInstances] = useState([]);
    const [shiftName, setShiftName] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // ShiftInstanceStatus enum from the server
    const ShiftInstanceStatus = {
        Scheduled: 1,
        InProgress: 2,
        Completed: 3,
        Cancelled: 4,
        NoShow: 5
    };

    // Function to get status text based on status code
    const getStatusText = (status) => {
        switch(status) {
            case ShiftInstanceStatus.Scheduled: return 'Scheduled';
            case ShiftInstanceStatus.InProgress: return 'In Progress';
            case ShiftInstanceStatus.Completed: return 'Completed';
            case ShiftInstanceStatus.Cancelled: return 'Cancelled';
            case ShiftInstanceStatus.NoShow: return 'No Show';
            default: return 'Unknown';
        }
    };

    // Function to get status color class
    const getStatusColor = (status) => {
        switch(status) {
            case ShiftInstanceStatus.Scheduled: return 'bg-blue-100 text-blue-800';
            case ShiftInstanceStatus.InProgress: return 'bg-yellow-100 text-yellow-800';
            case ShiftInstanceStatus.Completed: return 'bg-green-100 text-green-800';
            case ShiftInstanceStatus.Cancelled: return 'bg-red-100 text-red-800';
            case ShiftInstanceStatus.NoShow: return 'bg-purple-100 text-purple-800';
            default: return 'bg-gray-100 text-gray-800';
        }
    };

    // Fetch shift instances when component mounts or shiftId changes
    useEffect(() => {
        const loadInstances = async () => {
            if (!shiftId) return;

            try {
                setLoading(true);
                setError(null);
                const data = await fetchShiftInstances(shiftId);
                const instancesArray = Array.isArray(data) ? data : [data];
                setInstances(instancesArray);
                
                // Get the shift name from the first instance - try multiple possible paths
                if (instancesArray.length > 0) {
                    const firstInstance = instancesArray[0];
                    const name = firstInstance.shiftName || 
                                firstInstance.shift?.name || 
                                firstInstance.shift?.shiftName ||
                                'Unknown Shift';
                    setShiftName(name);
                } else {
                    setShiftName('Unknown Shift');
                }
            } catch (err) {
                console.error('Error loading shift instances:', err);
                setError('Failed to load shift instances');
            } finally {
                setLoading(false);
            }
        };

        loadInstances();
    }, [shiftId]);

    // Format date to display
    const formatDate = (dateString) => {
        try {
            return format(parseISO(dateString), 'MMM d, yyyy');
        } catch {
            return 'Invalid date';
        }
    };

    // Format time to display
    const formatTime = (timeString) => {
        try {
            return format(parseISO(timeString), 'h:mm a');
        } catch {
            return 'Invalid time';
        }
    };

    // Download PDF function
    const downloadPDF = () => {
        if (instances.length === 0) {
            alert('No data to download.');
            return;
        }

        const doc = new jsPDF();
        const pageWidth = doc.internal.pageSize.getWidth();
        const margin = 10;
        const startY = 20;
        let yPosition = startY;
        const rowHeight = 8;
        const colWidths = [(pageWidth - 2 * margin) / 3, (pageWidth - 2 * margin) / 3, (pageWidth - 2 * margin) / 3];
        let rowIndex = 0;

        // Title with shift name
        doc.setFontSize(18);
        doc.setFont(undefined, 'bold');
        doc.setTextColor(44, 62, 80); // Dark gray-blue
        doc.text(`Shift Instances - ${shiftName}`, margin, yPosition);
        yPosition += 20;

        // Table setup
        const headerY = yPosition;

        // Draw outer table border
        doc.setDrawColor(44, 62, 80);
        doc.setLineWidth(0.5);
        doc.rect(margin, headerY, pageWidth - 2 * margin, rowHeight * (instances.length + 1) + 10);

        // Headers with different column colors
        doc.setFontSize(12);
        doc.setFont(undefined, 'bold');

        // Date column header (blue background)
        doc.setFillColor(52, 152, 219);
        doc.rect(margin, headerY, colWidths[0], rowHeight + 2, 'F');
        doc.setTextColor(255, 255, 255);
        doc.text('Date', margin + 2, headerY + 6);

        // Shift Time column header (green background)
        doc.setFillColor(46, 204, 113);
        doc.rect(margin + colWidths[0], headerY, colWidths[1], rowHeight + 2, 'F');
        doc.setTextColor(255, 255, 255);
        doc.text('Shift Time', margin + colWidths[0] + 2, headerY + 6);

        // Status column header (orange background)
        doc.setFillColor(230, 126, 34);
        doc.rect(margin + colWidths[0] + colWidths[1], headerY, colWidths[2], rowHeight + 2, 'F');
        doc.setTextColor(255, 255, 255);
        doc.text('Status', margin + colWidths[0] + colWidths[1] + 2, headerY + 6);

        // Vertical lines for headers
        doc.setDrawColor(255, 255, 255);
        doc.setLineWidth(0.5);
        doc.line(margin + colWidths[0], headerY, margin + colWidths[0], headerY + rowHeight + 2);
        doc.line(margin + colWidths[0] + colWidths[1], headerY, margin + colWidths[0] + colWidths[1], headerY + rowHeight + 2);

        yPosition += rowHeight + 2;

        // Reset text color for rows
        doc.setTextColor(44, 62, 80);

        // Rows with alternating colors
        instances.forEach((instance) => {
            // Check if we need a new page
            if (yPosition > 270) {
                doc.addPage();
                yPosition = startY;
                // Redraw outer border if needed, but simplified here
            }

            const rowY = yPosition;
            const isEvenRow = rowIndex % 2 === 0;

            // Row background (alternating)
            if (isEvenRow) {
                doc.setFillColor(248, 249, 250); // Light gray
                doc.rect(margin, rowY, pageWidth - 2 * margin, rowHeight, 'F');
            }

            doc.setFontSize(10);
            doc.setFont(undefined, 'normal');
            doc.text(formatDate(instance.scheduledDate), margin + 2, rowY + 6);
            doc.text(`${formatTime(instance.scheduledStartTime)} - ${formatTime(instance.scheduledEndTime)}`, margin + colWidths[0] + 2, rowY + 6);
            doc.text(getStatusText(instance.status), margin + colWidths[0] + colWidths[1] + 2, rowY + 6);

            // Draw row borders
            doc.setDrawColor(189, 195, 199);
            doc.setLineWidth(0.2);
            doc.line(margin, rowY + rowHeight, pageWidth - margin, rowY + rowHeight); // Bottom horizontal
            doc.line(margin, rowY, margin, rowY + rowHeight); // Left vertical
            doc.line(pageWidth - margin, rowY, pageWidth - margin, rowY + rowHeight); // Right vertical
            doc.line(margin + colWidths[0], rowY, margin + colWidths[0], rowY + rowHeight); // Middle vertical 1
            doc.line(margin + colWidths[0] + colWidths[1], rowY, margin + colWidths[0] + colWidths[1], rowY + rowHeight); // Middle vertical 2

            yPosition += rowHeight;
            rowIndex++;
        });

        // Save the PDF with shift name (without ID)
        const sanitizedShiftName = shiftName.replace(/[^a-z0-9]/gi, '-').toLowerCase();
        doc.save(`shift-instances-${sanitizedShiftName}.pdf`);
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center p-4">
                <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="p-4 text-red-600">
                {error}
            </div>
        );
    }

    return (
        <div className="w-full mx-auto p-4">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-lg font-medium text-gray-900">Shift Instances</h2>
                <button
                    onClick={downloadPDF}
                    disabled={instances.length === 0}
                    className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                        instances.length === 0
                            ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                            : 'bg-amber-500 hover:bg-amber-600 text-white'
                    }`}
                >
                    Download PDF
                </button>
            </div>
            {instances.length === 0 ? (
                <div className="p-4 text-gray-500">
                    No shift instances found.
                </div>
            ) : (
                <div className="bg-white shadow overflow-x-auto sm:rounded-lg">
                    <div className="inline-block min-w-full align-middle">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Date
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Shift Time
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Status
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Actions
                                </th>
                            </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                            {instances.map((instance) => (
                                <tr key={instance.id} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="text-sm font-medium text-gray-900">
                                            {formatDate(instance.scheduledDate)}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="text-sm text-gray-900">
                                            {formatTime(instance.scheduledStartTime)} - {formatTime(instance.scheduledEndTime)}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(instance.status)}`}>
                                            {getStatusText(instance.status)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                        <button
                                            onClick={() => {
                                                // Navigate to the attendance page with instance data and shiftId
                                                navigate('/Admin/attendance', { 
                                                    state: { 
                                                        instanceData: instance,
                                                        instanceId: instance.id,
                                                        shiftId: shiftId,
                                                        shiftName: shiftName  // Add this line
                                                    } 
                                                });
                                            }}
                                            className="text-amber-600 hover:text-amber-900 flex items-center space-x-1"
                                            title="View Attendance"
                                        >
                                            <EyeIcon className="h-4 w-4" />
                                            <span>Attendance</span>
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ShiftInstances;