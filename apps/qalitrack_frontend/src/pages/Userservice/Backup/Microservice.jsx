import React, { useState, useEffect } from 'react';
import { fetchMicroserviceData, updateMicroservice, deleteMicroservice } from '../../../api/helpers/Backup/Microservice.js';
import { RefreshCw, Database, Clock, PlusCircle, Edit, Trash2, FileText, X, HardDriveDownload, Calendar, Download } from 'lucide-react';
import AddMicroservice from './AddMicroservice';
import EditMicroservice from './EditMicroservice';
import DeleteConfirmationModal from '../../../components/DeleteConfirmationModal';
import Backup from './Backup';
import ScheduledBackup from './ScheduledBackup';
import AvailableBackups from './AvailableBackups';

const Microservices = () => {
    const [microservices, setMicroservices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdated, setLastUpdated] = useState(null);
    const [showAddModal, setShowAddModal] = useState(false);
    const [editingService, setEditingService] = useState(null);
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [selectedService, setSelectedService] = useState(null);
    const [serviceToDelete, setServiceToDelete] = useState(null);
    const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
    const [deleteStatus, setDeleteStatus] = useState({ loading: false, error: null });
    const [userDetails, setUserDetails] = useState({});
    const [showBackupModal, setShowBackupModal] = useState(false);
    const [showScheduledBackups, setShowScheduledBackups] = useState(false);
    const [showAvailableBackupsModal, setShowAvailableBackupsModal] = useState(false);
    const [selectedMicroservice, setSelectedMicroservice] = useState(null);

    const handleEdit = (service) => {
        setEditingService(service);
    };

    const handleUpdateSuccess = (updatedService) => {
        setMicroservices((prev) =>
            prev.map((s) => (s.id === updatedService.id ? updatedService : s))
        );
        setEditingService(null);
    };

    const handleLogsClick = (service) => {
        setSelectedService(service);
        if (service.createdBy) loadUserDetails(service.createdBy);
        if (service.updatedBy) loadUserDetails(service.updatedBy);
        setLogsModalOpen(true);
    };

    const handleLogsModalClose = () => {
        setLogsModalOpen(false);
        setSelectedService(null);
    };

    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return;
        setUserDetails((prev) => ({
            ...prev,
            [userId]: `User ${userId}`,
        }));
    };

    const handleDeleteClick = (service) => {
        setServiceToDelete(service);
        setIsDeleteModalOpen(true);
    };

    const handleDeleteConfirm = async () => {
        if (!serviceToDelete) return;

        setDeleteStatus({ loading: true, error: null });

        try {
            await deleteMicroservice(serviceToDelete.name);
            setMicroservices((prev) => prev.filter((s) => s.id !== serviceToDelete.id));
            setIsDeleteModalOpen(false);
            setServiceToDelete(null);
        } catch (error) {
            console.error('Delete error:', error);
            setDeleteStatus({
                loading: false,
                error: error.message || 'Failed to delete service. Please try again.',
            });
        } finally {
            setDeleteStatus((prev) => ({ ...prev, loading: false }));
        }
    };

    const handleDeleteCancel = () => {
        setIsDeleteModalOpen(false);
        setServiceToDelete(null);
        setDeleteStatus({ loading: false, error: null });
    };

    const handleAvailableBackupsClick = (microservice) => {
        setSelectedMicroservice(microservice);
        setShowAvailableBackupsModal(true);
    };

    const handleAvailableBackupsClose = () => {
        setShowAvailableBackupsModal(false);
        setSelectedMicroservice(null);
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'Never';
        return new Date(dateString).toLocaleString();
    };

    const parseConnectionString = (cs) => {
        const host = cs.match(/host=([^;]+)/i)?.[1] ?? cs.match(/(?:postgres|mysql):\/\/[^:]+:[^@]+@([^:/]+)/i)?.[1] ?? 'N/A';
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
                setError('Failed to fetch microservices data');
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
            <div className="min-h-screen bg-gray-50 p-6">
                <div className="max-w-7xl mx-auto">
                    <div className="flex items-center justify-center min-h-64">
                        <div className="flex items-center space-x-2 text-gray-600">
                            <RefreshCw className="w-6 h-6 animate-spin" />
                            <span className="text-lg">Loading microservices...</span>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="min-h-screen bg-gray-50 p-6">
                <div className="max-w-7xl mx-auto">
                    <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
                        <h2 className="text-xl font-semibold text-red-800 mb-2">Error Loading Microservices</h2>
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

    const renderMicroservices = () => (
        <div className="min-h-screen bg-gray-50 p-6">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8 gap-4">
                    <div className="flex-1">
                        <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Microservices Dashboard</h1>
                        <p className="text-gray-600 mt-1">Monitor and manage your microservices</p>
                    </div>
                    <div className="flex items-center space-x-4">
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
                            <span>{showScheduledBackups ? 'Hide Scheduled Backups' : 'Show Scheduled Backups'}</span>
                        </button>
                        <button
                            type="button"
                            onClick={() => setShowBackupModal(true)}
                            className="flex items-center px-3 py-1.5 bg-amber-500 text-white rounded hover:bg-amber-700 transition-colors text-sm"
                        >
                            <HardDriveDownload size={16} className="mr-1" />
                            <span>Backup</span>
                        </button>
                        <button
                            type="button"
                            onClick={() => setShowAddModal(true)}
                            className="flex items-center px-3 py-1.5 bg-amber-500 text-white rounded hover:bg-amber-600 transition-colors text-sm"
                        >
                            <PlusCircle size={16} className="mr-1" />
                            <span>Add Service</span>
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

                {/* Scheduled Backups Section */}
                {showScheduledBackups && (
                    <div className="mb-8">
                        <ScheduledBackup />
                    </div>
                )}

                {/* Microservices Table */}
                <div className="bg-white rounded-md shadow-sm border overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-800">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Name</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Status</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Connection String</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Last Backup</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Backups</th>
                            <th className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Actions</th>
                        </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                        {microservices.map((service) => (
                            <tr key={service.id}>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{service.name}</td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                        <span
                                            className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                                                service.status === 0
                                                    ? 'bg-green-100 text-green-800'
                                                    : service.status === 1
                                                        ? 'bg-red-100 text-red-800'
                                                        : 'bg-yellow-100 text-yellow-800'
                                            }`}
                                        >
                                            {service.status === 0 ? 'Active' : service.status === 1 ? 'Inactive' : 'Paused'}
                                        </span>
                                </td>
                                <td className="px-6 py-4">
                                    {(() => { const { host, db } = parseConnectionString(service.connectionString); return (
                                        <div className="flex flex-col gap-1">
                                            <span className="flex items-center gap-1.5 text-xs">
                                                <span className="text-gray-400 uppercase tracking-wide font-medium w-7">host</span>
                                                <span className="font-mono text-gray-700 truncate max-w-[180px]" title={host}>{host}</span>
                                            </span>
                                            <span className="flex items-center gap-1.5 text-xs">
                                                <span className="text-gray-400 uppercase tracking-wide font-medium w-7">db</span>
                                                <span className="font-mono text-gray-700">{db}</span>
                                            </span>
                                            <span className="flex items-center gap-1.5 text-xs">
                                                <span className="text-gray-400 uppercase tracking-wide font-medium w-7">pwd</span>
                                                <span className="text-gray-400 tracking-widest">••••••</span>
                                            </span>
                                        </div>
                                    ); })()}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{formatDate(service.lastBackupAt)}</td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                    <button
                                        onClick={() => handleAvailableBackupsClick(service.name)}
                                        className="text-amber-600 hover:text-amber-900 p-1 rounded hover:bg-amber-50 transition-colors"
                                        title="View Backups"
                                        disabled={!!editingService || deleteStatus.loading}
                                    >
                                        <Download className="w-4 h-4" />
                                    </button>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleEdit(service)}
                                            className="text-amber-500 hover:text-amber-700 p-1 rounded hover:bg-amber-50 transition-colors"
                                            title="Edit"
                                            disabled={!!editingService}
                                        >
                                            <Edit className="w-4 h-4" />
                                        </button>
                                        <button
                                            onClick={() => handleDeleteClick(service)}
                                            className="text-red-600 hover:text-red-900 p-1 rounded hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                            title="Delete"
                                            disabled={!!editingService || deleteStatus.loading}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </button>
                                        <button
                                            onClick={() => handleLogsClick(service)}
                                            className="text-gray-600 hover:text-gray-900 p-1 rounded hover:bg-gray-50"
                                            title="View Logs"
                                            disabled={!!editingService}
                                        >
                                            <FileText className="w-4 h-4" />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>

                {microservices.length === 0 && !loading && (
                    <div className="text-center py-12">
                        <Database className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                        <h3 className="text-lg font-medium text-gray-900 mb-2">No microservices found</h3>
                        <p className="text-gray-600">There are no microservices to display at the moment.</p>
                    </div>
                )}
            </div>

            {/* Add Microservice Modal */}
            {showAddModal && (
                <AddMicroservice
                    onClose={() => setShowAddModal(false)}
                    onSuccess={(newService) => {
                        setMicroservices((prev) => [...prev, newService]);
                        setShowAddModal(false);
                    }}
                />
            )}
        </div>
    );

    return (
        <>
            {renderMicroservices()}
            {editingService && (
                <EditMicroservice
                    service={editingService}
                    onClose={() => setEditingService(null)}
                    onSuccess={handleUpdateSuccess}
                />
            )}
            {isLogsModalOpen && selectedService && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-md">
                        <div className="bg-amber-500 text-white px-6 py-4 rounded-t-lg flex justify-between items-center">
                            <h2 className="text-xl font-semibold">Audit Logs</h2>
                            <button onClick={handleLogsModalClose} className="text-white hover:text-gray-200">
                                <X size={24} />
                            </button>
                        </div>
                        <div className="p-6 space-y-4">
                            <div className="grid grid-cols-1 gap-4">
                                <div>
                                    <h3 className="font-medium text-gray-700 mb-2">Created</h3>
                                    <div className="bg-gray-50 p-3 rounded">
                                        <p className="text-sm text-gray-600">
                                            {selectedService.createdAt ? new Date(selectedService.createdAt).toLocaleString() : 'N/A'}
                                        </p>
                                    </div>
                                </div>
                                <div>
                                    <h3 className="font-medium text-gray-700 mb-2">Last Updated</h3>
                                    <div className="bg-gray-50 p-3 rounded">
                                        <p className="text-sm text-gray-600">
                                            {selectedService.updatedAt ? new Date(selectedService.updatedAt).toLocaleString() : 'N/A'}
                                        </p>
                                    </div>
                                </div>
                            </div>
                            <div className="mt-6 flex justify-end">
                                <button
                                    onClick={handleLogsModalClose}
                                    className="px-4 py-2 bg-amber-500 text-white rounded hover:bg-amber-600 transition-colors"
                                >
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
            <DeleteConfirmationModal
                isOpen={isDeleteModalOpen}
                onClose={handleDeleteCancel}
                onConfirm={handleDeleteConfirm}
                itemName={serviceToDelete?.name || 'this service'}
            />
            {showBackupModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
                        <div className="flex justify-between items-center border-b p-4">
                            <h2 className="text-xl font-semibold">Create New Backup</h2>
                            <button
                                onClick={() => setShowBackupModal(false)}
                                className="text-gray-500 hover:text-gray-700"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>
                        <div className="p-6">
                            <Backup onSuccess={() => setShowBackupModal(false)} />
                        </div>
                    </div>
                </div>
            )}
            {showAvailableBackupsModal && selectedMicroservice && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <AvailableBackups
                        microservice={selectedMicroservice}
                        onClose={handleAvailableBackupsClose}
                    />
                </div>
            )}
        </>
    );
};

export default Microservices;