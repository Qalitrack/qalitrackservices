import React, { useState, useEffect } from 'react';
import { backupAPI } from '../../../api/helpers/Backup/Backup';
import { RefreshCw, Clock, AlertCircle, CheckCircle, Download, Upload, X } from 'lucide-react';
import toast from 'react-hot-toast';
import {backupApiClient} from "../../../api/helpers/BackupApiclient.js";

const RestoreConfirmation = ({ backup, onConfirm, onCancel, isRestoring }) => {
    if (!backup) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Confirm Restore</h3>
                    <button onClick={onCancel} className="text-gray-500 hover:text-gray-700">
                        <X className="h-5 w-5" />
                    </button>
                </div>
                <p className="mb-4">Are you sure you want to restore backup {backup.backupId}? This action cannot be undone.</p>
                <div className="flex justify-end space-x-3">
                    <button
                        onClick={onCancel}
                        disabled={isRestoring}
                        className="px-4 py-2 text-gray-600 bg-gray-200 rounded hover:bg-gray-300 disabled:opacity-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        disabled={isRestoring}
                        className="px-4 py-2 text-white bg-green-600 rounded hover:bg-green-700 disabled:opacity-50"
                    >
                        {isRestoring ? 'Restoring...' : 'Confirm Restore'}
                    </button>
                </div>
            </div>
        </div>
    );
};

const RestoreSuccess = ({ result, onClose }) => {
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Restore Successful</h3>
                    <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
                        <X className="h-5 w-5" />
                    </button>
                </div>
                <p className="mb-4 text-green-600">Backup restoration has been initiated successfully.</p>
                <div className="flex justify-end">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 text-white bg-green-600 rounded hover:bg-green-700"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
};

const DownloadConfirmation = ({ backup, onConfirm, onCancel }) => {
    if (!backup) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Confirm Download</h3>
                    <button onClick={onCancel} className="text-gray-500 hover:text-gray-700">
                        <X className="h-5 w-5" />
                    </button>
                </div>
                <p className="mb-4">You are about to download backup <span className="font-medium">{backup.fileName}</span>. Would you like to continue?</p>
                <div className="flex justify-end space-x-3">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-gray-600 bg-gray-200 rounded hover:bg-gray-300"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 text-white bg-blue-600 rounded hover:bg-blue-700"
                    >
                        Download
                    </button>
                </div>
            </div>
        </div>
    );
};

const AvailableBackups = ({ microservice, onClose }) => {
    const [backups, setBackups] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdated, setLastUpdated] = useState(null);
    const [isRestoring, setIsRestoring] = useState(false);
    const [currentBackup, setCurrentBackup] = useState(null);
    const [backupToDownload, setBackupToDownload] = useState(null);
    const [showConfirmation, setShowConfirmation] = useState(false);
    const [restoreResult, setRestoreResult] = useState(null);

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

    const handleDownloadClick = (backup) => {
        setBackupToDownload(backup);
    };

    const confirmDownload = async () => {
        if (!backupToDownload) return;
        
        try {
            // Use the existing backupApiClient to make the request
            const response = await backupApiClient.client.get(
                `/Backup/download/${encodeURIComponent(microservice)}/${encodeURIComponent(backupToDownload.backupId)}`,
                {
                    responseType: 'blob', // Important for file downloads
                    headers: {
                        'Accept': '*/*'
                    }
                }
            );

            // Get the filename from content-disposition header or use a fallback
            const contentDisposition = response.headers['content-disposition'];
            let filename = backupToDownload.fileName || `backup-${backupToDownload.backupId}.dump`;
            
            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            // Create a blob URL for the file
            const blob = new Blob([response.data], { type: response.headers['content-type'] });
            const url = window.URL.createObjectURL(blob);
            
            // Create a temporary link element
            const link = document.createElement('a');
            link.href = url;
            link.download = filename;
            
            // Append to body (required for Firefox)
            document.body.appendChild(link);
            
            // Trigger the download
            link.click();
            
            // Clean up
            setTimeout(() => {
                window.URL.revokeObjectURL(url);
                document.body.removeChild(link);
            }, 100);
            
            toast.success('Download started successfully');
        } catch (error) {
            console.error('Failed to initiate download:', error);
            toast.error(`Failed to start download: ${error.message || 'Unknown error'}`);
        } finally {
            setBackupToDownload(null);
        }
    };

    const handleRestoreClick = (backup) => {
        setCurrentBackup(backup);
        setShowConfirmation(true);
    };

    const confirmRestore = async () => {
        if (!currentBackup) return;

        try {
            setIsRestoring(true);

            console.log('Initiating backup restore:', {
                backupId: currentBackup.backupId,
                microservice
            });

            const result = await backupAPI.restoreBackupById(currentBackup.backupId, microservice);
            setRestoreResult(result);
            toast.success('Backup restoration started successfully');

            // Refresh the backups list
            loadAvailableBackups();
        } catch (error) {
            console.error('Failed to restore backup:', error);
            toast.error(`Failed to restore backup: ${error.message || 'Unknown error'}`);
        } finally {
            setIsRestoring(false);
        }
        setShowConfirmation(false);
    };

    const closeSuccessDialog = () => {
        setRestoreResult(null);
    };

    const cancelDownload = () => {
        setBackupToDownload(null);
    };

    return (
        <>
            {showConfirmation && (
                <RestoreConfirmation
                    backup={currentBackup}
                    onConfirm={confirmRestore}
                    onCancel={() => setShowConfirmation(false)}
                    isRestoring={isRestoring}
                />
            )}

            {restoreResult && (
                <RestoreSuccess
                    result={restoreResult}
                    onClose={closeSuccessDialog}
                />
            )}

            {backupToDownload && (
                <DownloadConfirmation
                    backup={backupToDownload}
                    onConfirm={confirmDownload}
                    onCancel={cancelDownload}
                />
            )}

            <div className="relative bg-white rounded-lg shadow-xl w-full max-w-6xl max-h-[80vh] overflow-y-auto">
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
                                <th className="px-3 py-3.5 text-center text-sm font-semibold text-gray-900">Download</th>
                                <th className="px-3 py-3.5 text-center text-sm font-semibold text-gray-900">Restore</th>
                            </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200 bg-white">
                            {loading ? (
                                <tr>
                                    <td colSpan="8" className="px-3 py-4 text-sm text-gray-500 text-center">
                                        Loading available backups...
                                    </td>
                                </tr>
                            ) : backups.length === 0 ? (
                                <tr>
                                    <td colSpan="8" className="px-3 py-4 text-sm text-gray-500 text-center">
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
                                        <td className="whitespace-nowrap px-3 py-4 text-sm font-medium text-center">
                                            <button
                                                onClick={() => handleDownloadClick(backup)}
                                                className="text-amber-600 hover:text-amber-900"
                                                title="Download backup"
                                                disabled={isRestoring}
                                            >
                                                <Download className="h-4 w-4" />
                                            </button>
                                        </td>
                                        <td className="whitespace-nowrap px-3 py-4 text-sm font-medium text-center">
                                            <button
                                                onClick={() => handleRestoreClick(backup)}
                                                disabled={isRestoring}
                                                className="text-green-600 hover:text-green-900 disabled:opacity-50"
                                                title="Restore from backup"
                                            >
                                                <Upload className="h-4 w-4" />
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
        </>
    );
};

export default AvailableBackups;