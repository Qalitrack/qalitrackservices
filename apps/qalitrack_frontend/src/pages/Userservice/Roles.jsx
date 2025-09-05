import React, { useState, useEffect } from 'react';
import {
    fetchRoles,
    updateRole,
    deleteRole,
    createRole,
    getPermissionsForRole,
    assignPermissionToRole,
    removePermissionFromRole
} from '../../helpers/UserService/Roles/Roles.js';
import { fetchPermissions } from '../../helpers/UserService/Permissions/permissions.js';
import { Edit, Trash2, PlusCircle, Users, FileText, ShieldCheck } from 'lucide-react';
import { format, parseISO } from 'date-fns';

// A reusable Modal component
const Modal = ({ children, isOpen, onClose, size = 'md' }) => {
    if (!isOpen) return null;

    const sizeClasses = {
        sm: 'max-w-sm',
        md: 'max-w-md',
        lg: 'max-w-lg',
        xl: 'max-w-xl',
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className={`bg-white rounded-lg shadow-xl p-6 w-full m-4 ${sizeClasses[size]}`}>
                {children}
            </div>
        </div>
    );
};

const Roles = () => {
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // State for modals
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [isUserListModalOpen, setUserListModalOpen] = useState(false);
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [isPermissionsModalOpen, setPermissionsModalOpen] = useState(false);

    const [selectedRole, setSelectedRole] = useState(null);
    const [newRole, setNewRole] = useState({ name: '', description: '', isActive: true });
    const [isUpdating, setIsUpdating] = useState(false);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });

    // State for permission management
    const [allPermissions, setAllPermissions] = useState([]);
    const [rolePermissions, setRolePermissions] = useState([]);
    const [loadingPermissions, setLoadingPermissions] = useState(false);

    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 5000);
    };

    const loadRoles = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchRoles();
            setRoles(data);
        } catch (err) {
            setError(err.message || 'Failed to fetch roles.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadRoles();
    }, []);

    // Handlers for opening modals
    const handleAddClick = () => {
        setNewRole({ name: '', description: '', isActive: true });
        setAddModalOpen(true);
    };

    const handleEditClick = (role) => {
        setSelectedRole({ ...role });
        setEditModalOpen(true);
    };

    const handleDeleteClick = (role) => {
        setSelectedRole(role);
        setDeleteModalOpen(true);
    };

    const handleViewUsersClick = (role) => {
        setSelectedRole(role);
        setUserListModalOpen(true);
    };

    const handleLogsClick = (role) => {
        setSelectedRole(role);
        setLogsModalOpen(true);
    };

    const handleManagePermissionsClick = async (role) => {
        setSelectedRole(role);
        setPermissionsModalOpen(true);
        setLoadingPermissions(true);
        try {
            const [allPerms, rolePerms] = await Promise.all([
                fetchPermissions(),
                getPermissionsForRole(role.id)
            ]);
            setAllPermissions(allPerms);
            setRolePermissions(rolePerms.map(p => p.id));
        } catch (err) {
            showMessage(err.message || 'Failed to load permissions.', 'error');
        } finally {
            setLoadingPermissions(false);
        }
    };

    const handlePermissionChange = async (permissionId, isChecked) => {
        if (!selectedRole) return;

        const originalRolePermissions = [...rolePermissions];

        // Optimistically update UI
        if (isChecked) {
            setRolePermissions(prev => [...prev, permissionId]);
        } else {
            setRolePermissions(prev => prev.filter(id => id !== permissionId));
        }

        try {
            if (isChecked) {
                await assignPermissionToRole(selectedRole.id, permissionId);
                showMessage('Permission assigned successfully.', 'success');
            } else {
                await removePermissionFromRole(selectedRole.id, permissionId);
                showMessage('Permission removed successfully.', 'success');
            }
        } catch (err) {
            // Revert UI on error
            setRolePermissions(originalRolePermissions);
            showMessage(err.message || 'Failed to update permission.', 'error');
        }
    };

    // Handler for form input change in edit modal
    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setSelectedRole(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    };

    // Handler for form input change in add modal
    const handleNewRoleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setNewRole(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    };

    // Handler for submitting the edit form
    const handleUpdate = async (e) => {
        e.preventDefault();
        if (!selectedRole) return;

        setIsUpdating(true);
        try {
            await updateRole(selectedRole);
            setEditModalOpen(false);
            await loadRoles(); // Refresh the list
            showMessage('Role updated successfully!', 'success');
        } catch (err) {
            console.error("Failed to update role:", err);
            showMessage(err.message || 'Failed to update role.', 'error');
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for creating a new role
    const handleCreate = async (e) => {
        e.preventDefault();
        setIsUpdating(true);
        try {
            await createRole(newRole);
            setAddModalOpen(false);
            await loadRoles(); // Refresh the list
            showMessage('Role created successfully!', 'success');
        } catch (err) {
            console.error("Failed to create role:", err);
            showMessage(err.message || 'Failed to create role.', 'error');
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for confirming deletion
    const handleDelete = async () => {
        if (!selectedRole) return;

        setIsUpdating(true);
        try {
            await deleteRole(selectedRole.id);
            setDeleteModalOpen(false);
            await loadRoles(); // Refresh the list
            showMessage('Role deleted successfully!', 'success');
        } catch (err) {
            console.error("Failed to delete role:", err);
            showMessage(err.message || 'Failed to delete role.', 'error');
        } finally {
            setIsUpdating(false);
        }
    };

    if (loading) {
        return <div className="flex justify-center items-center h-32"><div>Loading roles...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
    }

    return (
        <div className="bg-white shadow-lg rounded-xl p-4 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl md:text-2xl font-bold text-gray-800">Manage Roles</h2>
                <button
                    onClick={handleAddClick}
                    className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow"
                >
                    <PlusCircle size={18} />
                    <span>Add Role</span>
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
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Users</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Status</th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-white uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                    {roles.map((role) => (
                        <tr key={role.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{role.name}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 hidden md:table-cell">{role.description}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <button
                                    onClick={() => handleViewUsersClick(role)}
                                    className="flex items-center gap-2 text-blue-600 hover:text-blue-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    disabled={!role.users || role.users.length === 0}
                                >
                                    <Users size={16} />
                                    <span>{role.totalUsers}</span>
                                </button>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                                <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${role.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                                    {role.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {format(parseISO(role.updatedAt), "PPP")}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-4">
                                <button onClick={() => handleManagePermissionsClick(role)} className="text-green-600 hover:text-green-900 transition-colors" title="Manage Permissions">
                                    <ShieldCheck size={18} />
                                </button>
                                <button onClick={() => handleLogsClick(role)} className="text-gray-600 hover:text-gray-900 transition-colors" title="View Logs">
                                    <FileText size={18} />
                                </button>
                                <button onClick={() => handleEditClick(role)} className="text-amber-600 hover:text-amber-900 transition-colors" title="Edit Role">
                                    <Edit size={18} />
                                </button>
                                <button onClick={() => handleDeleteClick(role)} className="text-red-600 hover:text-red-900 transition-colors" title="Delete Role">
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
                <h3 className="text-lg font-bold mb-4">Edit Role</h3>
                {selectedRole && (
                    <form onSubmit={handleUpdate} className="space-y-4">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-gray-700">Name</label>
                            <input
                                type="text"
                                name="name"
                                id="name"
                                value={selectedRole.name}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description</label>
                            <textarea
                                name="description"
                                id="description"
                                value={selectedRole.description}
                                onChange={handleInputChange}
                                rows="3"
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            ></textarea>
                        </div>
                        <div className="flex items-center">
                            <input
                                id="isActive"
                                name="isActive"
                                type="checkbox"
                                checked={selectedRole.isActive}
                                onChange={handleInputChange}
                                className="h-4 w-4 text-amber-600 focus:ring-amber-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                                Active
                            </label>
                        </div>
                        <div className="flex justify-end space-x-3 pt-4">
                            <button type="button" onClick={() => setEditModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                                Cancel
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
                <h3 className="text-lg font-bold mb-4">Add New Role</h3>
                <form onSubmit={handleCreate} className="space-y-4">
                    <div>
                        <label htmlFor="newName" className="block text-sm font-medium text-gray-700">Name</label>
                        <input
                            type="text"
                            name="name"
                            id="newName"
                            value={newRole.name}
                            onChange={handleNewRoleChange}
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        />
                    </div>
                    <div>
                        <label htmlFor="newDescription" className="block text-sm font-medium text-gray-700">Description</label>
                        <textarea
                            name="description"
                            id="newDescription"
                            value={newRole.description}
                            onChange={handleNewRoleChange}
                            rows="3"
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        ></textarea>
                    </div>
                    <div className="flex items-center">
                        <input
                            id="newIsActive"
                            name="isActive"
                            type="checkbox"
                            checked={newRole.isActive}
                            onChange={handleNewRoleChange}
                            className="h-4 w-4 text-amber-600 focus:ring-amber-500 border-gray-300 rounded"
                        />
                        <label htmlFor="newIsActive" className="ml-2 block text-sm text-gray-900">
                            Active
                        </label>
                    </div>
                    <div className="flex justify-end space-x-3 pt-4">
                        <button type="button" onClick={() => setAddModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                            Cancel
                        </button>
                        <button type="submit" disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300">
                            {isUpdating ? 'Adding...' : 'Add Role'}
                        </button>
                    </div>
                </form>
            </Modal>

            {/* Delete Confirmation Modal */}
            <Modal isOpen={isDeleteModalOpen} onClose={() => setDeleteModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Confirm Deletion</h3>
                <p>Are you sure you want to delete the role "{selectedRole?.name}"?</p>
                <div className="flex justify-end space-x-3 mt-6">
                    <button type="button" onClick={() => setDeleteModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">
                        Cancel
                    </button>
                    <button onClick={handleDelete} disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 disabled:bg-gray-300">
                        {isUpdating ? 'Deleting...' : 'Delete'}
                    </button>
                </div>
            </Modal>

            {/* User List Modal */}
            <Modal isOpen={isUserListModalOpen} onClose={() => setUserListModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Users in "{selectedRole?.name}" Role</h3>
                {selectedRole?.users && selectedRole.users.length > 0 ? (
                    <ul className="space-y-3 max-h-60 overflow-y-auto pr-2">
                        {selectedRole.users.map(user => (
                            <li key={user.id} className="bg-gray-100 p-3 rounded-lg shadow-sm">
                                <p className="font-semibold text-gray-800">{user.firstName} {user.lastName}</p>
                                <p className="text-sm text-gray-600">{user.email}</p>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <div className="text-center py-6">
                        <p className="text-gray-500">No users are assigned to this role.</p>
                    </div>
                )}
                <div className="flex justify-end mt-6">
                    <button type="button" onClick={() => setUserListModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors">
                        Close
                    </button>
                </div>
            </Modal>

            {/* Logs Modal */}
            <Modal isOpen={isLogsModalOpen} onClose={() => setLogsModalOpen(false)}>
                <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                    <h3 className="text-lg font-bold mb-4">Audit Logs for "{selectedRole?.name}"</h3>
                    {selectedRole && (
                        <div className="space-y-4">
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedRole.createdAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created By:</p>
                                <p className="text-gray-600">{selectedRole.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Last Updated At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedRole.updatedAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{selectedRole.updatedBy || 'N/A'}</p>
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

            {/* Manage Permissions Modal */}
            <Modal isOpen={isPermissionsModalOpen} onClose={() => setPermissionsModalOpen(false)} size="lg">
                <h3 className="text-lg font-bold mb-4">Manage Permissions for "{selectedRole?.name}"</h3>
                {loadingPermissions ? (
                    <div className="text-center p-8">Loading permissions...</div>
                ) : (
                    <div className="space-y-3 max-h-96 overflow-y-auto pr-3">
                        {allPermissions.map(permission => (
                            <div key={permission.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100">
                                <div>
                                    <p className="font-semibold text-gray-800">{permission.name}</p>
                                    <p className="text-xs text-gray-500">{permission.description}</p>
                                </div>
                                <input
                                    type="checkbox"
                                    className="h-5 w-5 text-amber-600 focus:ring-amber-500 border-gray-300 rounded"
                                    checked={rolePermissions.includes(permission.id)}
                                    onChange={(e) => handlePermissionChange(permission.id, e.target.checked)}
                                />
                            </div>
                        ))}
                    </div>
                )}
                 <div className="flex justify-end mt-6">
                    <button type="button" onClick={() => setPermissionsModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-amber-500 hover:bg-gray-50 transition-colors">
                        Done
                    </button>
                </div>
            </Modal>
        </div>
    );
};

export default Roles;
