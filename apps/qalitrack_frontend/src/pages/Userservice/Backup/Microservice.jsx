import React, { useState, useEffect } from 'react';
import { fetchMicroserviceData } from '../../../api/helpers/Backup/Microservice.js';
import { RefreshCw, Database, Clock, HardDriveDownload, Calendar, Download, X } from 'lucide-react';
import Backup from './Backup';
import ScheduledBackup from './ScheduledBackup';
import AvailableBackups from './AvailableBackups';

const Microservices = () => {
    const [microservices, setMicroservices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdated, setLastUpdated] = useState(null);
    const [showBackupModal, setShowBackupModal] = useState(false);
    const [showScheduledBackups, setShowScheduledBackups] = useState(false);
    const [showAvailableBackupsModal, setShowAvailableBackupsModal] = useState(false);

    const formatDate = (dateString) => {
        if (!dateString) return 'Never';
        return new Date(dateString).toLocaleString();
    };

    const parseConnectionString = (cs) => {
        const host = cs.match(/host=([^;]+)/i)?.[1] ?? 'N/A';
        const db   = cs.match(/database=([^;]+)/i)?.[1] ?? 'N/A';
        return { host, db };
    };

    const fetchData = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await fetchMicroserviceData();
            const list = data?.data ?? data?.items ?? data;
            if (Array.isArray(list)) {
                setMicroservices(list);
                setLastUpdated(new Date());
            } else {
                setError('Failed to fetch backup configuration');
            }
        } catch (err) {
            setError(err.message || 'An error occurred while fetching data');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 p-6 flex items-center justify-center">
                <div className="flex items-center space-x-2 text-gray-600">
                    <RefreshCw className="w-6 h-6 animate-spin" />
                    <span className="text-lg">Loading backup configuration...</span>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="min-h-screen bg-gray-50 p-6">
                <div className="max-w-7xl mx-auto">
                    <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
                        <h2 className="text-xl font-semibold text-red-800 mb-2">Error Loading Backup Configuration</h2>
                        <p className="text-red-600 mb-4">{error}</p>
                        <button
                            onClick={fetchData}
                            className="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors"
                        >
                            Retry
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    const db = microservices[0];

    return (
        <>
            <div className="min-h-screen bg-gray-50 p-6">
                <div className="max-w-7xl mx-auto">
                    {/* Header */}
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8 gap-4">
                        <div className="flex-1">
                            <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Database Backup</h1>
                            <p className="text-gray-600 mt-1">Backup and restore QalitrackDB</p>
                        </div>
                        <div className="flex items-center space-x-3">
                            {lastUpdated && (
                                <div className="hidden sm:flex text-sm text-gray-500 items-center">
                                    <Clock className="w-4 h-4 mr-1" />
                                    {lastUpdated.toLocaleTimeString()}
                                </div>
                            )}
                            <button
                                type="button"
                                onClick={() => setShowScheduledBackups(!showScheduledBackups)}
                                className="flex items-center px-3 py-1.5 bg-amber-500 text-white rounded hover:bg-amber-600 transition-colors text-sm"
                            >
                                <Calendar size={16} className="mr-1" />
                                {showScheduledBackups ? 'Hide Scheduled' : 'Scheduled Backups'}
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowAvailableBackupsModal(true)}
                                disabled={!db}
                                className="flex items-center px-3 py-1.5 bg-amber-500 text-white rounded hover:bg-amber-600 transition-colors text-sm disabled:opacity-50"
                            >
                                <Download size={16} className="mr-1" />
                                Restore / Download
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowBackupModal(true)}
                                className="flex items-center px-3 py-1.5 bg-amber-500 text-white rounded hover:bg-amber-700 transition-colors text-sm"
                            >
                                <HardDriveDownload size={16} className="mr-1" />
                                Backup Now
                            </button>
                            <button
                                onClick={fetchData}
                                disabled={loading}
                                className="bg-amber-500 text-white px-3 py-1.5 rounded-lg hover:bg-amber-600 transition-colors flex items-center space-x-1.5 text-sm"
                            >
                                <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
                                <span>Refresh</span>
                            </button>
                        </div>
                    </div>

                    {/* Scheduled Backups */}
                    {showScheduledBackups && (
                        <div className="mb-8">
                            <ScheduledBackup />
                        </div>
                    )}

                    {/* DB Info Card */}
                    {db ? (
                        <div className="bg-white rounded-md shadow-sm border overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-800">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Database</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Status</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Connection</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Last Backup</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white">
                                    <tr>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center gap-2">
                                                <Database className="w-4 h-4 text-amber-500" />
                                                <span className="text-sm font-medium text-gray-900">{db.name}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                                                db.status === 0
                                                    ? 'bg-green-100 text-green-800'
                                                    : db.status === 1
                                                        ? 'bg-red-100 text-red-800'
                                                        : 'bg-yellow-100 text-yellow-800'
                                            }`}>
                                                {db.status === 0 ? 'Active' : db.status === 1 ? 'Inactive' : 'Paused'}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4">
                                            {(() => {
                                                const { host, db: dbName } = parseConnectionString(db.connectionString);
                                                return (
                                                    <div className="flex flex-col gap-1">
                                                        <span className="flex items-center gap-1.5 text-xs">
                                                            <span className="text-gray-400 uppercase tracking-wide font-medium w-7">host</span>
                                                            <span className="font-mono text-gray-700">{host}</span>
                                                        </span>
                                                        <span className="flex items-center gap-1.5 text-xs">
                                                            <span className="text-gray-400 uppercase tracking-wide font-medium w-7">db</span>
                                                            <span className="font-mono text-gray-700">{dbName}</span>
                                                        </span>
                                                    </div>
                                                );
                                            })()}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                                            {formatDate(db.lastBackupAt)}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    ) : (
                        <div className="text-center py-12">
                            <Database className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                            <h3 className="text-lg font-medium text-gray-900 mb-2">No database configured</h3>
                            <p className="text-gray-600">Contact your administrator.</p>
                        </div>
                    )}
                </div>
            </div>

            {/* Backup Modal */}
            {showBackupModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
                        <div className="flex justify-between items-center border-b p-4">
                            <h2 className="text-xl font-semibold">Create New Backup</h2>
                            <button onClick={() => setShowBackupModal(false)} className="text-gray-500 hover:text-gray-700">
                                <X className="w-5 h-5" />
                            </button>
                        </div>
                        <div className="p-6">
                            <Backup onSuccess={() => setShowBackupModal(false)} />
                        </div>
                    </div>
                </div>
            )}

            {/* Available Backups / Restore Modal */}
            {showAvailableBackupsModal && db && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <AvailableBackups
                        microservice={db.name}
                        onClose={() => setShowAvailableBackupsModal(false)}
                    />
                </div>
            )}
        </>
    );
};

export default Microservices;
