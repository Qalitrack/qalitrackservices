import React, { useState, useEffect } from 'react';
import { backupAPI } from '../../../helpers/Backup/Backup';
import { RefreshCw, Clock, AlertCircle, CheckCircle, Download } from 'lucide-react';
import toast from 'react-hot-toast';

const AvailableBackups = ({ microservice, onClose }) => {
    const [backups, setBackups] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdated, setLastUpdated] = useState(null);

    const loadAvailableBackups = async () => {
        try {
            setLoading(true);
            const data = await backupAPI.getAvailableBackups(microservice);
            setBackups(data);
            setLastUpdated(new Date());
            setError(null);
        } catch (err) {
            console.error('Failed to load available backups:', err);
            setError('Failed to load available backups. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadAvailableBackups();
    }, [microservice]);

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString();
    };

    const getBackupType = (type) => {
        return type === 0 ? 'Full' : 'Incremental';
    };

    const formatFileSize = (bytes) => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const handleDownload = (backupId, fileName) => {
        // Placeholder for download functionality
        // In a real implementation, this would trigger a download via API or URL
        toast.success(`Download initiated for ${fileName}`);
    };

    return (
        <div className="relative bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[80vh] overflow-y-auto">
            <div className="flex justify-between items-center border-b p-4">
                <h2 className="text-xl font-semibold">Available Backups for {microservice}</h2>
                <button
                    onClick={onClose}
                    className="text-gray-500 hover:text-gray-700"
                >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>
            </div>
            <div className="p-6">
                <div className="flex justify-between items-center mb-4">
                    <div className="text-sm text-gray-500 flex items-center">
                        {lastUpdated && (
                            <>
                                <Clock className="h-3 w-3 mr-1" />
                                Last updated: {formatDate(lastUpdated)}
                            </>
                        )}
                    </div>
                    <button
                        type="button"
                        onClick={loadAvailableBackups}
                        disabled={loading}
                        className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-amber-600 hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 disabled:opacity-50"
                    >
                        <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
                        {loading ? 'Refreshing...' : 'Refresh'}
                    </button>
                </div>

                {error && (
                    <div className="mb-4 bg-red-50 border-l-4 border-red-400 p-4">
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

                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-300">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Backup ID</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">File Name</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Type</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Created At</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">File Size</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Latest</th>
                                <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200 bg-white">
                            {loading ? (
                                <tr>
                                    <td colSpan="7" className="px-3 py-4 text-sm text-gray-500 text-center">
                                        Loading available backups...
                                    </td>
                                </tr>
                            ) : backups.length === 0 ? (
                                <tr>
                                    <td colSpan="7" className="px-3 py-4 text-sm text-gray-500 text-center">
                                        No backups found for {microservice}.
                                    </td>
                                </tr>
                            ) : (
                                backups.map((backup) => (
                                    <tr key={backup.backupId} className="hover:bg-gray-50">
                                        <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                                            {backup.backupId}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                            {backup.fileName}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                            {getBackupType(backup.backupType)}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                            {formatDate(backup.createdAt)}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                            {formatFileSize(backup.fileSizeBytes)}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                                            {backup.isLatest ? (
                                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                    <CheckCircle className="h-3 w-3 mr-1" /> Yes
                                                </span>
                                            ) : 'No'}
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm font-medium">
                                            <button
                                                onClick={() => handleDownload(backup.backupId, backup.fileName)}
                                                className="text-amber-600 hover:text-amber-900"
                                                title="Download backup"
                                            >
                                                <Download className="h-4 w-4" />
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
    );
};

export default AvailableBackups;