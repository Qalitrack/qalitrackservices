import React, { useState, useEffect } from 'react';

import { fetchPermissions, updatePermission, deletePermission, createPermission, fetchRolesForPermission, fetchDeletedPermissions, restorePermission } from '../../api/helpers/UserService/Permissions/permissions.js';
import { fetchUserById } from '../../api/helpers/UserService/Users/users.js';
import { Edit, Trash2, ShieldAlert, PlusCircle, Users, FileText, RefreshCw, Download } from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

// A reusable Modal component
const Modal = ({ children, isOpen, onClose }) => {
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md m-4">
                {children}
            </div>
        </div>
    );
};

const Permissions = () => {
    const [permissions, setPermissions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [showDeleted, setShowDeleted] = useState(false);
    const [actionLoading, setActionLoading] = useState(null);

    // State for modals
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [isRolesModalOpen, setRolesModalOpen] = useState(false);
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [selectedPermission, setSelectedPermission] = useState(null);
    const [rolesForPermission, setRolesForPermission] = useState([]);
    const [newPermission, setNewPermission] = useState({ name: '', description: '' });
    const [isUpdating, setIsUpdating] = useState(false);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [modalFeedback, setModalFeedback] = useState({ text: '', type: '' });
    const [userDetails, setUserDetails] = useState({});

    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 5000);
    };

    const loadPermissions = async (deleted) => {
        setLoading(true);
        setError(null);
        try {
            const fetchFunction = deleted ? fetchDeletedPermissions : fetchPermissions;
            const data = await fetchFunction();
            setPermissions(data);
        } catch (err) {
            setError(err.message || 'Failed to fetch permissions.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadPermissions(showDeleted);
    }, [showDeleted]);

    const fetchAllPermissions = async () => {
        try {
            const data = await fetchPermissions(); // Fetch active permissions for PDF
            return data;
        } catch (error) {
            console.error('Error fetching all permissions:', error);
            throw error;
        }
    };

    const generatePDF = async () => {
        setLoading(true);
        try {
            const allPermissions = await fetchAllPermissions();

            const permissionsWithRoles = await Promise.all(
                allPermissions.map(async (permission) => {
                    try {
                        const roles = await fetchRolesForPermission(permission.id);
                        const formattedRoles = roles.map(role => role.name || 'Unknown').join('\n');
                        return {
                            ...permission,
                            assignedRolesList: formattedRoles || 'None'
                        };
                    } catch (err) {
                        console.warn(`Failed to fetch roles for permission ${permission.id}:`, err);
                        return {
                            ...permission,
                            assignedRolesList: 'Error fetching roles'
                        };
                    }
                })
            );

            const doc = new jsPDF({
                orientation: 'landscape'
            });

            doc.setFontSize(18);
            doc.text('Permissions Report', 14, 22);
            doc.setFontSize(11);
            doc.setTextColor(100);
            doc.text(`Generated on: ${new Date().toLocaleString()}`, 14, 30);
            doc.text(`Total Permissions: ${permissionsWithRoles.length}`, 14, 38);

            const columns = [
                { header: 'Name', dataKey: 'name', cellWidth: 'auto' },
                { header: 'Description', dataKey: 'description', cellWidth: 'wrap' },
                { header: 'Created At', dataKey: 'createdAt', cellWidth: 'wrap' },
                { header: 'Last Updated', dataKey: 'updatedAt', cellWidth: 'wrap' },
                { header: 'Assigned Roles', dataKey: 'assignedRolesList', cellWidth: 'wrap' }
            ];

            const data = permissionsWithRoles.map(permission => ({
                name: permission.name || 'N/A',
                description: permission.description || 'N/A',
                createdAt: permission.createdAt ? format(parseISO(permission.createdAt), "PPpp") : 'N/A',
                updatedAt: permission.updatedAt ? format(parseISO(permission.updatedAt), "PPpp") : 'N/A',
                assignedRolesList: permission.assignedRolesList !== 'None' ? { content: permission.assignedRolesList } : 'None'
            }));

            const columnStyles = {
                // Default styles for all columns
                default: {
                    cellPadding: 3,
                    overflow: 'linebreak',
                    lineWidth: 0.1,
                    valign: 'top'
                },
                // Smaller font size for date columns (index 2 and 3)
                2: { 
                    fontSize: 8,
                    minCellWidth: 40
                },
                3: { 
                    fontSize: 8,
                    minCellWidth: 40
                },
            };
            
            // Apply cell width settings
            columns.forEach((col, index) => {
                if (!columnStyles[index]) columnStyles[index] = {};
                columnStyles[index] = {
                    ...columnStyles[index],
                    cellWidth: col.cellWidth === 'auto' ? 'auto' : undefined,
                    minCellWidth: col.cellWidth === 'wrap' ? 50 : columnStyles[index].minCellWidth || 30,
                };
                
                if (col.dataKey === 'assignedRolesList') {
                    columnStyles[index].cellWidth = 100;
                }
            });

            autoTable(doc, {
                head: [columns.map(col => col.header)],
                body: data.map(row => columns.map(col => row[col.dataKey])),
                startY: 40,
                styles: {
                    fontSize: 8,
                    cellPadding: 1,
                    overflow: 'linebreak',
                    lineWidth: 0.1,
                    textColor: [0, 0, 0],
                    fontStyle: 'normal'
                },
                headStyles: {
                    fillColor: [41, 128, 185],
                    textColor: 255,
                    fontStyle: 'bold',
                    lineWidth: 0.1,
                    fontSize: 9
                },
                columnStyles,
                alternateRowStyles: {
                    fillColor: [245, 245, 245]
                },
                margin: {
                    top: 40,
                    right: 10,
                    bottom: 20,
                    left: 10
                },
                tableWidth: 'wrap',
                showHead: 'everyPage',
                willDrawPage: function(data) {
                    const pageSize = doc.internal.pageSize;
                    const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                    const pageNumber = data.pageNumber || 1;
                    const pageCount = data.pageCount || 1;
                    if (pageNumber && pageCount) {
                        doc.setFontSize(10);
                        doc.text(
                            `Page ${pageNumber} of ${pageCount}`,
                            data.settings.margin.left,
                            pageHeight - 10
                        );
                    }
                }
            });

            doc.save(`permissions-report-${new Date().toISOString().split('T')[0]}.pdf`);

            return true;
        } catch (error) {
            console.error('Error generating PDF:', error);
            setError('Failed to generate PDF: ' + (error.message || 'Unknown error'));
            return false;
        } finally {
            setLoading(false);
        }
    };

    const handleDownloadPDF = async () => {
        const success = await generatePDF();
        if (success) {
            // Optional: Show success message
        }
    };

    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return; // Don't fetch if no ID or already fetched

        try {
            const user = await fetchUserById(userId);
            setUserDetails(prev => ({ ...prev, [userId]: user.email }));
        } catch (error) {
            console.error(`Failed to fetch user ${userId}`, error);
            setUserDetails(prev => ({ ...prev, [userId]: 'Unknown' })); // Handle error case
        }
    };

    // Handlers for opening modals
    const handleAddClick = () => {
        setNewPermission({ name: '', description: '' });
        setModalFeedback({ text: '', type: '' }); // Clear previous feedback
        setAddModalOpen(true);
    };

    const handleEditClick = (permission) => {
        setSelectedPermission({ ...permission });
        setModalFeedback({ text: '', type: '' }); // Clear previous feedback
        setEditModalOpen(true);
    };

    const handleDeleteClick = (permission) => {
        setSelectedPermission(permission);
        setModalFeedback({ text: '', type: '' }); // Clear previous feedback
        setDeleteModalOpen(true);
    };

    const handleToggleShowDeleted = () => {
        setShowDeleted(prev => !prev);
    };

    const handleLogsClick = (permission) => {
        setSelectedPermission(permission);
        loadUserDetails(permission.createdBy);
        loadUserDetails(permission.updatedBy);
        setLogsModalOpen(true);
    };

    const handleViewRolesClick = async (permission) => {
        setSelectedPermission(permission);
        setRolesModalOpen(true);
        try {
            const roles = await fetchRolesForPermission(permission.id);
            setRolesForPermission(roles);
        } catch (err) {
            console.error("Failed to fetch roles:", err);
            setRolesForPermission([]); // Reset on error
        }
    };

    // Handler for form input change in edit modal
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setSelectedPermission(prev => ({ ...prev, [name]: value }));
    };

    // Handler for form input change in add modal
    const handleNewPermissionChange = (e) => {
        const { name, value } = e.target;
        setNewPermission(prev => ({ ...prev, [name]: value }));
    };

    // Handler for submitting the edit form
    const handleUpdate = async (e) => {
        e.preventDefault();
        if (!selectedPermission) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await updatePermission(selectedPermission);
            await loadPermissions(); // Refresh the list
            setModalFeedback({ text: 'Permission updated successfully!', type: 'success' });
            setTimeout(() => {
                setEditModalOpen(false);
            }, 3000);
        } catch (err) {
            console.error("Failed to update permission:", err);
            setModalFeedback({ text: err.message || 'Failed to update permission.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for creating a new permission
    const handleCreate = async (e) => {
        e.preventDefault();
        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await createPermission(newPermission);
            setNewPermission({ name: '', description: '' }); // Clear form
            await loadPermissions(); // Refresh the list
            setModalFeedback({ text: 'Permission created successfully!', type: 'success' });
            setTimeout(() => {
                setAddModalOpen(false);
            }, 3000);
        } catch (err) {
            console.error("Failed to create permission:", err);
            setModalFeedback({ text: err.message || 'Failed to create permission.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for confirming deletion
    const handleToggleDelete = async (permission) => {
        const action = permission.isDeleted ? 'restore' : 'delete';
        const actionVerb = permission.isDeleted ? 'restored' : 'deleted';

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            if (permission.isDeleted) {
                await restorePermission(permission.id);
            } else {
                await deletePermission(permission.id);
            }
            setModalFeedback({ text: `Permission successfully ${actionVerb}.`, type: 'success' });
            loadPermissions(showDeleted);
            // Don't close the modal immediately to show success message
            setTimeout(() => {
                setDeleteModalOpen(false);
                showMessage(`Permission successfully ${actionVerb}.`, 'success');
            }, 1500);
        } catch (err) {
            console.error(`Failed to ${action} permission:`, err);
            setModalFeedback({ text: err.message || `Failed to ${action} permission.`, type: 'error' });
            setIsUpdating(false);
        }
    };

    if (loading) {
        return <div className="flex justify-center items-center h-32"><div>Loading permissions...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
    }

    return (
        <div className="bg-white shadow-lg rounded-xl p-4 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl md:text-2xl font-bold text-gray-800">Manage Permissions</h2>
                <div className="flex items-center gap-4">
                    <div className="flex items-center gap-2">
                        <label htmlFor="show-deleted" className="text-sm font-medium text-gray-700">Show Deleted</label>
                        <input
                            type="checkbox"
                            id="show-deleted"
                            checked={showDeleted}
                            onChange={handleToggleShowDeleted}
                            className="h-4 w-4 rounded border-gray-300 text-amber-600 focus:ring-amber-500"
                        />
                    </div>
                    <button
                        onClick={handleDownloadPDF}
                        className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow"
                    >
                        <Download size={18} />
                        <span>Download PDF</span>
                    </button>
                    <button
                        onClick={handleAddClick}
                        className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow"
                    >
                        <PlusCircle size={18} />
                        <span>Add Permission</span>
                    </button>
                </div>
            </div>

            {feedbackMessage.text && (
                <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${feedbackMessage.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {feedbackMessage.text}
                </div>
            )}

            <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-800">
                    <tr>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Name</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider hidden md:table-cell">Description</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Actions</th>

                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {permissions.map((permission) => (
                        <tr key={permission.id} className={`hover:bg-gray-50 ${permission.isDeleted ? 'opacity-60 bg-gray-100' : ''}`}>
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{permission.name}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 hidden md:table-cell">{permission.description}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {format(parseISO(permission.updatedAt), "PPP")}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-left text-sm font-medium space-x-4">
                                <button onClick={() => handleLogsClick(permission)} className="text-gray-600 hover:text-gray-900 transition-colors" title="View Logs">
                                    <FileText size={18} />
                                </button>
                                {!showDeleted && (
                                    <>
                                        <button onClick={() => handleViewRolesClick(permission)} className="text-blue-600 hover:text-blue-900 transition-colors" title="View Roles">
                                            <Users size={18} />
                                        </button>
                                        <button onClick={() => handleEditClick(permission)} className="text-amber-600 hover:text-amber-900 transition-colors" title="Edit Permission" disabled={permission.isDeleted}>
                                            <Edit size={18} />
                                        </button>
                                    </>
                                )}
                                {!showDeleted && (
                                    <button
                                        onClick={() => handleDeleteClick(permission)}
                                        className={`text-red-600 hover:text-red-800 transition-colors ${actionLoading === permission.id ? 'opacity-50 cursor-not-allowed' : ''}`}
                                        title="Delete Permission"
                                        disabled={actionLoading === permission.id}
                                    >
                                        <Trash2 size={18} />
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            {/* Edit Modal */}
            <Modal isOpen={isEditModalOpen} onClose={() => setEditModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Edit Permission</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedPermission && (
                    <form onSubmit={handleUpdate} className="space-y-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700">Name</label>
                            <input
                                type="text"
                                name="name"
                                id="name"
                                value={selectedPermission.name}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description</label>
                            <textarea
                                name="description"
                                id="description"
                                value={selectedPermission.description}
                                onChange={handleInputChange}
                                rows="3"
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            ></textarea>
                        </div>
                        <div className="flex justify-end space-x-3 pt-4">
                            <button type="button" onClick={() => setEditModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                                Close
                            </button>
                            <button type="submit" disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300">
                                {isUpdating ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                )}
            </Modal>

            {/* Add Modal */}
            <Modal isOpen={isAddModalOpen} onClose={() => setAddModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Add New Permission</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                <form onSubmit={handleCreate} className="space-y-4">
                    <div>
                        <label htmlFor="newName" className="block text-sm font-medium text-gray-700">Name</label>
                        <input
                            type="text"
                            name="name"
                            id="newName"
                            value={newPermission.name}
                            onChange={handleNewPermissionChange}
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        />
                    </div>
                    <div>
                        <label htmlFor="newDescription" className="block text-sm font-medium text-gray-700">Description</label>
                        <textarea
                            name="description"
                            id="newDescription"
                            value={newPermission.description}
                            onChange={handleNewPermissionChange}
                            rows="3"
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        ></textarea>
                    </div>
                    <div className="flex justify-end space-x-3 pt-4">
                        <button type="button" onClick={() => setAddModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Close
                        </button>
                        <button type="submit" disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300">
                            {isUpdating ? 'Adding...' : 'Add Permission'}
                        </button>
                    </div>
                </form>
            </Modal>

            {/* Delete Confirmation Modal */}
            <Modal isOpen={isDeleteModalOpen} onClose={() => setDeleteModalOpen(false)}>
                <div className="text-center">
                    <ShieldAlert className="mx-auto h-12 w-12 text-red-500" />
                    <h3 className="mt-2 text-lg font-bold text-gray-800">Delete Permission</h3>
                    <p className="mt-2 text-sm text-gray-600">
                        Are you sure you want to delete the permission "{selectedPermission?.name}"? This action cannot be undone.
                    </p>
                    {modalFeedback.text && (
                        <div className={`mt-4 p-3 rounded-lg text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                            {modalFeedback.text}
                        </div>
                    )}
                </div>
                {!modalFeedback.text || modalFeedback.type !== 'success' ? (
                    <div className="mt-6 flex justify-center space-x-4">
                        <button onClick={() => setDeleteModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Cancel
                        </button>
                        <button onClick={() => handleToggleDelete(selectedPermission)} disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 disabled:bg-gray-400">
                            {isUpdating ? 'Deleting...' : 'Delete'}
                        </button>
                    </div>
                ) : (
                    <div className="mt-6 flex justify-center">
                        <button onClick={() => setDeleteModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Close
                        </button>
                    </div>
                )}
            </Modal>

            {/* Roles Modal */}
            <Modal isOpen={isRolesModalOpen} onClose={() => setRolesModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Roles with "{selectedPermission?.name}"</h3>
                {rolesForPermission.length > 0 ? (
                    <ul className="space-y-2">
                        {rolesForPermission.map(role => (
                            <li key={role.id} className="bg-gray-100 p-3 rounded-md text-sm font-medium">{role.name}</li>
                        ))}
                    </ul>
                ) : (
                    <p className="text-sm text-gray-500">No roles are assigned to this permission.</p>
                )}
                <div className="flex justify-end pt-4">
                    <button type="button" onClick={() => setRolesModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                        Close
                    </button>
                </div>
            </Modal>

            {/* Logs Modal */}
            <Modal isOpen={isLogsModalOpen} onClose={() => setLogsModalOpen(false)}>
                <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                    <h3 className="text-lg font-bold mb-4">Audit Logs for "{selectedPermission?.name}"</h3>
                    {selectedPermission && (
                        <div className="space-y-4">
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedPermission.createdAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created By:</p>
                                <p className="text-gray-600">{userDetails[selectedPermission.createdBy] || selectedPermission.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Last Updated At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedPermission.updatedAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{userDetails[selectedPermission.updatedBy] || selectedPermission.updatedBy || 'N/A'}</p>
                            </div>
                        </div>
                    )}
                    <div className="flex justify-end mt-6">
                        <button
                            type="button"
                            onClick={() => setLogsModalOpen(false)}
                            className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white rounded-md text-sm font-medium transition-colors duration-200"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

export default Permissions;