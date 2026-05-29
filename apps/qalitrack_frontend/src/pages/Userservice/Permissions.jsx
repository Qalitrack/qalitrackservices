import React, { useState, useEffect } from 'react';

import { fetchPermissions, updatePermission, deletePermission, createPermission, fetchRolesForPermission, fetchDeletedPermissions, restorePermission } from '../../api/helpers/UserService/Permissions/permissions.js';
import { fetchUserById } from '../../api/helpers/UserService/Users/users.js';
import { Edit, Trash2, ShieldAlert, PlusCircle, Users, FileText, RefreshCw, Download } from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import dayjs from 'dayjs';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

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
                        return { ...permission, assignedRolesList: roles.map(r => r.name || 'Unknown').join(', ') || 'None' };
                    } catch {
                        return { ...permission, assignedRolesList: 'N/A' };
                    }
                })
            );

            const settings    = getTicketSettings();
            const companyName = settings.companyName    || 'QALIBRATED SYSTEMS LTD';
            const companyAddr = settings.companyAddress || 'PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996';

            const doc = new jsPDF('landscape', 'mm', 'a4');
            const PW  = doc.internal.pageSize.getWidth();
            const L   = 14, R = PW - 14, TW = R - L;

            const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings);
            const black      = [0,   0,   0];
            const gray       = [107, 114, 128];
            const borderCol  = [229, 231, 235];

            // Circular logo
            let circularLogo = null;
            try {
                const img = await new Promise((resolve, reject) => {
                    const i = new Image(); i.onload = () => resolve(i); i.onerror = reject; i.src = settings.companyLogo || logoSrc;
                });
                const sz = Math.min(img.naturalWidth, img.naturalHeight);
                const cv = document.createElement('canvas'); cv.width = sz; cv.height = sz;
                const ctx = cv.getContext('2d');
                ctx.beginPath(); ctx.arc(sz/2, sz/2, sz/2, 0, Math.PI*2); ctx.clip();
                const srcX = (img.naturalWidth - sz) / 2;
                const srcY = (img.naturalHeight - sz) / 2;
                ctx.drawImage(img, srcX, srcY, sz, sz, 0, 0, sz, sz);
                circularLogo = cv.toDataURL('image/png');
            } catch (_) {}

            // Header
            if (circularLogo) doc.addImage(circularLogo, 'PNG', L, 5, 17, 17);
            doc.setFontSize(14); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
            doc.text(companyName, PW/2, 11, { align: 'center' });
            doc.setFontSize(7.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(companyAddr, PW/2, 16, { align: 'center' });

            // Badge
            const badgeW = 52;
            doc.setFillColor(...accent);
            doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, 'F');
            doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(...accentHeaderText);
            doc.text('PERMISSIONS REPORT', R - badgeW/2, 9.5, { align: 'center' });
            doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, R, 16, { align: 'right' });

            // Amber divider
            doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23);

            // Summary stats
            let y = 27;
            const statW = (TW - 8) / 3;
            const stats = [
                { label: 'TOTAL PERMISSIONS', value: `${permissionsWithRoles.length}` },
                { label: 'WITH ROLES',         value: `${permissionsWithRoles.filter(p => p.assignedRolesList !== 'None' && p.assignedRolesList !== 'N/A').length}` },
                { label: 'REPORT DATE',         value: dayjs().format('DD MMM YYYY') },
            ];
            stats.forEach((s, i) => {
                const bx = L + i * (statW + 4);
                doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
                doc.roundedRect(bx, y, statW, 10, 2, 2, 'FD');
                doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
                doc.text(s.label, bx + statW/2, y + 3.8, { align: 'center' });
                doc.setFontSize(9); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
                doc.text(s.value, bx + statW/2, y + 8.2, { align: 'center' });
            });
            y += 14;

            // Table
            const body = permissionsWithRoles.map((p, idx) => [
                idx + 1,
                p.name || 'N/A',
                p.description || 'N/A',
                p.createdAt ? dayjs(p.createdAt).format('DD MMM YY HH:mm') : 'N/A',
                p.updatedAt ? dayjs(p.updatedAt).format('DD MMM YY HH:mm') : 'N/A',
                p.assignedRolesList || 'None',
            ]);

            autoTable(doc, {
                startY: y,
                margin: { left: L, right: L },
                head: [['#', 'Name', 'Description', 'Created', 'Updated', 'Assigned Roles']],
                body,
                styles: { fontSize: 6.5, cellPadding: 1.5, textColor: black, lineColor: borderCol },
                headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: 'bold', fontSize: 7, halign: 'center', lineColor: accentDark },
                alternateRowStyles: { fillColor: [252, 252, 252] },
                columnStyles: {
                    0: { halign: 'center', cellWidth: 8 },
                    5: { cellWidth: 70 },
                },
            });

            // Footer
            const footerY = doc.lastAutoTable.finalY + 4;
            doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
            doc.roundedRect(L, footerY, TW, 10, 2, 2, 'FD');
            if (circularLogo) doc.addImage(circularLogo, 'PNG', L+2, footerY+1, 8, 8);
            doc.setFontSize(7.5); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
            doc.text('Powered by Qalibrated Systems  |  www.qalibrated.co.ke', PW/2, footerY+5, { align: 'center' });
            doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text('Inventing and Making Happen', PW/2, footerY+8.5, { align: 'center' });

            // Watermark on all pages
            if (circularLogo) {
                try {
                    const wmSize = 90;
                    const PH = doc.internal.pageSize.getHeight();
                    const wmCanvas = document.createElement('canvas');
                    wmCanvas.width = 200; wmCanvas.height = 200;
                    const wmCtx = wmCanvas.getContext('2d');
                    const wmImg = await new Promise((resolve, reject) => {
                        const i = new Image(); i.onload = () => resolve(i); i.onerror = reject;
                        i.src = circularLogo;
                    });
                    wmCtx.globalAlpha = 0.07;
                    wmCtx.drawImage(wmImg, 0, 0, 200, 200);
                    const wmData = wmCanvas.toDataURL('image/png');
                    const totalPages = doc.internal.getNumberOfPages();
                    for (let p = 1; p <= totalPages; p++) {
                        doc.setPage(p);
                        doc.addImage(wmData, 'PNG', PW/2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
                    }
                } catch (_) {}
            }

            doc.save(`permissions-report-${dayjs().format('YYYY-MM-DD')}.pdf`);
            return true;
        } catch (error) {
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
            await loadPermissions(showDeleted);
            setEditModalOpen(false);
            showMessage('Permission updated successfully!', 'success');
        } catch (err) {
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
            setNewPermission({ name: '', description: '' });
            await loadPermissions(showDeleted);
            setAddModalOpen(false);
            showMessage('Permission created successfully!', 'success');
        } catch (err) {
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
            await loadPermissions(showDeleted);
            setDeleteModalOpen(false);
            showMessage(`Permission successfully ${actionVerb}.`, 'success');
        } catch (err) {
            setModalFeedback({ text: err.message || `Failed to ${action} permission.`, type: 'error' });
            setIsUpdating(false);
        }
    };

    if (loading) {
        return <div className="h-full flex items-center justify-center"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div></div>;
    }

    if (error) {
        return <div className="m-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm" role="alert">{error}</div>;
    }

    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <div className="px-4 py-3 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 flex items-center justify-between flex-wrap gap-2">
                <h2 className="text-sm font-bold text-gray-800">Manage Permissions</h2>
                <div className="flex items-center gap-2">
                    <label htmlFor="show-deleted" className="flex items-center gap-1.5 text-xs font-medium text-gray-600 cursor-pointer">
                        <input
                            type="checkbox"
                            id="show-deleted"
                            checked={showDeleted}
                            onChange={handleToggleShowDeleted}
                            className="h-3.5 w-3.5 rounded border-gray-300 text-amber-600 focus:ring-amber-500"
                        />
                        Show Deleted
                    </label>
                    <button
                        onClick={handleDownloadPDF}
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold border border-amber-300 text-amber-700 hover:bg-amber-100 rounded transition-colors"
                    >
                        <Download size={13} />
                        <span>PDF</span>
                    </button>
                    <button
                        onClick={handleAddClick}
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all"
                    >
                        <PlusCircle size={13} />
                        <span>Add Permission</span>
                    </button>
                </div>
            </div>

            {feedbackMessage.text && (
                <div className={`mx-4 mt-2 px-3 py-2 rounded-md text-xs font-medium border ${feedbackMessage.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
                    {feedbackMessage.text}
                </div>
            )}

            <div className="flex-1 overflow-auto">
                <table className="min-w-full">
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
                    <tr>
                        <th scope="col" className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Name</th>
                        <th scope="col" className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider hidden md:table-cell">Description</th>
                        <th scope="col" className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                    {permissions.map((permission) => (
                        <tr key={permission.id} className={`border-b border-gray-100 hover:bg-amber-50 transition-all ${permission.isDeleted ? 'opacity-60 bg-gray-50' : ''}`}>
                            <td className="px-4 py-3 text-sm font-medium text-gray-800">{permission.name}</td>
                            <td className="px-4 py-3 text-sm text-gray-500 hidden md:table-cell">{permission.description}</td>
                            <td className="px-4 py-3 text-sm text-gray-500">
                                {format(parseISO(permission.updatedAt), "PPP")}
                            </td>
                            <td className="px-4 py-3 text-sm font-medium space-x-3">
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
                <h3 className="text-lg font-semibold text-gray-800 mb-4">Edit Permission</h3>
                {modalFeedback.text && (
                    <div className={`px-3 py-2 rounded-md mb-4 text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
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
                <h3 className="text-lg font-semibold text-gray-800 mb-4">Add New Permission</h3>
                {modalFeedback.text && (
                    <div className={`px-3 py-2 rounded-md mb-4 text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
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
                    <h3 className="mt-2 text-lg font-semibold text-gray-800">Delete Permission</h3>
                    <p className="mt-2 text-sm text-gray-600">
                        Are you sure you want to delete the permission "{selectedPermission?.name}"? This action cannot be undone.
                    </p>
                    {modalFeedback.text && (
                        <div className={`mt-4 px-3 py-2 rounded-md text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
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
                <h3 className="text-lg font-semibold text-gray-800 mb-4">Roles with "{selectedPermission?.name}"</h3>
                {rolesForPermission.length > 0 ? (
                    <ul className="space-y-2">
                        {rolesForPermission.map(role => (
                            <li key={role.id} className="bg-amber-50 border border-amber-100 px-3 py-2 rounded-md text-sm font-medium text-gray-700">{role.name}</li>
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
                <div className="bg-amber-50 border border-amber-100 p-5 rounded-lg">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Audit Logs — {selectedPermission?.name}</h3>
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
