import React, { useState, useEffect } from 'react';
import { backupAPI } from '../../../api/helpers/Backup/Backup';
import { RefreshCw, Clock, AlertCircle, CheckCircle, HelpCircle, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';

const ScheduledBackup = () => {
    const [scheduledBackups, setScheduledBackups] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdated, setLastUpdated] = useState(null);
    const [tooltip, setTooltip] = useState({ show: false, text: '', x: 0, y: 0 });
    const [isDeleting, setIsDeleting] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [selectedBackup, setSelectedBackup] = useState(null);

    const loadScheduledBackups = async () => {
        try {
            setLoading(true);
            const data = await backupAPI.getScheduledBackups();
            setScheduledBackups(data);
            setLastUpdated(new Date());
            setError(null);
        } catch (err) {
            setError('Failed to load scheduled backups. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadScheduledBackups();
    }, []);

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString();
    };

    const getBackupType = (type) => {
        return type === 0 ? 'Full' : 'Incremental';
    };

    const getStatusBadge = (nextFireTime) => {
        const now = new Date();
        const nextRun = new Date(nextFireTime);
        return nextRun > now ? (
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                <CheckCircle className="h-3 w-3 mr-1" /> Active
            </span>
        ) : (
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                <AlertCircle className="h-3 w-3 mr-1" /> Expired
            </span>
        );
    };

    const parseCronExpression = (cron) => {
        if (!cron) return 'Not scheduled';

        try {
            const parts = cron.split(' ');
            if (parts.length < 5) return cron;

            const [minute, hour, dayOfMonth, month, dayOfWeek] = parts;

            // Common patterns
            if (cron === '0 0 * * *') return 'Daily at 12:00 AM';
            if (cron === '0 0 * * 0') return 'Weekly on Sunday at 12:00 AM';
            if (cron === '0 0 1 * *') return 'Monthly on the 1st at 12:00 AM';
            if (cron === '0 0 1 1 *') return 'Yearly on January 1st at 12:00 AM';

            // Parse custom cron
            let description = '';
            const daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
            const months = [
                'January', 'February', 'March', 'April', 'May', 'June',
                'July', 'August', 'September', 'October', 'November', 'December'
            ];

            // Handle minutes
            if (minute === '*') {
                description += 'Every minute';
            } else if (minute.includes('/')) {
                const [_, interval] = minute.split('/');
                description += `Every ${interval} minutes`;
            } else if (minute !== '0') {
                description += `At ${minute.padStart(2, '0')} minutes`;
            }

            // Handle hours
            if (hour !== '*') {
                if (minute === '0' || minute === '0/0' || minute === '00') {
                    description = description.replace('At 00 minutes', `At ${hour.padStart(2, '0')}:00`);
                } else if (minute === '0/0' || minute === '00') {
                    description = description.replace('Every 0 minutes', `At ${hour.padStart(2, '0')}:00`);
                } else if (description.includes('minutes')) {
                    description += ` past ${hour.padStart(2, '0')}:00`;
                } else {
                    description += ` at ${hour.padStart(2, '0')}:${minute.padStart(2, '0')}`;
                }
            } else if (description.includes('minutes') && !description.includes('at')) {
                description += ' of every hour';
            }

            // Handle day of month
            if (dayOfMonth !== '*') {
                if (dayOfMonth.includes('/')) {
                    const [_, interval] = dayOfMonth.split('/');
                    description += ` every ${interval} days`;
                } else {
                    description += ` on day ${dayOfMonth} of the month`;
                }
            }

            // Handle month
            if (month !== '*' && month !== '?') {
                if (month.includes(',')) {
                    const monthsList = month.split(',').map(m => {
                        const monthIndex = parseInt(m) - 1;
                        return months[monthIndex] || m;
                    });
                    description += ` in ${monthsList.join(', ')}`;
                } else if (month.includes('-')) {
                    const [start, end] = month.split('-').map(m => {
                        const monthIndex = parseInt(m) - 1;
                        return months[monthIndex] || m;
                    });
                    description += ` from ${start} to ${end}`;
                } else if (!isNaN(month)) {
                    const monthIndex = parseInt(month) - 1;
                    if (monthIndex >= 0 && monthIndex < 12) {
                        description += ` in ${months[monthIndex]}`;
                    }
                }
            }

            // Handle day of week
            if (dayOfWeek !== '*') {
                if (dayOfWeek.includes(',')) {
                    const daysList = dayOfWeek.split(',').map(d => daysOfWeek[parseInt(d)]);
                    description += ` on ${daysList.join(', ')}`;
                } else if (dayOfWeek.includes('-')) {
                    const [start, end] = dayOfWeek.split('-').map(d => daysOfWeek[parseInt(d)]);
                    description += ` from ${start} to ${end}`;
                } else if (!isNaN(dayOfWeek)) {
                    description += ` on ${daysOfWeek[parseInt(dayOfWeek)]}`;
                }
            }

            // Clean up the description
            description = description.replace(/,/g, ', ').replace(/\s+/g, ' ').trim();
            return description || cron;
        } catch (e) {
            return cron;
        }
    };

    const handleDeleteClick = (microservice, backupType) => {
        setSelectedBackup({ microservice, backupType });
        setShowDeleteModal(true);
    };

    const handleConfirmDelete = async () => {
        if (!selectedBackup) return;

        try {
            setIsDeleting(true);
            await backupAPI.unscheduleBackup(selectedBackup.microservice, selectedBackup.backupType);
            toast.success('Backup unscheduled successfully');
            await loadScheduledBackups();
        } catch (error) {
            toast.error(`Failed to unschedule backup: ${error.message}`);
        } finally {
            setIsDeleting(false);
            setShowDeleteModal(false);
            setSelectedBackup(null);
        }
    };

    const handleCancelDelete = () => {
        setShowDeleteModal(false);
        setSelectedBackup(null);
    };

    const showTooltip = (e, text) => {
        setTooltip({
            show: true,
            text,
            x: e.clientX,
            y: e.clientY - 40
        });
    };

    const hideTooltip = () => {
        setTooltip(prev => ({ ...prev, show: false }));
    };

    return (
        <div className="relative">
            {tooltip.show && (
                <div
                    className="fixed bg-gray-900 text-white text-xs rounded py-1 px-2 z-50 pointer-events-none"
                    style={{ left: `${tooltip.x}px`, top: `${tooltip.y}px` }}
                >
                    {tooltip.text}
                </div>
            )}

            <div className="px-4 sm:px-6 lg:px-8">
                <div className="sm:flex sm:items-center">
                    <div className="sm:flex-auto">
                        <h1 className="text-xl font-semibold text-gray-900">Scheduled Backups</h1>
                        <p className="mt-2 text-sm text-gray-700">
                            A list of all scheduled backup jobs and their next run times.
                        </p>
                    </div>
                    <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
                        <button
                            type="button"
                            onClick={loadScheduledBackups}
                            disabled={loading}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-amber-600 hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:opacity-50"
                        >
                            <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
                            {loading ? 'Refreshing...' : 'Refresh'}
                        </button>
                    </div>
                </div>

                {lastUpdated && (
                    <div className="mt-2 text-xs text-gray-500 flex items-center">
                        <Clock className="h-3 w-3 mr-1" />
                        Last updated: {formatDate(lastUpdated)}
                    </div>
                )}

                {error && (
                    <div className="mt-4 bg-red-50 border-l-4 border-red-400 p-4">
                        <div className="flex">
                            <div className="flex-shrink-0">
                                <AlertCircle className="h-5 w-5 text-red-400" aria-hidden="true" />
                            </div>
                            <div className="ml-3">
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        </div>
                    </div>
                )}

                <div className="mt-8 flex flex-col">
                    <div className="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
                        <div className="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
                            <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
                                <table className="min-w-full divide-y divide-gray-300">
                                    <thead className="bg-gray-50">
                                    <tr>
                                        <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                                            Microservice
                                        </th>
                                        <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                                            Type
                                        </th>
                                        <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                                            Schedule
                                        </th>
                                        <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                                            Next Run
                                        </th>
                                        <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                                            Status
                                        </th>
                                        <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                                            <span className="sr-only">Actions</span>
                                        </th>
                                    </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-200 bg-white">
                                    {loading ? (
                                        <tr>
                                            <td colSpan="6" className="px-3 py-4 text-sm text-gray-500 text-center">
                                                Loading scheduled backups...
                                            </td>
                                        </tr>
                                    ) : scheduledBackups.length === 0 ? (
                                        <tr>
                                            <td colSpan="6" className="px-3 py-4 text-sm text-gray-500 text-center">
                                                No scheduled backups found.
                                            </td>
                                        </tr>
                                    ) : (
                                        scheduledBackups.map((backup, index) => (
                                            <tr key={index} className="hover:bg-gray-50">
                                                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                                                    {backup.microservice}
                                                </td>
                                                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                                    {getBackupType(backup.backupType)}
                                                </td>
                                                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                                    <div className="flex items-center">
                                                        <span>{parseCronExpression(backup.cronSchedule)}</span>
                                                        <div
                                                            className="ml-1 text-gray-400 hover:text-gray-600 cursor-help relative"
                                                            onMouseEnter={(e) => showTooltip(e, backup.cronSchedule)}
                                                            onMouseLeave={hideTooltip}
                                                        >
                                                            <HelpCircle className="h-4 w-4" />
                                                        </div>
                                                    </div>
                                                </td>
                                                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                                    {formatDate(backup.nextFireTime)}
                                                </td>
                                                <td className="whitespace-nowrap px-3 py-4 text-sm">
                                                    {getStatusBadge(backup.nextFireTime)}
                                                </td>
                                                <td className="whitespace-nowrap px-3 py-4 text-right text-sm font-medium">
                                                    <button
                                                        onClick={() => handleDeleteClick(backup.microservice, backup.backupType)}
                                                        disabled={isDeleting}
                                                        className="text-red-600 hover:text-red-900 disabled:opacity-50 disabled:cursor-not-allowed"
                                                        title="Unschedule backup"
                                                    >
                                                        <Trash2 className="h-4 w-4" />
                                                    </button>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Delete Confirmation Modal */}
                {showDeleteModal && (
                    <div className="fixed z-10 inset-0 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
                        <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
                            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true" onClick={handleCancelDelete}></div>
                            <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
                            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
                                <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                                    <div className="sm:flex sm:items-start">
                                        <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-red-100 sm:mx-0 sm:h-10 sm:w-10">
                                            <AlertCircle className="h-6 w-6 text-red-600" aria-hidden="true" />
                                        </div>
                                        <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
                                            <h3 className="text-lg leading-6 font-medium text-gray-900" id="modal-title">
                                                Unschedule Backup
                                            </h3>
                                            <div className="mt-2">
                                                <p className="text-sm text-gray-500">
                                                    Are you sure you want to unschedule this backup? This action cannot be undone.
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                                    <button
                                        type="button"
                                        onClick={handleConfirmDelete}
                                        disabled={isDeleting}
                                        className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-red-600 text-base font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {isDeleting ? 'Unscheduling...' : 'Yes, unschedule'}
                                    </button>
                                    <button
                                        type="button"
                                        onClick={handleCancelDelete}
                                        disabled={isDeleting}
                                        className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        Cancel
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ScheduledBackup;