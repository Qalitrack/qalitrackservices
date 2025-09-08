import React, { useState, useEffect } from 'react';

import { fetchPermissions, updatePermission, deletePermission, createPermission, fetchRolesForPermission } from '../../helpers/UserService/Permissions/permissions.js';
import { Edit, Trash2, ShieldAlert, PlusCircle, Users, FileText } from 'lucide-react';
import { format, parseISO } from 'date-fns';

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


    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 5000);
    };

    const loadPermissions = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchPermissions();
            setPermissions(data);
        } catch (err) {
            setError(err.message || 'Failed to fetch permissions.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadPermissions();
    }, []);

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

    const handleLogsClick = (permission) => {
        setSelectedPermission(permission);
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
        } catch (err) {
            console.error("Failed to create permission:", err);
            setModalFeedback({ text: err.message || 'Failed to create permission.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for confirming deletion
    const handleDelete = async () => {
        if (!selectedPermission) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await deletePermission(selectedPermission.id);
            await loadPermissions(); // Refresh the list
            setModalFeedback({ text: 'Permission deleted successfully!', type: 'success' });
            // Optionally close the modal after a delay or keep it open
            setTimeout(() => {
                setDeleteModalOpen(false);
            }, 2000);
        } catch (err) {
            console.error("Failed to delete permission:", err);
            setModalFeedback({ text: err.message || 'Failed to delete permission.', type: 'error' });
        } finally {
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
                <button
                    onClick={handleAddClick}
                    className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow"
                >
                    <PlusCircle size={18} />
                    <span>Add Permission</span>
                </button>
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
                        <th scope="col" className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {permissions.map((permission) => (
                        <tr key={permission.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{permission.name}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 hidden md:table-cell">{permission.description}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {format(parseISO(permission.updatedAt), "PPP")}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-4">
                                <button onClick={() => handleLogsClick(permission)} className="text-gray-600 hover:text-gray-900 transition-colors" title="View Logs">
                                    <FileText size={18} />
                                </button>
                                <button onClick={() => handleViewRolesClick(permission)} className="text-blue-600 hover:text-blue-900 transition-colors" title="View Roles">
                                    <Users size={18} />
                                </button>
                                <button onClick={() => handleEditClick(permission)} className="text-amber-600 hover:text-amber-900 transition-colors" title="Edit Permission">
                                    <Edit size={18} />
                                </button>
                                <button onClick={() => handleDeleteClick(permission)} className="text-red-600 hover:text-red-900 transition-colors" title="Delete Permission">
                                    <Trash2 size={18} />
                                </button>
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
                        <button onClick={handleDelete} disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 disabled:bg-gray-400">
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
                                <p className="text-gray-600">{selectedPermission.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Last Updated At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedPermission.updatedAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{selectedPermission.updatedBy || 'N/A'}</p>
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

