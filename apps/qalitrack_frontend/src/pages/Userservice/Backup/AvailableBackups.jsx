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
                <p className="mb-4 text-sm text-gray-700">Are you sure you want to restore backup {backup.backupId}? This action cannot be undone.</p>
                <div className="flex justify-end space-x-3">
                    <button
                        onClick={onCancel}
                        disabled={isRestoring}
                        className="px-4 py-2 text-sm font-semibold border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        disabled={isRestoring}
                        className="px-4 py-2 text-sm font-semibold text-white bg-green-600 rounded-lg shadow hover:bg-green-700 transition-all disabled:opacity-50"
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
                <p className="mb-4 text-sm text-green-600">Backup restoration has been initiated successfully.</p>
                <div className="flex justify-end">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 text-sm font-semibold text-white bg-green-600 rounded-lg shadow hover:bg-green-700 transition-all"
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
                <p className="mb-4 text-sm text-gray-700">You are about to download backup <span className="font-medium">{backup.fileName}</span>. Would you like to continue?</p>
                <div className="flex justify-end space-x-3">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-sm font-semibold border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 text-sm font-semibold text-white bg-amber-500 rounded-lg shadow hover:bg-amber-600 transition-all"
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


            const result = await backupAPI.restoreBackupById(currentBackup.backupId, microservice);
            setRestoreResult(result);
            toast.success('Backup restoration started successfully');

            // Refresh the backups list
            loadAvailableBackups();
        } catch (error) {
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
                        <X className="w-5 h-5" />
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
                            className="inline-flex items-center px-3 py-1.5 text-xs font-semibold rounded-lg shadow-sm text-white bg-amber-500 hover:bg-amber-600 transition-all disabled:opacity-50"
                        >
                            <RefreshCw className={`h-3.5 w-3.5 mr-1.5 ${loading ? 'animate-spin' : ''}`} />
                            {loading ? 'Refreshing...' : 'Refresh'}
                        </button>
                    </div>

                    {error && (
                        <div className="mb-4 bg-red-50 border border-red-200 rounded-lg p-3">
                            <div className="flex items-center">
                                <AlertCircle className="h-4 w-4 text-red-400 mr-2 shrink-0" aria-hidden="true" />
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        </div>
                    )}

                    <div className="overflow-x-auto border border-gray-200 rounded-lg">
                        <table className="w-full compact-table">
                            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                            <tr>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Backup ID</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">File Name</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Type</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Created At</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">File Size</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Latest</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Download</th>
                                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Restore</th>
                            </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100 bg-white">
                            {loading && backups.length === 0 ? (
                                <tr>
                                    <td colSpan="9" className="px-3 py-4 text-xs text-gray-500 text-center">
                                        Loading available backups...
                                    </td>
                                </tr>
                            ) : backups.length === 0 ? (
                                <tr>
                                    <td colSpan="9" className="px-3 py-4 text-xs text-gray-500 text-center">
                                        No backups found for {microservice}.
                                    </td>
                                </tr>
                            ) : (
                                backups.map((backup, index) => (
                                    <tr key={backup.backupId} className={`hover:bg-amber-50/40 transition-all ${index % 2 === 0 ? "bg-white" : "bg-gray-50"}`}>
                                        <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">{index + 1}</td>
                                        <td className="px-3 py-2 text-[10px] font-bold text-gray-900">
                                            {backup.backupId}
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600 font-mono">
                                            {backup.fileName}
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600">
                                            {getBackupType(backup.backupType)}
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600">
                                            {formatDate(backup.createdAt)}
                                        </td>
                                        <td className="px-3 py-2 text-[10px] text-gray-600">
                                            {formatFileSize(backup.fileSizeBytes)}
                                        </td>
                                        <td className="px-3 py-2 text-[10px]">
                                            {backup.isLatest ? (
                                                <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[9px] font-semibold bg-green-100 text-green-800">
                                                        <CheckCircle className="h-3 w-3 mr-1" /> Yes
                                                    </span>
                                            ) : 'No'}
                                        </td>
                                        <td className="px-3 py-2 text-center">
                                            <button
                                                onClick={() => handleDownloadClick(backup)}
                                                className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all disabled:opacity-40"
                                                title="Download backup"
                                                disabled={isRestoring}
                                            >
                                                <Download className="w-3 h-3" />
                                            </button>
                                        </td>
                                        <td className="px-3 py-2 text-center">
                                            <button
                                                onClick={() => handleRestoreClick(backup)}
                                                disabled={isRestoring}
                                                className="p-1 rounded text-green-600 hover:bg-green-50 border border-green-300 hover:border-green-500 transition-all disabled:opacity-40"
                                                title="Restore from backup"
                                            >
                                                <Upload className="w-3 h-3" />
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