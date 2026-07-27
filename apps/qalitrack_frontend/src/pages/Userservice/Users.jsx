import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchUsers, fetchDeletedUsers, deleteUser, restoreUser, updateUser, resetPassword, assignRoleToUser, removeRoleFromUser, fetchUserRoles, fetchUserShifts, createUser } from '../../api/helpers/UserService/Users/users.js';
import { fetchRoles } from '../../api/helpers/UserService/Roles/Roles.js';
import { Edit, Trash2, PlusCircle, ChevronLeft, ChevronRight, RefreshCw, Mail, Phone, Save, XCircle, Key, ShieldAlert, ShieldCheck, Users as UsersIcon, Clock, Download } from 'lucide-react';

// Every new/reset account gets this same fixed password until the user
// completes first login and sets their own — mirrors UserService.cs's
// DefaultTemporaryPassword constant. Only accurate for accounts where the
// admin didn't set a custom password at creation.
const DEFAULT_TEMP_PASSWORD = 'ChangeMe123!';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ChevronsLeft as ChevronDoubleLeft, ChevronsRight as ChevronDoubleRight } from 'lucide-react';
import { format, parseISO, formatDistanceToNow } from 'date-fns';
import dayjs from 'dayjs';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';
import TablePagination from '../../components/TablePagination';

const Modal = ({ children, isOpen, onClose, size = "md" }) => {
    if (!isOpen) return null;

    const sizeClasses = {
        sm: "max-w-sm",
        md: "max-w-md",
        lg: "max-w-2xl",
        xl: "max-w-4xl",
        xxl: "max-w-6xl"
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center p-4">
            <div className={`bg-white rounded-lg shadow-xl p-6 w-full ${sizeClasses[size]} max-h-[90vh] overflow-y-auto`}>
                {children}
            </div>
        </div>
    );
};

