import React, { useState, useEffect, useMemo } from 'react';
import TablePagination from '../../components/TablePagination';
import PageHeader from '../../components/PageHeader.jsx';
import {
    fetchRoles,
    updateRole,
    deleteRole,
    createRole,
    getPermissionsForRole,
    assignPermissionToRole,
    removePermissionFromRole,
    fetchDeletedRoles,
    restoreRole
} from '../../api/helpers/UserService/Roles/Roles.js';
import { fetchPermissions } from '../../api/helpers/UserService/Permissions/permissions.js';
import { Edit, Trash2, PlusCircle, Users, ShieldCheck, ShieldAlert, RefreshCw, Download, Lock } from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import dayjs from 'dayjs';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

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
    const [showDeleted, setShowDeleted] = useState(false);
    const [actionLoading, setActionLoading] = useState(null);

    // ── Pagination ──
    const PAGE_SIZE = 10;
    const [page, setPage] = useState(1);

    // State for modals
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [isUserListModalOpen, setUserListModalOpen] = useState(false);
    const [isPermissionsModalOpen, setPermissionsModalOpen] = useState(false);
    const [isConfirmPermissionModalOpen, setConfirmPermissionModalOpen] = useState(false);

    const [selectedRole, setSelectedRole] = useState(null);
    const [newRole, setNewRole] = useState({ name: '', description: '', isActive: true });
    const [isUpdating, setIsUpdating] = useState(false);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [modalFeedback, setModalFeedback] = useState({ text: '', type: '' });

    // State for permission management
    const [allPermissions, setAllPermissions] = useState([]);
    const [rolePermissions, setRolePermissions] = useState([]);
    const [loadingPermissions, setLoadingPermissions] = useState(false);

    // State for permission confirmation
    const [pendingPermissionAction, setPendingPermissionAction] = useState({ id: null, action: null });

    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 5000);
    };

    const loadRoles = async (deleted) => {
        setLoading(true);
        setError(null);
        try {
            const fetchFunction = deleted ? fetchDeletedRoles : fetchRoles;
            const data = await fetchFunction();
            setRoles(data.map(r => ({ ...r, isDeleted: r.isDeleted === true || r.isDeleted === 'True' })));
        } catch (err) {
            setError(err.message || 'Failed to fetch roles.');
        } finally {
            setLoading(false);
        }
    };


    useEffect(() => {
        loadRoles(showDeleted);
    }, [showDeleted]);

    const fetchAllRoles = async () => {
        try {
            const data = await fetchRoles(); // Fetch active roles for PDF
            return data;
        } catch (error) {
            throw error;
        }
    };

    const generatePDF = async () => {
        setLoading(true);
        try {
            const allRoles = await fetchAllRoles();

            const rolesWithPermissions = await Promise.all(
                allRoles.map(async (role) => {
                    try {
                        const permissions = await getPermissionsForRole(role.id);
                        return { ...role, assignedPermissionsList: permissions.map(p => p.name || 'Unknown').join(', ') || 'None' };
                    } catch {
                        return { ...role, assignedPermissionsList: 'N/A' };
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
            const green      = [21,  128, 61];

            // Circular logo
            let circularLogo = null;
            try {
                const img = await new Promise((resolve, reject) => {
                    const i = new Image(); i.onload = () => resolve(i); i.onerror = reject; i.src = settings.companyLogo || logoSrc;
                });
                const sz = 200;
                const pad = sz * 0.06;
                const cv = document.createElement('canvas'); cv.width = sz; cv.height = sz;
                const ctx = cv.getContext('2d');
                ctx.fillStyle = '#ffffff'; ctx.fillRect(0, 0, sz, sz);
                const avail = sz - pad * 2;
                const aspect = img.naturalWidth / img.naturalHeight;
                const drawW = aspect >= 1 ? avail : avail * aspect;
                const drawH = aspect >= 1 ? avail / aspect : avail;
                ctx.drawImage(img, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH);
                circularLogo = cv.toDataURL('image/png');
            } catch (_) {}

            // Header
            if (circularLogo) doc.addImage(circularLogo, 'PNG', L, 5, 17, 17);
            doc.setFontSize(14); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
            doc.text(companyName, PW/2, 11, { align: 'center' });
            doc.setFontSize(7.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(companyAddr, PW/2, 16, { align: 'center' });

            // Badge
            const badgeW = 44;
            doc.setFillColor(...accent);
            doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, 'F');
            doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(...accentHeaderText);
            doc.text('ROLES REPORT', R - badgeW/2, 9.5, { align: 'center' });
            doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, R, 16, { align: 'right' });

            // Amber divider
            doc.setDrawColor(...accent); doc.setLineWidth(0.8); doc.line(L, 23, R, 23);

            // Summary stats
            let y = 27;
            const statW = (TW - 8) / 3;
            const activeCount = rolesWithPermissions.filter(r => r.isActive).length;
            const stats = [
                { label: 'TOTAL ROLES',  value: `${rolesWithPermissions.length}` },
                { label: 'ACTIVE ROLES', value: `${activeCount}` },
                { label: 'REPORT DATE',  value: dayjs().format('DD MMM YYYY') },
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
            const body = rolesWithPermissions.map((role, idx) => [
                idx + 1,
                role.name || 'N/A',
                role.description || 'N/A',
                role.totalUsers || 0,
                role.isActive ? 'ACTIVE' : 'INACTIVE',
                role.createdAt ? dayjs(role.createdAt).format('DD MMM YY HH:mm') : 'N/A',
                role.updatedAt ? dayjs(role.updatedAt).format('DD MMM YY HH:mm') : 'N/A',
                role.assignedPermissionsList || 'None',
            ]);

            autoTable(doc, {
                startY: y,
                margin: { left: L, right: L },
                head: [['#', 'Name', 'Description', 'Users', 'Status', 'Created', 'Updated', 'Permissions']],
                body,
                styles: { fontSize: 6.5, cellPadding: 1.5, textColor: black, lineColor: borderCol },
                headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: 'bold', fontSize: 7, halign: 'center', lineColor: accentDark },
                columnStyles: {
                    0: { halign: 'center', cellWidth: 8 },
                    3: { halign: 'center', cellWidth: 14 },
                    4: { halign: 'center', cellWidth: 18 },
                    7: { cellWidth: 75 },
                },
                didParseCell: (data) => {
                    if (data.column.index === 4 && data.section === 'body') {
                        if (data.cell.raw === 'ACTIVE')   { data.cell.styles.textColor = green;     data.cell.styles.fontStyle = 'bold'; }
                        else                               { data.cell.styles.textColor = amberDark; data.cell.styles.fontStyle = 'bold'; }
                    }
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
            try {
                const wmSize = 90;
                const PH = doc.internal.pageSize.getHeight();
                const wmCanvas = document.createElement('canvas');
                wmCanvas.width = 400; wmCanvas.height = 400;
                const wmCtx = wmCanvas.getContext('2d');
                const wmImg = await new Promise((resolve, reject) => {
                    const i = new Image(); i.onload = () => resolve(i); i.onerror = reject;
                    i.src = settings.companyLogo || logoSrc;
                });
                wmCtx.beginPath(); wmCtx.arc(200, 200, 200, 0, Math.PI * 2); wmCtx.clip();
                wmCtx.globalAlpha = 0.07;
                const wmSz = Math.min(wmImg.naturalWidth, wmImg.naturalHeight);
                const wmSrcX = (wmImg.naturalWidth - wmSz) / 2;
                const wmSrcY = (wmImg.naturalHeight - wmSz) / 2;
                wmCtx.drawImage(wmImg, wmSrcX, wmSrcY, wmSz, wmSz, 0, 0, 400, 400);
                const wmData = wmCanvas.toDataURL('image/png');
                const totalPages = doc.internal.getNumberOfPages();
                for (let p = 1; p <= totalPages; p++) {
                    doc.setPage(p);
                    doc.addImage(wmData, 'PNG', PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
                }
            } catch (_) {}

            doc.save(`roles-report-${dayjs().format('YYYY-MM-DD')}.pdf`);
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

    // Handlers for opening modals
    const handleAddClick = () => {
        setNewRole({ name: '', description: '', isActive: true });
        setModalFeedback({ text: '', type: '' });
        setAddModalOpen(true);
    };

    const handleEditClick = (role) => {
        setSelectedRole({ ...role });
        setModalFeedback({ text: '', type: '' });
        setEditModalOpen(true);
    };

    const handleDeleteClick = (role) => {
        setSelectedRole(role);
        setModalFeedback({ text: '', type: '' });
        setDeleteModalOpen(true);
    };

    const handleToggleShowDeleted = () => {
        setShowDeleted(prev => !prev);
        setPage(1);
    };

    const totalPages = Math.max(1, Math.ceil(roles.length / PAGE_SIZE));
    const paginated = useMemo(
        () => roles.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE),
        [roles, page]
    );

    const handleViewUsersClick = (role) => {
        setSelectedRole(role);
        setUserListModalOpen(true);
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

    const handleConfirmPermissionAction = () => {
        const { id, action } = pendingPermissionAction;
        const isChecked = action === 'add';
        handlePermissionChange(id, isChecked);
        setConfirmPermissionModalOpen(false);
        setPendingPermissionAction({ id: null, action: null });
    };

    const handlePermissionButtonClick = (permissionId, action) => {
        setPendingPermissionAction({ id: permissionId, action });
        setConfirmPermissionModalOpen(true);
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
        setModalFeedback({ text: '', type: '' });
        try {
            await updateRole(selectedRole);
            await loadRoles(showDeleted);
            setEditModalOpen(false);
            showMessage('Role updated successfully!', 'success');
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to update role.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for creating a new role
    const handleCreate = async (e) => {
        e.preventDefault();
        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await createRole(newRole);
            setNewRole({ name: '', description: '', isActive: true });
            await loadRoles(showDeleted);
            setAddModalOpen(false);
            showMessage('Role created successfully!', 'success');
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to create role.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    // Handler for confirming deletion
    const handleToggleDelete = async (role) => {
        const action = role.isDeleted ? 'restore' : 'delete';
        const actionVerb = role.isDeleted ? 'restored' : 'deleted';

        setIsUpdating(true);
        setActionLoading(role.id);
        setModalFeedback({ text: '', type: '' });

        // Optimistic removal from list
        if (!role.isDeleted && !showDeleted) {
            setRoles(prev => prev.filter(r => r.id !== role.id));
        } else if (role.isDeleted && showDeleted) {
            setRoles(prev => prev.filter(r => r.id !== role.id));
        }

        try {
            if (role.isDeleted) {
                await restoreRole(role.id);
            } else {
                await deleteRole(role.id);
            }
            await loadRoles(showDeleted);
            setDeleteModalOpen(false);
            showMessage(`Role successfully ${actionVerb}.`, 'success');
        } catch (err) {
            setModalFeedback({ text: err.message || `Failed to ${action} role.`, type: 'error' });
            loadRoles(showDeleted);
        } finally {
            setIsUpdating(false);
            setActionLoading(null);
        }
    };

    if (loading && roles.length === 0) {
        return <div className="h-full flex items-center justify-center"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div></div>;
    }

    if (error) {
        return <div className="m-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm" role="alert">{error}</div>;
    }

    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <div className="shrink-0 flex items-center justify-end gap-3 px-3 py-2 border-b border-gray-200 bg-gray-50">
                <label htmlFor="show-deleted" className="flex items-center gap-1.5 text-xs font-medium text-gray-600 cursor-pointer">
                    <input
                        type="checkbox"
                        id="show-deleted"
                        checked={showDeleted}
                        onChange={handleToggleShowDeleted}
                        className="h-3.5 w-3.5 rounded border-gray-300"
                    />
                    Show Deleted
                </label>
                <button
                    onClick={handleDownloadPDF}
                    className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded border border-amber-300 text-amber-700 bg-white hover:bg-amber-50 transition-colors"
                >
                    <Download size={13} />
                    <span>PDF</span>
                </button>
                <button
                    onClick={handleAddClick}
                    className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow-sm transition-all bg-amber-500 hover:bg-amber-600 text-white"
                >
                    <PlusCircle size={13} />
                    <span>Add Role</span>
                </button>
            </div>

            {feedbackMessage.text && (
                <div className={`mx-4 mt-2 px-3 py-2 rounded-md text-xs font-medium border ${feedbackMessage.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
                    {feedbackMessage.text}
                </div>
            )}

            <div className="flex-1 overflow-auto bg-white">
                <table className="w-full compact-table">
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                    <tr>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide hidden md:table-cell">Description</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Users</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Last Updated</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {paginated.map((role, index) => (
                        <tr
                            key={role.id}
                            className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                                role.isDeleted ? 'opacity-60 bg-gray-100' : index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
                            }`}
                        >
                            <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">{(page - 1) * PAGE_SIZE + index + 1}</td>
                            <td className="px-3 py-2 text-[10px] font-semibold text-gray-900">
                                <div className="flex items-center gap-1.5">
                                    {role.name}
                                    {role.isSystem && (
                                        <span
                                            className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded-full text-[8px] font-semibold uppercase bg-gray-100 text-gray-600 border border-gray-300"
                                            title="System role — used by backend authorization checks, cannot be edited"
                                        >
                                            <Lock size={8} /> System
                                        </span>
                                    )}
                                </div>
                            </td>
                            <td className="px-3 py-2 text-[10px] text-gray-600 hidden md:table-cell max-w-xs truncate">{role.description}</td>
                            <td className="px-3 py-2 text-[10px] text-gray-600">
                                <button
                                    onClick={() => handleViewUsersClick(role)}
                                    className="flex items-center gap-1 text-amber-600 hover:text-amber-700 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    disabled={!role.users || role.users.length === 0}
                                >
                                    <Users size={12} />
                                    <span>{role.totalUsers}</span>
                                </button>
                            </td>
                            <td className="px-3 py-2">
                                <span className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase ${role.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                    {role.isActive ? 'Active' : 'Inactive'}
                                </span>
                            </td>
                            <td className="px-3 py-2 text-[10px] text-gray-600">
                                {format(parseISO(role.updatedAt), "PPP")}
                            </td>
                            <td className="px-3 py-2">
                                <div className="flex gap-1.5 justify-center">
                                    {!showDeleted && (
                                        <>
                                            <button onClick={() => handleManagePermissionsClick(role)} className="p-1 rounded text-green-600 hover:bg-green-50 border border-green-300 hover:border-green-500 transition-all" title="Manage Permissions">
                                                <ShieldCheck size={12} />
                                            </button>
                                            <button onClick={() => handleViewUsersClick(role)} className="p-1 rounded text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed" title="View Users" disabled={!role.users || role.users.length === 0}>
                                                <Users size={12} />
                                            </button>
                                            <button
                                                onClick={() => handleEditClick(role)}
                                                className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                                title={role.isSystem ? "System role — not editable" : "Edit Role"}
                                                disabled={role.isSystem}
                                            >
                                                <Edit size={12} />
                                            </button>
                                        </>
                                    )}
                                    {!showDeleted && (
                                        <button
                                            onClick={() => handleDeleteClick(role)}
                                            className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                            title={role.isSystem ? "System role — not deletable" : "Delete Role"}
                                            disabled={actionLoading === role.id || role.isSystem}
                                        >
                                            <Trash2 size={12} />
                                        </button>
                                    )}
                                    {showDeleted && (
                                        <button
                                            onClick={() => handleToggleDelete(role)}
                                            className="p-1 rounded text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                            title="Restore Role"
                                            disabled={actionLoading === role.id}
                                        >
                                            <RefreshCw size={12} />
                                        </button>
                                    )}
                                </div>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>

                {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                <TablePagination
                    page={page}
                    totalPages={totalPages}
                    onPageChange={setPage}
                    itemCount={roles.length}
                    itemLabel="roles total"
                />
            </div>

            <style>{`
                .compact-table {
                  font-size: 10px;
                }
                .compact-table thead tr th {
                  padding: 6px 12px;
                  font-weight: 700;
                  font-size: 9px;
                  line-height: 1.2;
                }
                .compact-table tbody tr td {
                  padding: 6px 12px;
                  line-height: 1.3;
                }
            `}</style>

            {/* Edit Modal */}
            <Modal isOpen={isEditModalOpen} onClose={() => setEditModalOpen(false)}>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Edit Role</h3>
                {modalFeedback.text && (
                    <div className={`px-3 py-2 rounded-md mb-4 text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedRole && (
                    <form onSubmit={handleUpdate} className="space-y-4 bg-white p-4 rounded-lg">
                        <div>
                            <label htmlFor="name" className="block text-sm font-medium text-gray-800">Name</label>
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
                            <label htmlFor="description" className="block text-sm font-medium text-gray-800">Description</label>
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
                            <button type="button" onClick={() => setEditModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-white">
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
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Add New Role</h3>
                {modalFeedback.text && (
                    <div className={`px-3 py-2 rounded-md mb-4 text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                <form onSubmit={handleCreate} className="space-y-4 bg-white p-4 rounded-lg">
                    <div>
                        <label htmlFor="newName" className="block text-sm font-medium text-gray-800">Name</label>
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
                        <label htmlFor="newDescription" className="block text-sm font-medium text-gray-800">Description</label>
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
                        <button type="button" onClick={() => setAddModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-white">
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
                <div className="text-center">
                    <ShieldAlert className="mx-auto h-12 w-12 text-red-500" />
                    <h3 className="mt-2 text-lg font-semibold text-gray-900">Delete Role</h3>
                    <p className="mt-2 text-sm text-gray-600">
                        Are you sure you want to delete the role "{selectedRole?.name}"? This action cannot be undone.
                    </p>
                    {modalFeedback.text && (
                        <div className={`mt-4 px-3 py-2 rounded-md text-sm font-medium border ${modalFeedback.type === 'success' ? 'bg-green-50 border-green-200 text-green-700' : 'bg-red-50 border-red-200 text-red-700'}`}>
                            {modalFeedback.text}
                        </div>
                    )}
                </div>
                {!modalFeedback.text || modalFeedback.type !== 'success' ? (
                    <div className="mt-6 flex justify-center space-x-4 bg-white p-4 rounded-lg">
                        <button onClick={() => setDeleteModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-white">
                            Cancel
                        </button>
                        <button onClick={() => handleToggleDelete(selectedRole)} disabled={isUpdating} className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 disabled:bg-gray-400">
                            {isUpdating ? 'Deleting...' : 'Delete'}
                        </button>
                    </div>
                ) : (
                    <div className="mt-6 flex justify-center bg-white p-4 rounded-lg">
                        <button onClick={() => setDeleteModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-white">
                            Close
                        </button>
                    </div>
                )}
            </Modal>

            {/* User List Modal */}
            <Modal isOpen={isUserListModalOpen} onClose={() => setUserListModalOpen(false)}>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Users in "{selectedRole?.name}" Role</h3>
                {selectedRole?.users && selectedRole.users.length > 0 ? (
                    <ul className="space-y-2 max-h-60 overflow-y-auto pr-2">
                        {selectedRole.users.map(user => (
                            <li key={user.id} className="bg-amber-50 border border-amber-100 p-3 rounded-md">
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
                    <button type="button" onClick={() => setUserListModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-white transition-colors">
                        Close
                    </button>
                </div>
            </Modal>

            {/* Manage Permissions Modal */}
            <Modal isOpen={isPermissionsModalOpen} onClose={() => setPermissionsModalOpen(false)} size="lg">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Manage Permissions — {selectedRole?.name}</h3>
                {loadingPermissions ? (
                    <div className="flex justify-center py-8"><div className="animate-spin rounded-full h-6 w-6 border-b-2 border-amber-500"></div></div>
                ) : (
                    <div className="space-y-2 max-h-96 overflow-y-auto pr-2">
                        {allPermissions.map(permission => (
                            <div key={permission.id} className="flex items-center justify-between p-3 bg-gray-50 hover:bg-amber-50 border border-gray-100 rounded-md transition-colors">
                                <div>
                                    <p className="font-semibold text-gray-800">{permission.name}</p>
                                    <p className="text-xs text-gray-500">{permission.description}</p>
                                </div>
                                {rolePermissions.includes(permission.id) ? (
                                    <button
                                        onClick={() => handlePermissionButtonClick(permission.id, 'remove')}
                                        className="px-3 py-1 bg-red-100 text-red-700 text-sm font-medium rounded-md hover:bg-red-200 transition-colors"
                                    >
                                        Remove
                                    </button>
                                ) : (
                                    <button
                                        onClick={() => handlePermissionButtonClick(permission.id, 'add')}
                                        className="px-3 py-1 bg-amber-500 text-amber-700 text-sm font-medium rounded-md hover:bg-amber-200 transition-colors"
                                    >
                                        Add
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>
                )}
                <div className="flex justify-end mt-6">
                    <button type="button" onClick={() => setPermissionsModalOpen(false)} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-white bg-amber-500 hover:bg-amber-600 transition-colors">
                        Done
                    </button>
                </div>
            </Modal>

            {/* Permission Confirmation Modal */}
            <Modal isOpen={isConfirmPermissionModalOpen} onClose={() => setConfirmPermissionModalOpen(false)} size="md">
                <div className="text-center">
                    <ShieldAlert className={`mx-auto h-12 w-12 ${pendingPermissionAction.action === 'add' ? 'text-green-500' : 'text-red-500'}`} />
                    <h3 className="mt-2 text-lg font-semibold text-gray-900">
                        Confirm {pendingPermissionAction.action === 'add' ? 'Add' : 'Remove'} Permission
                    </h3>
                    <p className="mt-2 text-sm text-gray-600">
                        Are you sure you want to {pendingPermissionAction.action} the permission "
                        {allPermissions.find(p => p.id === pendingPermissionAction.id)?.name}" to the role "{selectedRole?.name}"?
                    </p>
                </div>
                <div className="mt-6 flex justify-center space-x-4">
                    <button
                        type="button"
                        onClick={() => setConfirmPermissionModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        onClick={handleConfirmPermissionAction}
                        className={`px-4 py-2 rounded-md text-sm font-medium text-white transition-colors ${
                            pendingPermissionAction.action === 'add'
                                ? 'bg-amber-600 hover:bg-amber-700'
                                : 'bg-red-600 hover:bg-red-700'
                        }`}
                    >
                        {pendingPermissionAction.action === 'add' ? 'Add Permission' : 'Remove Permission'}
                    </button>
                </div>
            </Modal>
        </div>
    );
};

export default Roles;