import React, { useState, useEffect } from 'react';
import { fetchMicroserviceData } from '../../../api/helpers/Backup/Microservice.js';
import { RefreshCw, Database, Clock, HardDriveDownload, Calendar, Download, X } from 'lucide-react';
import PageHeader from '../../../components/PageHeader.jsx';
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
    const [scheduledRefreshKey, setScheduledRefreshKey] = useState(0);

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
            // Bumping this key also re-triggers ScheduledBackup's own fetch (it
            // watches this prop) — so the one page-level Refresh button covers
            // both data sources instead of needing a second button just for
            // the scheduled-backups list.
            setScheduledRefreshKey((k) => k + 1);
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

    const db = microservices[0];

    return (
        <>
            <div className="h-full flex flex-col rounded-lg shadow-md border border-gray-200 bg-white overflow-hidden">
                <PageHeader
                    icon={Database}
                    title="DATABASE BACKUP"
                    subtitle="Backup and restore QalitrackDB"
                    flush
                    className="border-b border-white/10"
                    actions={
                        <>
                            {lastUpdated && (
                                <div className="hidden sm:flex items-center gap-1 text-[10px]" style={{ color: 'var(--cs-appbar-text)', opacity: 0.7 }}>
                                    <Clock className="w-3 h-3" />
                                    {lastUpdated.toLocaleTimeString()}
                                </div>
                            )}
                            <button
                                type="button"
                                onClick={() => setShowScheduledBackups(!showScheduledBackups)}
                                className="flex items-center gap-1.5 px-3 py-1.5 border cs-solid-chip-btn rounded-lg text-xs font-semibold shadow-sm transition-all"
                            >
                                <Calendar size={14} />
                                {showScheduledBackups ? 'Hide Scheduled' : 'Scheduled Backups'}
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowAvailableBackupsModal(true)}
                                disabled={!db}
                                className="flex items-center gap-1.5 px-3 py-1.5 border cs-solid-chip-btn rounded-lg text-xs font-semibold shadow-sm transition-all disabled:opacity-50"
                            >
                                <Download size={14} />
                                Restore / Download
                            </button>
                            <button
                                type="button"
                                onClick={() => setShowBackupModal(true)}
                                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold shadow-md transition-all border-0"
                                style={{ backgroundColor: '#ffffff', color: 'var(--cs-appbar-bg)' }}
                            >
                                <HardDriveDownload size={14} />
                                Backup Now
                            </button>
                            <button
                                onClick={fetchData}
                                disabled={loading}
                                className="flex items-center gap-1.5 px-3 py-1.5 border cs-solid-chip-btn rounded-lg text-xs font-semibold shadow-sm transition-all disabled:opacity-50"
                            >
                                <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
                                Refresh
                            </button>
                        </>
                    }
                />

                <div className="flex-1 overflow-auto p-4 sm:p-6">
                    {/* Scheduled Backups */}
                    {showScheduledBackups && (
                        <div className="mb-6">
                            <ScheduledBackup refreshSignal={scheduledRefreshKey} />
                        </div>
                    )}

                    {error ? (
                        <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
                            <h2 className="text-base font-semibold text-red-800 mb-2">Error Loading Backup Configuration</h2>
                            <p className="text-red-600 text-sm mb-4">{error}</p>
                            <button
                                onClick={fetchData}
                                className="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors text-sm"
                            >
                                Retry
                            </button>
                        </div>
                    ) : loading && microservices.length === 0 ? (
                        <div className="flex items-center justify-center py-16">
                            <div className="flex items-center gap-2 text-gray-500">
                                <RefreshCw className="w-5 h-5 animate-spin" />
                                <span className="text-sm">Loading backup configuration...</span>
                            </div>
                        </div>
                    ) : db ? (
                        <table className="w-full compact-table">
                            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                                <tr>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Database</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Connection</th>
                                    <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Last Backup</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr className="border-b border-gray-100 bg-white">
                                    <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">1</td>
                                    <td className="px-3 py-2">
                                        <div className="flex items-center gap-2">
                                            <Database className="w-3.5 h-3.5 text-amber-500" />
                                            <span className="text-[10px] font-bold text-gray-900">{db.name}</span>
                                        </div>
                                    </td>
                                    <td className="px-3 py-2">
                                        <span className={`px-2 inline-flex text-[9px] leading-5 font-semibold rounded-full ${
                                            db.status === 0
                                                ? 'bg-green-100 text-green-800'
                                                : db.status === 1
                                                    ? 'bg-red-100 text-red-800'
                                                    : 'bg-yellow-100 text-yellow-800'
                                        }`}>
                                            {db.status === 0 ? 'Active' : db.status === 1 ? 'Inactive' : 'Paused'}
                                        </span>
                                    </td>
                                    <td className="px-3 py-2">
                                        {(() => {
                                            const { host, db: dbName } = parseConnectionString(db.connectionString);
                                            return (
                                                <div className="flex flex-col gap-1">
                                                    <span className="flex items-center gap-1.5 text-[10px]">
                                                        <span className="text-gray-400 uppercase tracking-wide font-medium w-7">host</span>
                                                        <span className="font-mono text-gray-700">{host}</span>
                                                    </span>
                                                    <span className="flex items-center gap-1.5 text-[10px]">
                                                        <span className="text-gray-400 uppercase tracking-wide font-medium w-7">db</span>
                                                        <span className="font-mono text-gray-700">{dbName}</span>
                                                    </span>
                                                </div>
                                            );
                                        })()}
                                    </td>
                                    <td className="px-3 py-2 text-[10px] text-gray-600">
                                        {formatDate(db.lastBackupAt)}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    ) : (
                        <div className="text-center py-12">
                            <Database className="w-12 h-12 text-gray-300 mx-auto mb-2" />
                            <p className="text-gray-500 text-sm font-medium">No database configured</p>
                            <p className="text-gray-400 text-xs mt-1">Contact your administrator.</p>
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
