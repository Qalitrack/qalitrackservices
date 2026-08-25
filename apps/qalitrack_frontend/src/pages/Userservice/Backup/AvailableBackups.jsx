import React, { useState, useEffect, useMemo } from 'react';
import { backupAPI } from '../../../api/helpers/Backup/Backup';
import { RefreshCw, Clock, AlertCircle, CheckCircle, Download, Upload, X, Layers, GitBranch } from 'lucide-react';
import toast from 'react-hot-toast';
import {backupApiClient} from "../../../api/helpers/BackupApiclient.js";

const RestoreConfirmation = ({ backup, onConfirm, onCancel, isRestoring }) => {
    if (!backup) return null;
    const isIncremental = backup.backupType === 1;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Confirm Restore</h3>
                    <button onClick={onCancel} className="text-gray-500 hover:text-gray-700">
                        <X className="h-5 w-5" />
                    </button>
                </div>
                <p className="mb-2 text-sm text-gray-700">
                    Restore to {isIncremental ? 'the incremental backup' : 'the full backup'} taken on{' '}
                    <span className="font-medium">{new Date(backup.createdAt).toLocaleString()}</span>? This action cannot be undone.
                </p>
                {backup.isPhysical && (
                    <p className="mb-4 text-sm bg-amber-50 border border-amber-200 rounded p-2 text-amber-800">
                        This restores the entire shared Postgres instance, not just this service — every
                        service is briefly unavailable while it stops, restores, and starts back up.
                        Anything written after this point in the chain will be lost, including{' '}
                        {isIncremental ? 'later incremental backups' : 'any incremental backups chained after it'}.
                    </p>
                )}
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

    const formatFileSize = (bytes) => {
        if (!bytes) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    // Groups the flat backup list into full+incremental chains so the chain
    // structure is visible instead of an undifferentiated list — a Full backup
    // is the chain's base, its Incrementals are nested underneath it in order.
    const chains = useMemo(() => {
        const byChain = new Map();
        backups.forEach((b) => {
            if (!byChain.has(b.chainId)) {
                byChain.set(b.chainId, { chainId: b.chainId, full: null, incrementals: [] });
            }
            const entry = byChain.get(b.chainId);
            if (b.backupType === 0) entry.full = b;
            else entry.incrementals.push(b);
        });

        return Array.from(byChain.values())
            .filter((c) => c.full)
            .sort((a, b) => new Date(b.full.createdAt) - new Date(a.full.createdAt))
            .map((c) => ({
                ...c,
                incrementals: [...c.incrementals].sort((x, y) => new Date(x.createdAt) - new Date(y.createdAt)),
            }));
    }, [backups]);

    const handleDownloadClick = (backup) => {
        if (backup.isPhysical) {
            toast.error("This is a pgBackRest physical backup — it can't be downloaded as a single file. Restore it in place instead.");
            return;
        }
        setBackupToDownload(backup);
    };

    const confirmDownload = async () => {
        if (!backupToDownload) return;

        try {
            const response = await backupApiClient.client.get(
                `/Backup/download/${encodeURIComponent(microservice)}/${encodeURIComponent(backupToDownload.backupId)}`,
                {
                    responseType: 'blob',
                    headers: {
                        'Accept': '*/*'
                    }
                }
            );

            const contentDisposition = response.headers['content-disposition'];
            let filename = backupToDownload.fileName || `backup-${backupToDownload.backupId}.dump`;

            if (contentDisposition) {
                const filenameMatch = contentDisposition.match(/filename[^;=]*=((['"]).*?\2|[^;\n]*)/);
                if (filenameMatch && filenameMatch[1]) {
                    filename = filenameMatch[1].replace(/['"]/g, '');
                }
            }

            const blob = new Blob([response.data], { type: response.headers['content-type'] });
            const url = window.URL.createObjectURL(blob);

            const link = document.createElement('a');
            link.href = url;
            link.download = filename;

            document.body.appendChild(link);
            link.click();

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

    const DownloadButton = ({ backup }) => (
        <button
            onClick={() => handleDownloadClick(backup)}
            className={`p-1 rounded border transition-all ${
                backup.isPhysical
                    ? 'text-gray-300 border-gray-200 cursor-not-allowed'
                    : 'text-amber-600 hover:bg-amber-50 border-amber-300 hover:border-amber-500 disabled:opacity-40'
            }`}
            title={backup.isPhysical ? "pgBackRest backups can't be downloaded as a single file" : 'Download backup'}
            disabled={isRestoring || backup.isPhysical}
        >
            <Download className="w-3 h-3" />
        </button>
    );

    const RestoreButton = ({ backup }) => (
        <button
            onClick={() => handleRestoreClick(backup)}
            disabled={isRestoring}
            className="p-1 rounded text-green-600 hover:bg-green-50 border border-green-300 hover:border-green-500 transition-all disabled:opacity-40"
            title="Restore to this point"
        >
            <Upload className="w-3 h-3" />
        </button>
    );

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

                    {loading && chains.length === 0 ? (
                        <div className="border border-gray-200 rounded-lg px-3 py-6 text-xs text-gray-500 text-center">
                            Loading available backups...
                        </div>
                    ) : chains.length === 0 ? (
                        <div className="border border-gray-200 rounded-lg px-3 py-6 text-xs text-gray-500 text-center">
                            No backups found for {microservice}.
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {chains.map((chain) => (
                                <div key={chain.chainId} className="border border-gray-200 rounded-lg overflow-hidden">
                                    {/* Full backup — the chain's base */}
                                    <div className="flex items-center justify-between gap-3 px-3 py-2.5 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                                        <div className="flex items-center gap-2.5 min-w-0">
                                            <Layers className="h-3.5 w-3.5 text-amber-600 shrink-0" />
                                            <div className="min-w-0">
                                                <div className="flex items-center gap-2">
                                                    <span className="text-[11px] font-bold text-gray-900">Full backup</span>
                                                    {chain.full.isLatest && (
                                                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[9px] font-semibold bg-green-100 text-green-800">
                                                            <CheckCircle className="h-3 w-3 mr-1" /> Latest chain
                                                        </span>
                                                    )}
                                                </div>
                                                <div className="text-[10px] text-gray-500 font-mono truncate">{chain.full.backupId}</div>
                                            </div>
                                        </div>
                                        <div className="flex items-center gap-4 shrink-0">
                                            <span className="text-[10px] text-gray-600">{formatDate(chain.full.createdAt)}</span>
                                            <span className="text-[10px] text-gray-600">{formatFileSize(chain.full.fileSizeBytes)}</span>
                                            <div className="flex items-center gap-1.5">
                                                <DownloadButton backup={chain.full} />
                                                <RestoreButton backup={chain.full} />
                                            </div>
                                        </div>
                                    </div>

                                    {/* Incrementals chained onto this full backup */}
                                    {chain.incrementals.length === 0 ? (
                                        <div className="px-3 py-2 text-[10px] text-gray-400 italic">No incremental backups on this chain yet.</div>
                                    ) : (
                                        <div className="divide-y divide-gray-100">
                                            {chain.incrementals.map((inc, i) => (
                                                <div key={inc.backupId} className="flex items-center justify-between gap-3 pl-8 pr-3 py-2 bg-white">
                                                    <div className="flex items-center gap-2 min-w-0">
                                                        <GitBranch className="h-3 w-3 text-gray-400 shrink-0" />
                                                        <div className="min-w-0">
                                                            <div className="flex items-center gap-2">
                                                                <span className="text-[10px] font-semibold text-gray-700">Incremental #{i + 1}</span>
                                                                {inc.isLatest && (
                                                                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[9px] font-semibold bg-green-100 text-green-800">
                                                                        <CheckCircle className="h-3 w-3 mr-1" /> Latest
                                                                    </span>
                                                                )}
                                                            </div>
                                                            <div className="text-[9px] text-gray-400 font-mono truncate">{inc.backupId}</div>
                                                        </div>
                                                    </div>
                                                    <div className="flex items-center gap-4 shrink-0">
                                                        <span className="text-[10px] text-gray-500">{formatDate(inc.createdAt)}</span>
                                                        <span className="text-[10px] text-gray-500">{formatFileSize(inc.fileSizeBytes)}</span>
                                                        <div className="flex items-center gap-1.5">
                                                            <DownloadButton backup={inc} />
                                                            <RestoreButton backup={inc} />
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default AvailableBackups;
