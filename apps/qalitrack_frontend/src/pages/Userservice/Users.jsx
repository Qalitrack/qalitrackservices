import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchUsers, fetchDeletedUsers, deleteUser, restoreUser, updateUser, fetchUserById, resetPassword, assignRoleToUser, removeRoleFromUser, fetchUserRoles, fetchUserShifts, createUser } from '../../api/helpers/UserService/Users/users.js';
import { fetchRoles } from '../../api/helpers/UserService/Roles/Roles.js';
import { Edit, Trash2, PlusCircle, ChevronLeft, ChevronRight, RefreshCw, Mail, Phone, Save, XCircle, FileText, Key, ShieldAlert, ShieldCheck, Users as UsersIcon, Clock, Download } from 'lucide-react';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ChevronsLeft as ChevronDoubleLeft, ChevronsRight as ChevronDoubleRight } from 'lucide-react';
import { format, parseISO, formatDistanceToNow } from 'date-fns';
import dayjs from 'dayjs';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

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
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);
    const [newUser, setNewUser] = useState({
        firstName: '',
        lastName: '',
        mobileNumber: '',
        email: ''
    });
    const [isUpdating, setIsUpdating] = useState(false);
    const [modalFeedback, setModalFeedback] = useState({ text: '', type: '' });
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [isResetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
    const [userDetails, setUserDetails] = useState({});
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

    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return; // Don't fetch if no ID or already fetched

        try {
            const user = await fetchUserById(userId);
            setUserDetails(prev => ({ ...prev, [userId]: user.email }));
        } catch (error) {
            setUserDetails(prev => ({ ...prev, [userId]: 'Unknown' })); // Handle error case
        }
    };

    const handleAddUserClick = () => {
        setNewUser({
            firstName: '',
            lastName: '',
            mobileNumber: '',
            email: ''
        });
        setModalFeedback({ text: '', type: '' });
        setAddModalOpen(true);
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

    const handleNewUserInputChange = (e) => {
        const { name, value } = e.target;
        setNewUser(prev => ({
            ...prev,
            [name]: value,
            // Add error state for mobile number
            mobileNumberError: name === 'mobileNumber' && value && !validateKenyanPhoneNumber(value)
                ? 'Please enter a valid Kenyan phone number (e.g., 0712345678)'
                : ''
        }));
    };

    const handleCreateUser = async (e) => {
        e.preventDefault();

        // Validation
        if (!newUser.firstName || !newUser.lastName || !newUser.email) {
            setModalFeedback({ text: 'Please fill in all required fields.', type: 'error' });
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(newUser.email)) {
            setModalFeedback({ text: 'Please enter a valid email address.', type: 'error' });
            return;
        }

        // Validate and format mobile number if provided
        if (newUser.mobileNumber) {
            if (!validateKenyanPhoneNumber(newUser.mobileNumber)) {
                setModalFeedback({
                    text: 'Please enter a valid Kenyan phone number (e.g., 0712345678 or 712345678)',
                    type: 'error'
                });
                return;
            }
            // Format the mobile number before sending
            newUser.mobileNumber = formatKenyanPhoneNumber(newUser.mobileNumber);
        }

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });

        try {
            await createUser(newUser);
            setModalFeedback({ text: 'User created successfully!', type: 'success' });
            await loadData(pagination.page, showDeleted);
            setTimeout(() => {
                setAddModalOpen(false);
            }, 2000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to create user.', type: 'error' });
        } finally {
            setIsUpdating(false);
        }
    };

    const handleEditClick = (user) => {
        setSelectedUser({
            ...user,
            mobileNumberError: ''
        });
        setModalFeedback({ text: '', type: '' });
        setEditModalOpen(true);
    };

    const handleLogsClick = (user) => {
        setSelectedUser(user);
        loadUserDetails(user.createdBy);
        loadUserDetails(user.updatedBy);
        setLogsModalOpen(true);
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
            setModalFeedback({ text: 'Password reset email sent successfully!', type: 'success' });
            setTimeout(() => {
                setResetPasswordModalOpen(false);
            }, 3000);
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

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setSelectedUser(prev => ({
            ...prev,
            [name]: value,
            // Add error state for mobile number
            mobileNumberError: name === 'mobileNumber' && value && !validateKenyanPhoneNumber(value)
                ? 'Please enter a valid Kenyan phone number (e.g., 0712345678)'
                : ''
        }));
    };

    const handleUpdate = async (e) => {
        e.preventDefault();
        if (!selectedUser) return;

        // Validation
        if (!selectedUser.firstName || !selectedUser.lastName || !selectedUser.email) {
            setModalFeedback({ text: 'Please fill in all required fields.', type: 'error' });
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(selectedUser.email)) {
            setModalFeedback({ text: 'Please enter a valid email address.', type: 'error' });
            return;
        }

        // Validate and format mobile number if provided
        if (selectedUser.mobileNumber) {
            if (!validateKenyanPhoneNumber(selectedUser.mobileNumber)) {
                setModalFeedback({
                    text: 'Please enter a valid Kenyan phone number (e.g., 0712345678 or 712345678)',
                    type: 'error'
                });
                return;
            }
            // Format the mobile number before sending
            selectedUser.mobileNumber = formatKenyanPhoneNumber(selectedUser.mobileNumber);
        }

        setIsUpdating(true);
        setModalFeedback({ text: '', type: '' });
        try {
            const { id, mobileNumberError, ...userData } = selectedUser;
            await updateUser(id, userData);
            setModalFeedback({ text: 'User updated successfully!', type: 'success' });
            await loadData(pagination.page, showDeleted);
            setTimeout(() => {
                setEditModalOpen(false);
            }, 3000);
        } catch (err) {
            setModalFeedback({ text: err.message || 'Failed to update user.', type: 'error' });
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
                const sz = Math.min(img.naturalWidth, img.naturalHeight);
                const cv = document.createElement('canvas');
                cv.width = sz; cv.height = sz;
                const ctx = cv.getContext('2d');
                ctx.beginPath();
                ctx.arc(sz / 2, sz / 2, sz / 2, 0, Math.PI * 2);
                ctx.clip();
                const srcX = (img.naturalWidth - sz) / 2;
                const srcY = (img.naturalHeight - sz) / 2;
                ctx.drawImage(img, srcX, srcY, sz, sz, 0, 0, sz, sz);
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
                alternateRowStyles: { fillColor: [252, 252, 252] },
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
        <div className="bg-white shadow-lg rounded-xl p-5 md:p-8 max-w-7xl mx-auto my-4 md:my-10">
            <div className="flex flex-col md:flex-row justify-between items-center mb-4 gap-4 md:gap-0">
                <div className="flex items-center gap-4 w-full md:w-auto">
                    <h2 className="text-xl md:text-2xl font-bold text-gray-900">Users</h2>
                    <div className="flex items-center gap-2 ml-4">
                        <label htmlFor="show-deleted" className="text-sm font-medium text-gray-800">Show Deleted</label>
                        <input
                            type="checkbox"
                            id="show-deleted"
                            checked={showDeleted}
                            onChange={handleToggleShowDeleted}
                            className="h-4 w-4 rounded border-gray-300 text-amber-600 focus:ring-amber-500"
                        />
                    </div>
                </div>
                <div className="flex items-center gap-4 w-full md:w-auto justify-end mt-2 md:mt-0">
                    <button
                        onClick={handleDownloadPDF}
                        className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white border border-amber-500 rounded-lg hover:bg-amber-600 transition-colors shadow"
                        title="Download Users as PDF"
                    >
                        <Download size={18} />
                        <span className="hidden md:inline">Download PDF</span>
                    </button>
                    <button
                        onClick={handleAddUserClick}
                        className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white border border-amber-500 rounded-lg hover:bg-amber-600 transition-colors shadow"
                    >
                        <PlusCircle size={18} />
                        <span className="hidden md:inline">Add User</span>
                        <span className="inline md:hidden">Add</span>
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
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
                    <tr>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Email</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Name</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Roles</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Shifts</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Status</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Last Updated</th>
                        <th scope="col" className="px-3 py-2.5 md:px-6 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-100">
                    {users.map((user) => (
                        <tr key={user.id} className={`hover:bg-gray-50 ${user.isDeleted ? 'opacity-60 bg-gray-100' : ''}`}>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-sm font-semibold text-gray-900">{user.email}</td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-sm font-medium text-gray-800">{user.firstName} {user.lastName}</td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-sm text-gray-700">
                                <button
                                    onClick={() => handleViewRolesClick(user)}
                                    className="flex items-center text-amber-500 hover:text-amber-600 transition-colors"
                                    title="View Roles"
                                >
                                    <UsersIcon size={18} className="mr-1" />
                                    <span>{user.roles ? user.roles.length : '0'}</span>
                                </button>
                            </td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-sm text-gray-700">
                                <button
                                    onClick={() => handleViewShiftsClick(user)}
                                    className="flex items-center text-purple-500 hover:text-purple-600 transition-colors"
                                    title="View Shifts"
                                >
                                    <Clock size={18} className="mr-1" />
                                    <span>{userShiftCounts[user.id] || 0}</span>
                                </button>
                            </td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap">
                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusBadge(user)}`}>
                                        {getStatusText(user)}
                                    </span>
                            </td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-sm text-gray-700">
                                {formatDistanceToNow(parseISO(user.updatedAt), { addSuffix: true })}
                            </td>
                            <td className="px-3 py-4 md:px-6 md:py-4 whitespace-nowrap text-left text-sm font-medium flex flex-wrap gap-2">
                                <button
                                    onClick={() => handleLogsClick(user)}
                                    className="text-gray-600 hover:text-gray-900 transition-colors"
                                    title="View Logs"
                                >
                                    <FileText size={18} />
                                </button>
                                <button
                                    onClick={() => handleEditClick(user)}
                                    className="text-amber-600 hover:text-amber-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Edit User"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Edit size={18} />
                                </button>
                                <button
                                    onClick={() => handleManageRolesClick(user)}
                                    className="text-green-600 hover:text-green-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Manage Roles"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <ShieldCheck size={18} />
                                </button>
                                <button
                                    onClick={() => handleResetPasswordClick(user)}
                                    className="text-blue-600 hover:text-blue-900 transition-colors disabled:text-gray-400 disabled:cursor-not-allowed"
                                    title="Reset Password"
                                    disabled={user.isDeleted || actionLoading === user.id}
                                >
                                    <Key size={18} />
                                </button>
                                <button
                                    onClick={() => handleToggleDelete(user)}
                                    className={`${
                                        user.isDeleted ? 'text-blue-600 hover:text-blue-800' : 'text-red-600 hover:text-red-800'
                                    } transition-colors ${actionLoading === user.id ? 'opacity-50 cursor-not-allowed' : ''}`}
                                    title={user.isDeleted ? 'Restore User' : 'Delete User'}
                                    disabled={actionLoading === user.id}
                                >
                                    {user.isDeleted ? <RefreshCw size={18} /> : <Trash2 size={18} />}
                                </button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            <div className="flex flex-col md:flex-row justify-between items-center mt-4 text-sm text-gray-700 gap-4 md:gap-0">
                <div>
                    <p className="text-gray-700">
                        <span className="font-medium">{pagination.page * pagination.pageSize - pagination.pageSize + 1}</span> to <span className="font-medium">{Math.min(pagination.page * pagination.pageSize, pagination.totalCount)}</span> of <span className="font-medium">{pagination.totalCount}</span> rows
                    </p>
                </div>
                <div className="flex items-center gap-1">
                    <button
                        onClick={handlePreviousPage}
                        disabled={!pagination.hasPreviousPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronLeft size={16} />
                    </button>
                    {[...Array(pagination.totalPages).keys()].map((index) => (
                        <button
                            key={index}
                            className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                                pagination.page === index + 1
                                    ? 'bg-amber-500 text-white'
                                    : 'bg-white text-gray-700 hover:bg-gray-100'
                            }`}
                            onClick={() => handlePageClick(index + 1)}
                        >
                            {index + 1}
                        </button>
                    ))}
                    <button
                        onClick={handleNextPage}
                        disabled={!pagination.hasNextPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <ChevronRight size={16} />
                    </button>
                </div>
            </div>
            {/* Add User Modal */}
            <Modal isOpen={isAddModalOpen} onClose={() => setAddModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Add New User</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                <form onSubmit={handleCreateUser} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label htmlFor="firstName" className="block text-sm font-medium text-gray-800">First Name *</label>
                            <input
                                type="text"
                                name="firstName"
                                id="firstName"
                                value={newUser.firstName}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                            />
                        </div>
                        <div>
                            <label htmlFor="lastName" className="block text-sm font-medium text-gray-800">Last Name *</label>
                            <input
                                type="text"
                                name="lastName"
                                id="lastName"
                                value={newUser.lastName}
                                onChange={handleNewUserInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                required
                            />
                        </div>
                    </div>

                    <div>
                        <label htmlFor="email" className="block text-sm font-medium text-gray-800">Email *</label>
                        <input
                            type="email"
                            name="email"
                            id="email"
                            value={newUser.email}
                            onChange={handleNewUserInputChange}
                            className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            required
                        />
                    </div>

                    <div>
                        <label htmlFor="mobileNumber" className="block text-sm font-medium text-gray-800">Mobile Number</label>
                        <input
                            type="tel"
                            name="mobileNumber"
                            id="mobileNumber"
                            value={newUser.mobileNumber || ''}
                            onChange={handleNewUserInputChange}
                            placeholder="e.g., 0712345678 or 712345678"
                            className={`mt-1 block w-full p-2 border ${newUser.mobileNumberError ? 'border-red-500' : 'border-gray-300'} rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500`}
                        />
                        {newUser.mobileNumberError && (
                            <p className="mt-1 text-sm text-red-600">{newUser.mobileNumberError}</p>
                        )}
                    </div>
                    <div className="flex justify-end space-x-3 pt-4">
                        <button
                            type="button"
                            onClick={() => setAddModalOpen(false)}
                            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isUpdating}
                            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300"
                        >
                            {isUpdating ? 'Creating...' : 'Create User'}
                        </button>
                    </div>
                </form>
            </Modal>


            <Modal isOpen={isEditModalOpen} onClose={() => setEditModalOpen(false)}>
                <h3 className="text-lg font-bold mb-4">Edit User</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-center text-sm font-medium ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedUser && (
                    <form onSubmit={handleUpdate} className="space-y-4">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label htmlFor="firstName" className="block text-sm font-medium text-gray-800">First Name</label>
                                <input
                                    type="text"
                                    name="firstName"
                                    id="firstName"
                                    value={selectedUser.firstName}
                                    onChange={handleInputChange}
                                    className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                />
                            </div>
                            <div>
                                <label htmlFor="lastName" className="block text-sm font-medium text-gray-800">Last Name</label>
                                <input
                                    type="text"
                                    name="lastName"
                                    id="lastName"
                                    value={selectedUser.lastName}
                                    onChange={handleInputChange}
                                    className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                                />
                            </div>
                        </div>
                        <div>
                            <label htmlFor="email" className="block text-sm font-medium text-gray-800">Email</label>
                            <input
                                type="email"
                                name="email"
                                id="email"
                                value={selectedUser.email}
                                onChange={handleInputChange}
                                className="mt-1 block w-full p-2 border border-gray-300 rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500"
                            />
                        </div>
                        <div>
                            <label htmlFor="mobileNumber" className="block text-sm font-medium text-gray-800">Mobile Number</label>
                            <input
                                type="tel"
                                name="mobileNumber"
                                id="mobileNumber"
                                value={selectedUser.mobileNumber || ''}
                                onChange={handleInputChange}
                                placeholder="e.g., 0712345678 or 712345678"
                                className={`mt-1 block w-full p-2 border ${selectedUser.mobileNumberError ? 'border-red-500' : 'border-gray-300'} rounded-md shadow-sm focus:ring-amber-500 focus:border-amber-500`}
                            />
                            {selectedUser.mobileNumberError && (
                                <p className="mt-1 text-sm text-red-600">{selectedUser.mobileNumberError}</p>
                            )}
                        </div>
                        <div className="flex justify-end space-x-3 pt-4">
                            <button
                                type="button"
                                onClick={() => setEditModalOpen(false)}
                                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                disabled={isUpdating}
                                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-amber-600 hover:bg-amber-700 disabled:bg-gray-300"
                            >
                                {isUpdating ? 'Updating...' : 'Update User'}
                            </button>
                        </div>
                    </form>
                )}
            </Modal>

            {/* Logs Modal */}
            <Modal isOpen={isLogsModalOpen} onClose={() => setLogsModalOpen(false)}>
                <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                    <h3 className="text-lg font-bold mb-4">Audit Logs for "{selectedUser?.firstName} {selectedUser?.lastName}"</h3>
                    {selectedUser && (
                        <div className="space-y-4">
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedUser.createdAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created By:</p>
                                <p className="text-gray-600">{userDetails[selectedUser.createdBy] || selectedUser.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Last Updated At:</p>
                                <p className="text-gray-600">{format(parseISO(selectedUser.updatedAt), "PPP p")}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{userDetails[selectedUser.updatedBy] || selectedUser.updatedBy || 'N/A'}</p>
                            </div>
                            {selectedUser.isDeleted && (
                                <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                    <p className="font-semibold text-gray-700">Deleted At:</p>
                                    <p className="text-gray-600">{format(parseISO(selectedUser.deletedAt), "PPP p")}</p>
                                </div>
                            )}
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

            {/* Reset Password Modal */}
            <Modal isOpen={isResetPasswordModalOpen} onClose={() => setResetPasswordModalOpen(false)} size="sm">
                <h3 className="text-lg font-bold mb-4">Reset Password</h3>
                {modalFeedback.text && (
                    <div className={`p-3 rounded-lg mb-4 text-sm font-medium text-center ${modalFeedback.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {modalFeedback.text}
                    </div>
                )}
                {selectedUser && (
                    <div className="text-gray-700">
                        <p className="mb-4">Are you sure you want to send a password reset email to **{selectedUser.email}**?</p>
                        <div className="flex justify-end space-x-3">
                            <button
                                type="button"
                                onClick={() => setResetPasswordModalOpen(false)}
                                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                            >
                                Cancel
                            </button>
                            <button
                                type="button"
                                onClick={handleConfirmResetPassword}
                                disabled={isUpdating}
                                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300"
                            >
                                {isUpdating ? 'Sending...' : 'Send'}
                            </button>
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