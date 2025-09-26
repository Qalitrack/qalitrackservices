import React, { useState, useEffect } from 'react';
import { format, parseISO } from 'date-fns';
import { useNavigate } from 'react-router-dom';
import { fetchShiftInstances } from '../../../helpers/UserService/Shifts/Shifts';
import { EyeIcon } from '@heroicons/react/24/outline';

const ShiftInstances = ({ shiftId }) => {
    const navigate = useNavigate();
    const [instances, setInstances] = useState([]);
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
                setInstances(Array.isArray(data) ? data : [data]);
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

    if (instances.length === 0) {
        return (
            <div className="p-4 text-gray-500">
                No shift instances found.
            </div>
        );
    }

    return (
        <div className="w-full mx-auto p-4">
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
                                            // Navigate to the attendance page with instance data
                                            navigate('/Admin/attendance', { 
                                                state: { 
                                                    instanceData: instance,
                                                    instanceId: instance.id
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
        </div>
    );
};

export default ShiftInstances;