const Users = () => {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
    });
    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState(null);
    const [error, setError] = useState(null);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [showDeleted, setShowDeleted] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);
    const [editingUser, setEditingUser] = useState(null);
    const [editingUserOriginalRoleId, setEditingUserOriginalRoleId] = useState('');
    const [form, setForm] = useState({
        firstName: '',
        lastName: '',
        mobileNumber: '',
        email: '',
        roleId: ''
    });
    const [isUpdating, setIsUpdating] = useState(false);
    const [modalFeedback, setModalFeedback] = useState({ text: '', type: '' });
    // Shown after create/reset so the admin can relay it out-of-band (phone/SMS)
    // when the welcome/reset email doesn't reach a user in a remote area.
    const [temporaryPassword, setTemporaryPassword] = useState('');
    const [isResetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
    const [isManageRolesModalOpen, setManageRolesModalOpen] = useState(false);
    const [isViewRolesModalOpen, setViewRolesModalOpen] = useState(false);
    const [selectedUserRoles, setSelectedUserRoles] = useState([]);
    const [isViewShiftsModalOpen, setViewShiftsModalOpen] = useState(false);
    const [selectedUserShifts, setSelectedUserShifts] = useState([]);
    const [isConfirmOpen, setIsConfirmOpen] = useState(false);
    const [pendingAction, setPendingAction] = useState({ type: null, role: null });
    const navigate = useNavigate();
    const [userShiftCounts, setUserShiftCounts] = useState({});

    const showMessage = (text, type) => {
        setFeedbackMessage({ text, type });
        setTimeout(() => {
            setFeedbackMessage({ text: '', type: '' });
        }, 3000);
    };

    const loadData = async (page, deleted) => {
        setLoading(true);
        setError(null);
        try {
            const fetchFunction = deleted ? fetchDeletedUsers : fetchUsers;
            const data = await fetchFunction(page, pagination.pageSize);
            const fetchedUsers = data.items || [];
            setUsers(fetchedUsers);
            setPagination({
                page: data.page || 1,
                pageSize: data.pageSize || 10,
                totalCount: data.totalCount || 0,
                totalPages: data.totalPages || 1,
                hasPreviousPage: data.hasPreviousPage || false,
                hasNextPage: data.hasNextPage || false,
            });

            // Load shift counts for the fetched users
            if (fetchedUsers.length > 0) {
                await loadShiftCounts(fetchedUsers);
            }
        } catch (err) {
            setError(err.message || 'Failed to fetch users.');
        } finally {
            setLoading(false);
        }
    };

    const loadRoles = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchRoles();
            setRoles(data || []);
        } catch (err) {
            setError(err.message || 'Failed to fetch roles.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page, showDeleted);
        loadRoles();
    }, [pagination.page, showDeleted]);

    const handlePreviousPage = () => {
        if (pagination.hasPreviousPage) {
            setPagination((p) => ({ ...p, page: p.page - 1 }));
        }
    };

    const handleNextPage = () => {
        if (pagination.hasNextPage) {
            setPagination((p) => ({ ...p, page: p.page + 1 }));
        }
    };

    const handlePageClick = (page) => {
        setPagination((p) => ({ ...p, page }));
    };

    const handleToggleShowDeleted = () => {
        setShowDeleted((prev) => !prev);
        setPagination((p) => ({ ...p, page: 1 }));
    };

    const resetForm = () => {
        setForm({
            firstName: '',
            lastName: '',
            mobileNumber: '',
            email: '',
            roleId: ''
        });
        setEditingUser(null);
        setEditingUserOriginalRoleId('');
        setModalFeedback({ text: '', type: '' });
        setTemporaryPassword('');
    };

    const formatKenyanPhoneNumber = (number) => {
        if (!number) return '';
        // Remove all non-digit characters
        const cleaned = number.replace(/\D/g, '');

        // Check if the number starts with 0 or 254
        if (cleaned.startsWith('0')) {
            return `+254${cleaned.substring(1)}`;
        } else if (cleaned.startsWith('254')) {
            return `+${cleaned}`;
        } else if (cleaned.startsWith('7') || cleaned.startsWith('1')) {
            return `+254${cleaned}`;
        }

        // If it doesn't match any pattern, return as is (will be caught by validation)
        return number;
    };

    const validateKenyanPhoneNumber = (number) => {
        // Check if the number is empty (optional field)
        if (!number) return true;

        // Check if the number matches Kenyan phone number patterns
        // Valid formats: 07XXXXXXXX, 7XXXXXXXX, 2547XXXXXXXX, +2547XXXXXXXX
        const kenyanPhoneRegex = /^(?:\+?254|0)?(7\d{8})$/;
        return kenyanPhoneRegex.test(number);
    };

    const handleFormChange = (e) => {
        const { name, value, type, checked } = e.target;
        setForm(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value,
            // Add error state for mobile number
            mobileNumberError: name === 'mobileNumber' && value && !validateKenyanPhoneNumber(value)
                ? 'Please enter a valid Kenyan phone number (e.g., 0712345678)'
                : ''
        }));
    };

    const handleCreateUser = async () => {
        const { roleId, ...userPayload } = form;
        const result = await createUser(userPayload);
        setTemporaryPassword(result?.temporaryPassword || '');

        if (roleId) {
            const newUserId = result?.user?.id;
            try {
                await assignRoleToUser(newUserId, roleId);
                setModalFeedback({ text: 'User created and role assigned successfully!', type: 'success' });
            } catch (roleErr) {
                // The user account exists either way — a role can still be
                // assigned afterwards from Manage Roles, so don't present
                // this as if user creation itself failed.
                setModalFeedback({ text: `User created, but role assignment failed: ${roleErr.message || 'unknown error'}. Assign it from Manage Roles.`, type: 'error' });
            }
        } else {
            setModalFeedback({ text: 'User created successfully! Remember to assign a role from Manage Roles.', type: 'success' });
        }

        await loadData(pagination.page, showDeleted);
        // Left open (no auto-close) so the admin has time to copy the temp
        // password down below before dismissing.
    };

    const handleEditClick = async (user) => {
        let currentRoleId = '';
        try {
            const userRoles = await fetchUserRoles(user.id);
            currentRoleId = userRoles?.[0]?.id || '';
        } catch (err) {
            currentRoleId = '';
        }
        setForm({
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            email: user.email || '',
            mobileNumber: user.mobileNumber || '',
            roleId: currentRoleId,
            mobileNumberError: ''
        });
        setEditingUserOriginalRoleId(currentRoleId);
        setModalFeedback({ text: '', type: '' });
        setTemporaryPassword('');
        setEditingUser(user);
    };

    const handleViewRolesClick = async (user) => {
        try {
            setSelectedUser(user);
            const userRoles = await fetchUserRoles(user.id);
            setSelectedUser(prev => ({
                ...prev,
                roles: userRoles // Update the user with fetched roles
            }));
            setViewRolesModalOpen(true);
        } catch (error) {
            // Still open the modal but with an error message
            setSelectedUser(prev => ({
                ...prev,
                roles: []
            }));
            setViewRolesModalOpen(true);
        }
    };

    const handleResetPasswordClick = (user) => {
        setSelectedUser(user);
        setModalFeedback({ text: '', type: '' });
        setTemporaryPassword('');
        setResetPasswordModalOpen(true);
    };

    const handleManageRolesClick = async (user) => {
        setSelectedUser(user);
        setModalFeedback({ text: '', type: '' });
        setManageRolesModalOpen(true);
        try {
            const userRolesData = await fetchUserRoles(user.id);
            const roleIds = userRolesData.map(role => role.id);
            setSelectedUserRoles(roleIds);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to fetch user roles.', type: 'error' });
        }
    };

    const handleViewShiftsClick = async (user) => {
        setSelectedUser(user);
        setViewShiftsModalOpen(true);
        try {
            const shifts = await fetchUserShifts(user.id);
            setSelectedUserShifts(shifts);
        } catch (err) {
            setSelectedUserShifts([]);
        }
    };

    const handleConfirmResetPassword = async () => {
        if (!selectedUser) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            await resetPassword(selectedUser.id);
            setModalFeedback({ text: 'Password reset successfully!', type: 'success' });
            // Reset always sets the same fixed default password — no need to
            // parse it back out of the response.
            setTemporaryPassword(DEFAULT_TEMP_PASSWORD);
            // Left open (no auto-close) so the admin has time to copy the temp
            // password below before dismissing.
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to reset password.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    const handleAddRoleClick = (role) => {
        setPendingAction({ type: 'add', role });
        setIsConfirmOpen(true);
    };

    const handleRemoveRoleClick = (role) => {
        setPendingAction({ type: 'remove', role });
        setIsConfirmOpen(true);
    };

    const handleConfirmAction = async () => {
        if (!pendingAction.role || !selectedUser) return;

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });

        try {
            if (pendingAction.type === 'add') {
                await assignRoleToUser(selectedUser.id, pendingAction.role.id);
                showMessage(`Role '${pendingAction.role.name}' added successfully!`, 'success');
            } else if (pendingAction.type === 'remove') {
                await removeRoleFromUser(selectedUser.id, pendingAction.role.id);
                showMessage(`Role '${pendingAction.role.name}' removed successfully!`, 'success');
            }

            // Refetch user roles to update lists
            const userRolesData = await fetchUserRoles(selectedUser.id);
            const roleIds = userRolesData.map(role => role.id);
            setSelectedUserRoles(roleIds);
        } catch (err) {
            setModalFeedback({ text: err.message || `Failed to ${pendingAction.type} role.`, type: 'error' });
        } finally {
            setIsUpdating(false);
            setIsConfirmOpen(false);
            setPendingAction({ type: null, role: null });
        }
    };

    const handleUpdateUser = async () => {
        const { mobileNumberError, roleId, ...userData } = form;
        await updateUser(editingUser.id, userData);

        if (roleId !== editingUserOriginalRoleId) {
            try {
                if (editingUserOriginalRoleId) await removeRoleFromUser(editingUser.id, editingUserOriginalRoleId);
                if (roleId) await assignRoleToUser(editingUser.id, roleId);
            } catch (roleErr) {
                setModalFeedback({ text: `User updated, but role change failed: ${roleErr.message || 'unknown error'}. Change it from Manage Roles.`, type: 'error' });
                await loadData(pagination.page, showDeleted);
                return;
            }
        }

        setModalFeedback({ text: 'User updated successfully!', type: 'success' });
        await loadData(pagination.page, showDeleted);
        setTimeout(resetForm, 1500);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validation
        if (!form.firstName || !form.lastName || !form.email) {
            setModalFeedback({ text: 'Please fill in all required fields.', type: 'error' });
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(form.email)) {
            setModalFeedback({ text: 'Please enter a valid email address.', type: 'error' });
            return;
        }

        // Validate and format mobile number if provided
        if (form.mobileNumber) {
            if (!validateKenyanPhoneNumber(form.mobileNumber)) {
                setModalFeedback({
                    text: 'Please enter a valid Kenyan phone number (e.g., 0712345678 or 712345678)',
                    type: 'error'
                });
                return;
            }
            // Format the mobile number before sending
            form.mobileNumber = formatKenyanPhoneNumber(form.mobileNumber);
        }

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        setTemporaryPassword('');
        try {
            if (editingUser) await handleUpdateUser();
            else await handleCreateUser();
        } catch (err) {
            setModalFeedback({ text: err.message || `Failed to ${editingUser ? 'update' : 'create'} user.`, type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    const handleToggleDelete = async (user) => {
        const action = user.isDeleted ? 'restore' : 'delete';
        const actionVerb = user.isDeleted ? 'restored' : 'deleted';

        setActionLoading(user.id);
        try {
            if (!user.isDeleted && !showDeleted) {
                setUsers((prevUsers) => prevUsers.filter((u) => u.id !== user.id));
            } else if (user.isDeleted && showDeleted) {
                setUsers((prevUsers) => prevUsers.filter((u) => u.id !== user.id));
            }

            if (user.isDeleted) {
                await restoreUser(user.id);
            } else {
                await deleteUser(user.id);
            }
            showMessage(`User successfully ${actionVerb}.`, 'success');
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage(err.message || `Failed to ${action} user.`, 'error');
            loadData(pagination.page, showDeleted);
        } finally {
            setActionLoading(null);
        }
    };

    const fetchAllUsers = async (showDeleted = false) => {
        let allUsers = [];
        let currentPage = 1;
        const pageSize = 50; // Larger page size to reduce number of requests
        let hasMore = true;

        const fetchFunction = showDeleted ? fetchDeletedUsers : fetchUsers;

        try {
            while (hasMore) {
                const response = await fetchFunction(currentPage, pageSize);
                if (response.items && response.items.length > 0) {
                    allUsers = [...allUsers, ...response.items];
                    // Check if there are more pages
                    hasMore = response.hasNextPage === true &&
                        response.items.length === pageSize;
                    currentPage++;
                } else {
                    hasMore = false;
                }
            }
            return allUsers;
        } catch (error) {
            throw error;
        }
    };

    const generatePDF = async () => {
        setLoading(true);
        try {
            const allUsers = await fetchAllUsers(showDeleted);
            const settings    = getTicketSettings();
            const companyName = settings.companyName    || 'QALIBRATED SYSTEMS LTD';
            const companyAddr = settings.companyAddress || 'PO BOX 34463-00100, NAIROBI | TEL: +254 714 999 996';

            const doc  = new jsPDF('landscape', 'mm', 'a4');
            const PW   = doc.internal.pageSize.getWidth();
            const L    = 14;
            const R    = PW - 14;
            const TW   = R - L;

            const { primary: accent, primaryDark: accentDark, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(settings);
            const black      = [0,   0,   0];
            const gray       = [107, 114, 128];
            const borderCol  = [229, 231, 235];
            const green      = [21,  128, 61];
            const red        = [185,  28, 28];

            // Circular logo
            let circularLogo = null;
            try {
                const img = await new Promise((resolve, reject) => {
                    const i = new Image();
                    i.onload = () => resolve(i);
                    i.onerror = reject;
                    i.src = settings.companyLogo || logoSrc;
                });
                const sz  = 200;
                const pad = sz * 0.06;
                const cv = document.createElement('canvas');
                cv.width = sz; cv.height = sz;
                const ctx = cv.getContext('2d');
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(0, 0, sz, sz);
                const avail  = sz - pad * 2;
                const aspect = img.naturalWidth / img.naturalHeight;
                const drawW  = aspect >= 1 ? avail : avail * aspect;
                const drawH  = aspect >= 1 ? avail / aspect : avail;
                ctx.drawImage(img, (sz - drawW) / 2, (sz - drawH) / 2, drawW, drawH);
                circularLogo = cv.toDataURL('image/png');
            } catch (_) {}

            // Header
            if (circularLogo) doc.addImage(circularLogo, 'PNG', L, 5, 17, 17);
            doc.setFontSize(14); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
            doc.text(companyName, PW / 2, 11, { align: 'center' });
            doc.setFontSize(7.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(companyAddr, PW / 2, 16, { align: 'center' });

            // Badge
            const badgeW = 44;
            doc.setFillColor(...accent);
            doc.roundedRect(R - badgeW, 4, badgeW, 9, 2, 2, 'F');
            doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(...accentHeaderText);
            doc.text('USERS REPORT', R - badgeW / 2, 9.5, { align: 'center' });
            doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, R, 16, { align: 'right' });

            // Amber divider
            doc.setDrawColor(...accent); doc.setLineWidth(0.8);
            doc.line(L, 23, R, 23);

            // Summary stats
            let y = 27;
            const statW = (TW - 8) / 3;
            const activeCount = allUsers.filter(u => !u.isDeleted && u.isActive).length;
            const stats = [
                { label: 'TOTAL USERS',  value: `${allUsers.length}` },
                { label: 'ACTIVE USERS', value: `${activeCount}` },
                { label: 'REPORT DATE',  value: dayjs().format('DD MMM YYYY') },
            ];
            stats.forEach((s, i) => {
                const bx = L + i * (statW + 4);
                doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
                doc.roundedRect(bx, y, statW, 10, 2, 2, 'FD');
                doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
                doc.text(s.label, bx + statW / 2, y + 3.8, { align: 'center' });
                doc.setFontSize(9); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
                doc.text(s.value, bx + statW / 2, y + 8.2, { align: 'center' });
            });
            y += 14;

            // Table
            const body = allUsers.map((user, idx) => {
                const status = user.isDeleted ? 'DELETED' : (user.isActive ? 'ACTIVE' : 'INACTIVE');
                return [
                    idx + 1,
                    `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'N/A',
                    user.email || 'N/A',
                    user.mobileNumber || 'N/A',
                    status,
                    user.roles ? user.roles.map(r => r.name || r).join(', ') : 'None',
                    user.updatedAt ? dayjs(user.updatedAt).format('DD MMM YY HH:mm') : 'N/A',
                ];
            });

            autoTable(doc, {
                startY: y,
                margin: { left: L, right: L },
                head: [['#', 'Name', 'Email', 'Mobile', 'Status', 'Roles', 'Last Updated']],
                body,
                styles: { fontSize: 6.5, cellPadding: 1.5, textColor: black, lineColor: borderCol },
                headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: 'bold', fontSize: 7, halign: 'center', lineColor: accentDark },
                columnStyles: { 0: { halign: 'center', cellWidth: 8 }, 4: { halign: 'center' } },
                didParseCell: (data) => {
                    if (data.column.index === 4 && data.section === 'body') {
                        const raw = String(data.cell.raw || '');
                        if (raw === 'ACTIVE')   { data.cell.styles.textColor = green; data.cell.styles.fontStyle = 'bold'; }
                        else if (raw === 'INACTIVE') { data.cell.styles.textColor = [217, 119, 6]; data.cell.styles.fontStyle = 'bold'; }
                        else if (raw === 'DELETED')  { data.cell.styles.textColor = red;  data.cell.styles.fontStyle = 'bold'; }
                    }
                },
            });

            // Footer
            const footerY = doc.lastAutoTable.finalY + 4;
            doc.setFillColor(...accentLight); doc.setDrawColor(...accentDark); doc.setLineWidth(0.3);
            doc.roundedRect(L, footerY, TW, 10, 2, 2, 'FD');
            if (circularLogo) doc.addImage(circularLogo, 'PNG', L + 2, footerY + 1, 8, 8);
            doc.setFontSize(7.5); doc.setFont('helvetica', 'bold'); doc.setTextColor(...black);
            doc.text('Powered by Qalibrated Systems  |  www.qalibrated.co.ke', PW / 2, footerY + 5, { align: 'center' });
            doc.setFontSize(6.5); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text('Inventing and Making Happen', PW / 2, footerY + 8.5, { align: 'center' });

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
                        doc.addImage(wmData, 'PNG', PW / 2 - wmSize / 2, PH / 2 - wmSize / 2, wmSize, wmSize);
                    }
                } catch (_) {}
            }

            doc.save(`users-report-${dayjs().format('YYYY-MM-DD')}.pdf`);
            return true;
        } catch (error) {
            showMessage('Failed to generate PDF: ' + (error.message || 'Unknown error'), 'error');
            return false;
        } finally {
            setLoading(false);
        }

        // Calculate column widths based on content
        const columnStyles = {};
        columns.forEach((col, index) => {
            columnStyles[index] = {
                cellWidth: col.cellWidth === 'auto' ? 'auto' : undefined,
                minCellWidth: col.cellWidth === 'wrap' ? 40 : undefined,
                cellPadding: 3,
                overflow: 'linebreak',
                lineWidth: 0.1
            };
        });

        // Add the table with proper pagination
        autoTable(doc, {
            head: [columns.map(col => col.header)],
            body: data.map(row => columns.map(col => row[col.dataKey])),
            startY: 40,
            styles: {
                fontSize: 8,  // Slightly smaller font to fit more content
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
            margin: {
                top: 40,
                right: 10,
                bottom: 20,
                left: 10
            },
            tableWidth: 'wrap',
            showHead: 'everyPage',
            didDrawPage: function(data) {
                // Add page number to bottom of each page
                const pageSize = doc.internal.pageSize;
                const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                doc.setFontSize(10);
                doc.text(
                    `Page ${data.pageCount} of ${data.pageCount}`,  // This will be updated after all pages are drawn
                    data.settings.margin.left,
                    pageHeight - 10
                );
            },
            willDrawPage: function(data) {
                // Update page number for each page
                const pageSize = doc.internal.pageSize;
                const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                doc.setFontSize(10);
                doc.text(
                    `Page ${data.pageNumber} of ${data.pageCount}`,
                    data.settings.margin.left,
                    pageHeight - 10
                );
            }
        });

        // Save the PDF with a timestamp in the filename
        doc.save(`users-report-${new Date().toISOString().split('T')[0]}.pdf`);
    };

    const handleDownloadPDF = async () => {
        const success = await generatePDF();
        if (success) {
            showMessage('PDF downloaded successfully!', 'success');
        }
    };

    const loadShiftCounts = async (users) => {
        const shiftCounts = {};

        // Fetch shift counts for all users in parallel
        const promises = users.map(async (user) => {
            try {
                const shifts = await fetchUserShifts(user.id);
                shiftCounts[user.id] = shifts.length;
            } catch (error) {
                shiftCounts[user.id] = 0;
            }
        });

        await Promise.all(promises);
        setUserShiftCounts(shiftCounts);
    };

    const getStatusBadge = (user) => {
        if (user.isDeleted) return 'bg-gray-200 text-gray-800';
        return user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
    };

    const getStatusText = (user) => {
        if (user.isDeleted) return 'Deleted';
        return user.isActive ? 'Online' : 'Offline';
    };

    if (loading) {
        return <div className="flex justify-center items-center h-32"><div>Loading users...</div></div>;
    }

    if (error) {
        return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md" role="alert">{error}</div>;
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
                    className="h-7 px-3 flex items-center gap-1.5 text-[11px] font-semibold rounded shadow-sm transition-all border border-amber-300 text-amber-700 bg-white hover:bg-amber-50"
                    title="Download Users as PDF"
                >
                    <Download size={14} />
                    <span className="hidden md:inline">Download PDF</span>
                </button>
            </div>

            {feedbackMessage.text && (
                <div className={`shrink-0 mx-3 mt-2 p-2 rounded text-center text-[11px] font-medium ${feedbackMessage.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {feedbackMessage.text}
                </div>
            )}

            {/* Inline Form */}
            <div className="shrink-0 px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm">
                {modalFeedback.text && (
                    <div className={`mb-2 p-2 rounded text-center text-[11px] font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {temporaryPassword && (
                    <div className="mb-2 p-2 rounded bg-amber-50 border border-amber-200">
                        <p className="text-[10px] text-amber-800 mb-1">
                            Temporary password — the email may not always reach the user (e.g. in remote areas). Relay this to them directly if needed.
                        </p>
                        <div className="flex items-center gap-2">
                            <code className="flex-1 px-2 py-1 bg-white border border-amber-300 rounded text-[11px] font-mono text-amber-900">{temporaryPassword}</code>
                            <button
                                type="button"
                                onClick={() => navigator.clipboard.writeText(temporaryPassword)}
                                className="px-2 py-1 text-[10px] font-semibold border border-amber-300 rounded text-amber-700 hover:bg-amber-100"
                            >
                                Copy
                            </button>
                        </div>
                    </div>
                )}
                <form onSubmit={handleSubmit} className="grid grid-cols-4 gap-2">
                    <div>
                        <label className="text-[10px] font-semibold text-gray-700 mb-1 block">First Name *</label>
                        <input
                            name="firstName"
                            value={form.firstName}
                            onChange={handleFormChange}
                            required
                            placeholder="First name"
                            className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                        />
                    </div>

                    <div>
                        <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Last Name *</label>
                        <input
                            name="lastName"
                            value={form.lastName}
                            onChange={handleFormChange}
                            required
                            placeholder="Last name"
                            className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                        />
                    </div>

                    <div>
                        <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Email *</label>
                        <input
                            type="email"
                            name="email"
                            value={form.email}
                            onChange={handleFormChange}
                            required
                            placeholder="email@example.com"
                            className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                        />
                    </div>

                    <div>
                        <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Mobile Number</label>
                        <input
                            type="tel"
                            name="mobileNumber"
                            value={form.mobileNumber || ''}
                            onChange={handleFormChange}
                            placeholder="e.g., 0712345678"
                            className={`w-full h-7 text-[11px] rounded border px-2 focus:ring-1 ${form.mobileNumberError ? 'border-red-400 focus:border-red-500 focus:ring-red-200' : 'border-amber-300 focus:border-amber-500 focus:ring-amber-200'}`}
                        />
                        {form.mobileNumberError && (
                            <p className="mt-0.5 text-[9px] text-red-600">{form.mobileNumberError}</p>
                        )}
                    </div>

                    <div className="col-span-2">
                        <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Role</label>
                        <select
                            name="roleId"
                            value={form.roleId}
                            onChange={handleFormChange}
                            className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                        >
                            <option value="">-- No role (assign later) --</option>
                            {roles.map((role) => (
                                <option key={role.id} value={role.id}>{role.name}</option>
                            ))}
                        </select>
                    </div>

                    {!editingUser && (
                        <div>
                            <label className="text-[10px] font-semibold text-gray-700 mb-1 block">Password</label>
                            <input
                                type="text"
                                name="password"
                                value={DEFAULT_TEMP_PASSWORD}
                                readOnly
                                disabled
                                title="Every new user gets this same default password until they log in and set their own"
                                className="w-full h-7 text-[11px] rounded border border-amber-300 px-2 bg-amber-50 text-amber-900 font-mono cursor-not-allowed"
                            />
                        </div>
                    )}

                    <div className="col-span-4 flex gap-2 justify-end mt-1">
                        {editingUser && (
                            <button type="button" onClick={resetForm}
                                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1">
                                <XCircle className="w-3 h-3" /> Cancel
                            </button>
                        )}
                        <button type="submit" disabled={isUpdating}
                            className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow-sm transition-all flex items-center gap-1">
                            <PlusCircle className="w-3.5 h-3.5" />
                            {isUpdating ? (editingUser ? 'Updating...' : 'Creating...') : (editingUser ? 'Update User' : 'Add User')}
                        </button>
                    </div>
                </form>
            </div>

            {/* Table Section */}
            <div className="flex-1 overflow-auto bg-white">
                <table className="w-full compact-table">
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                    <tr>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Email</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Name</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Roles</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Shifts</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Last Updated</th>
                        <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {users.map((user, index) => (
                        <tr
                            key={user.id}
                            className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
                                user.isDeleted ? 'opacity-60 bg-gray-100' : index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
                            }`}
                        >
                            <td className="px-3 py-2 text-[10px] font-semibold text-gray-900">{user.email}</td>
                            <td className="px-3 py-2 text-[10px] font-medium text-gray-700">{user.firstName} {user.lastName}</td>
                            <td className="px-3 py-2 text-[10px] text-gray-600">
                                <button
                                    onClick={() => handleViewRolesClick(user)}
                                    className="flex items-center text-amber-600 hover:text-amber-700 transition-colors"
                                    title="View Roles"
                                >
                                    <UsersIcon size={12} className="mr-1" />
                                    <span>{user.roles ? user.roles.length : '0'}</span>
                                </button>
                            </td>
                            <td className="px-3 py-2 text-[10px] text-gray-600">
                                <button
                                    onClick={() => handleViewShiftsClick(user)}
                                    className="flex items-center text-purple-600 hover:text-purple-700 transition-colors"
                                    title="View Shifts"
                                >
                                    <Clock size={12} className="mr-1" />
                                    <span>{userShiftCounts[user.id] || 0}</span>
                                </button>
                            </td>
                            <td className="px-3 py-2">
                                    <span className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase ${getStatusBadge(user)}`}>
                                        {getStatusText(user)}
                                    </span>
                                    {user.isFirstLogin && !user.isDeleted && (
                                        <div className="mt-1 text-[9px] font-mono text-amber-700" title="Hasn't logged in yet — only accurate if no custom password was set at creation">
                                            {DEFAULT_TEMP_PASSWORD}
                                        </div>
                                    )}
                            </td>
                            <td className="px-3 py-2 text-[10px] text-gray-600">
                                {formatDistanceToNow(parseISO(user.updatedAt), { addSuffix: true })}
                            </td>
                            <td className="px-3 py-2">
                                <div className="flex gap-1.5 justify-center">
                                <button
                                    onClick={() => handleEditClick(user)}
                                    className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                    title="Edit User"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Edit size={12} />
                                </button>
                                <button
                                    onClick={() => handleManageRolesClick(user)}
                                    className="p-1 rounded text-green-600 hover:bg-green-50 border border-green-300 hover:border-green-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                    title="Manage Roles"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <ShieldCheck size={12} />
                                </button>
                                <button
                                    onClick={() => handleResetPasswordClick(user)}
                                    className="p-1 rounded text-blue-600 hover:bg-blue-50 border border-blue-300 hover:border-blue-500 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                                    title="Reset Password"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Key size={12} />
                                </button>
                                <button
                                    onClick={() => handleToggleDelete(user)}
                                    className={`p-1 rounded border transition-all disabled:opacity-40 disabled:cursor-not-allowed ${
                                        user.isDeleted ? 'text-blue-600 border-blue-300 hover:bg-blue-50 hover:border-blue-500' : 'text-red-600 border-red-300 hover:bg-red-50 hover:border-red-500'
                                    }`}
                                    title={user.isDeleted ? 'Restore User' : 'Delete User'}
                                    disabled={actionLoading === user.id}
                                >
                                    {user.isDeleted ? <RefreshCw size={12} /> : <Trash2 size={12} />}
                                </button>
                                </div>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>

                {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                <TablePagination
                    page={pagination.page}
                    totalPages={pagination.totalPages}
                    onPageChange={handlePageClick}
                    itemCount={pagination.totalCount}
                    itemLabel="users total"
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

            {/* Reset Password Modal */}
            <Modal isOpen={isResetPasswordModalOpen} onClose={() => setResetPasswordModalOpen(false)} size="sm">
                <h3 className="text-lg font-bold mb-4">Reset Password</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-sm font-medium text-center ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {temporaryPassword && (
                    <div className="p-3 rounded-lg mb-4 bg-amber-50 border border-amber-200">
                        <p className="text-xs text-amber-800 mb-1.5">
                            Temporary password — the reset email may not always reach the user (e.g. in remote areas). Relay this to them directly if needed.
                        </p>
                        <div className="flex items-center gap-2">
                            <code className="flex-1 px-2 py-1 bg-white border border-amber-300 rounded text-sm font-mono text-amber-900">{temporaryPassword}</code>
                            <button
                                type="button"
                                onClick={() => navigator.clipboard.writeText(temporaryPassword)}
                                className="px-3 py-1 text-xs font-semibold border border-amber-300 rounded text-amber-700 hover:bg-amber-100"
                            >
                                Copy
                            </button>
                        </div>
                    </div>
                )}
                {selectedUser && (
                    <div className="text-gray-700">
                        {!temporaryPassword && (
                            <p className="mb-4">Are you sure you want to reset the password for **{selectedUser.email}**?</p>
                        )}
                        <div className="flex justify-end space-x-3">
                            <button
                                type="button"
                                onClick={() => setResetPasswordModalOpen(false)}
                                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                            >
                                {temporaryPassword ? 'Close' : 'Cancel'}
                            </button>
                            {!temporaryPassword && (
                                <button
                                    type="button"
                                    onClick={handleConfirmResetPassword}
                                    disabled={isUpdating}
                                    className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300"
                                >
                                    {isUpdating ? 'Resetting...' : 'Reset'}
                                </button>
                            )}
                        </div>
                    </div>
                )}
            </Modal>

            {/* View Roles Modal */}
            <Modal isOpen={isViewRolesModalOpen} onClose={() => setViewRolesModalOpen(false)} size="sm">
                <h3 className="text-lg font-bold mb-4">Roles for {selectedUser?.firstName} {selectedUser?.lastName}</h3>
                <div className="space-y-2">
                    {selectedUser?.roles?.length > 0 ? (
                        selectedUser.roles.map((role, index) => (
                            <div key={index} className="bg-gray-100 p-2 rounded-md text-gray-700 text-sm">{role.name}</div>
                        ))
                    ) : (
                        <p className="text-gray-500 text-sm">No roles assigned.</p>
                    )}
                </div>
                <div className="flex justify-end mt-4">
                    <button
                        onClick={() => setViewRolesModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
                    >
                        Close
                    </button>
                </div>
            </Modal>

            {/* Manage Roles Modal */}
            <Modal isOpen={isManageRolesModalOpen} onClose={() => setManageRolesModalOpen(false)} size="md">
                <h3 className="text-lg font-bold mb-4">Manage Roles for {selectedUser?.firstName} {selectedUser?.lastName}</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-amber-150 text-black' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedUser && (
                    <>
                        <div className="space-y-4 mb-6">
                            <div>
                                <h4 className="font-semibold text-gray-700 mb-2">Current Roles</h4>
                                {(() => {
                                    const currentRoles = selectedUserRoles
                                        .map(id => roles.find(r => r.id === id))
                                        .filter(Boolean);
                                    return currentRoles.length > 0 ? (
                                        currentRoles.map(role => (
                                            <div key={role.id} className="flex justify-between items-center p-3 bg-gray-50 rounded-md mb-2">
                                                <span className="text-sm font-medium text-gray-700">{role.name}</span>
                                                <button
                                                    onClick={() => handleRemoveRoleClick(role)}
                                                    disabled={isUpdating}
                                                    className="px-3 py-1 text-sm font-medium text-red-600 hover:text-red-800 bg-red-100 hover:bg-red-200 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                                >
                                                    Remove
                                                </button>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="text-gray-500 text-sm italic">No roles assigned.</p>
                                    );
                                })()}
                            </div>
                            <div>
                                <h4 className="font-semibold text-gray-700 mb-2">Available Roles</h4>
                                {(() => {
                                    const availableRoles = roles.filter(r => !selectedUserRoles.includes(r.id));
                                    return availableRoles.length > 0 ? (
                                        availableRoles.map(role => (
                                            <div key={role.id} className="flex justify-between items-center p-3 bg-blue-50 rounded-md mb-2">
                                                <span className="text-sm font-medium text-gray-700">{role.name}</span>
                                                <button
                                                    onClick={() => handleAddRoleClick(role)}
                                                    disabled={isUpdating}
                                                    className="px-3 py-1 text-sm font-medium text-white hover:text-amber-500 bg-amber-500 hover:bg-amber-600 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                                >
                                                    Add
                                                </button>
                                            </div>
                                        ))
                                    ) : (
                                        <p className="text-gray-500 text-sm italic">No available roles.</p>
                                    );
                                })()}
                            </div>
                        </div>
                        <div className="flex justify-end">
                            <button
                                type="button"
                                onClick={() => setManageRolesModalOpen(false)}
                                disabled={isUpdating}
                                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                            >
                                Close
                            </button>
                        </div>
                    </>
                )}
            </Modal>

            {/* Role Action Confirmation Modal */}
            <Modal 
                isOpen={isConfirmOpen} 
                onClose={() => {
                    setIsConfirmOpen(false);
                    setPendingAction({ type: null, role: null });
                }} 
                size="sm"
            >
                <h3 className="text-lg font-bold mb-4">
                    Confirm {pendingAction.type === 'add' ? 'Add' : 'Remove'} Role
                </h3>
                {selectedUser && pendingAction.role && (
                    <div className="text-gray-700">
                        <p className="mb-4 text-sm">
                            Are you sure you want to {pendingAction.type} the role &quot;{pendingAction.role.name}&quot; for <strong>{selectedUser.email}</strong>?
                        </p>
                        <div className="flex justify-end space-x-3">
                            <button
                                type="button"
                                onClick={() => {
                                    setIsConfirmOpen(false);
                                    setPendingAction({ type: null, role: null });
                                }}
                                disabled={isUpdating}
                                className="px-4 py-2 border border-amber-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleConfirmAction}
                                disabled={isUpdating}
                                className={`px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white disabled:bg-gray-300 ${
                                    pendingAction.type === 'add' 
                                        ? 'bg-amber-600 hover:bg-amber-700' 
                                        : 'bg-red-600 hover:bg-red-700'
                                }`}
                            >
                                {isUpdating ? 'Processing...' : (pendingAction.type === 'add' ? 'Add Role' : 'Remove Role')}
                            </button>
                        </div>
                    </div>
                )}
            </Modal>

            {/* View Shifts Modal */}
            <Modal isOpen={isViewShiftsModalOpen} onClose={() => setViewShiftsModalOpen(false)} size="lg">
                <h3 className="text-lg font-bold mb-4">Shifts for {selectedUser?.firstName} {selectedUser?.lastName}</h3>
                <div className="overflow-x-auto">
                    {selectedUserShifts.length > 0 ? (
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                            <tr>
                                <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Shift Date</th>
                                <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Start Time</th>
                                <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">End Time</th>
                                <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Duration</th>
                            </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                            {selectedUserShifts.map(shift => (
                                <tr key={shift.id}>
                                    <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-900">{format(parseISO(shift.startTime), 'PP')}</td>
                                    <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-700">{format(parseISO(shift.startTime), 'p')}</td>
                                    <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-700">{format(parseISO(shift.endTime), 'p')}</td>
                                    <td className="px-4 py-2 whitespace-nowrap text-sm text-gray-700">
                                        {formatDistanceToNow(parseISO(shift.startTime), { addSuffix: false })}
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    ) : (
                        <p className="text-gray-500 text-sm text-center">No shifts found for this user.</p>
                    )}
                </div>
                <div className="flex justify-end mt-4">
                    <button
                        onClick={() => setViewShiftsModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
                    >
                        Close
                    </button>
                </div>
            </Modal>
        </div>
    );
};

export default Users;