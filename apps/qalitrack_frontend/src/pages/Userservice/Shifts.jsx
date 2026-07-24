import React, { useState, useEffect } from 'react';
import {
    fetchShifts,
    fetchDeletedShifts,
    updateShift,
    deleteShift,
    createShift,
} from '../../api/helpers/UserService/Shifts/Shifts.js';
import { fetchUserById } from '../../api/helpers/UserService/Users/users.js';
import {
    Edit,
    Trash2,
    PlusCircle,
    FileText,
    Lock,
    Unlock,
    Download,
} from 'lucide-react';
import TablePagination from '../../components/TablePagination';
import { format, parseISO } from 'date-fns';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import dayjs from 'dayjs';
import logoSrc from '../../assets/logo.jpeg';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';
import ShiftInstances from './Shifts/ShiftInstances';
import AddShift from './Shifts/AddShift';
import ShiftEdit from './Shifts/ShiftEdit';

function isValidDateString(dateString) {
    if (!dateString) return false;
    const d = parseISO(dateString);
    return d instanceof Date && !isNaN(d);
}

function formatTimeOnlyString(timeString) {
    if (!timeString) return '-';
    try {
        const date = new Date(timeString);
        if (isNaN(date.getTime())) return timeString;
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch (e) {
        return timeString;
    }
}

function formatDateTime(dateTimeString) {
    if (!dateTimeString) return '-';
    try {
        const date = new Date(dateTimeString);
        if (isNaN(date.getTime())) return dateTimeString;
        return date.toLocaleString([], {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (e) {
        return dateTimeString;
    }
}

const Modal = ({ children, isOpen, size = 'default' }) => {
    if (!isOpen) return null;

    const widthClass = size === 'large' ? 'max-w-4xl' : 'max-w-lg';

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className={`bg-white rounded-lg shadow-xl p-6 w-full ${widthClass} mx-4`}>
                {children}
            </div>
        </div>
    );
};

// Helper to check if a date string is a valid ISO date
function isValidISODate(dateString) {
    if (!dateString) return false;
    const date = parseISO(dateString);
    return date instanceof Date && !isNaN(date);
}

// Helper to get progress bar color based on fill percentage
const getProgressBarColor = (percentage) => {
    if (percentage >= 1) return '#F59E0B'; // Green when complete
    if (percentage >= 0.75) return '#22C55E'; // Light green when almost complete
    if (percentage >= 0.5) return '#10B981'; // Amber when half complete
    if (percentage >= 0.25) return '#F97316'; // Orange when starting to fill
    return '#EF4444'; // Red when just starting
};

const Shifts = () => {
    const [shifts, setShifts] = useState([]);
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [feedbackMessage, setFeedbackMessage] = useState({ text: '', type: '' });
    const [isViewUsersModalOpen, setViewUsersModalOpen] = useState(false);
    const [selectedShiftUsers, setSelectedShiftUsers] = useState([]);
    const [selectedShift, setSelectedShift] = useState(null);
    const [showDeleted, setShowDeleted] = useState(false);
    const [isEditModalOpen, setEditModalOpen] = useState(false);
    const [selectedShiftToEdit, setSelectedShiftToEdit] = useState(null);
    const [editForm, setEditForm] = useState({
        name: '',
        description: '',
        startTime: '',
        durationHours: 1,
        mode: '0', // '0' for Open, '1' for Closed
        autoRepeatDaily: false,
    });
    const [editFormErrors, setEditFormErrors] = useState({});
    const [isDeleteModalOpen, setDeleteModalOpen] = useState(false);
    const [shiftToDelete, setShiftToDelete] = useState(null);
    const [isAddModalOpen, setAddModalOpen] = useState(false);
    const [isLogsModalOpen, setLogsModalOpen] = useState(false);
    const [logsShift, setLogsShift] = useState(null);
    const [showInstancesModal, setShowInstancesModal] = useState(false);
    const [selectedShiftForInstances, setSelectedShiftForInstances] = useState(null);
    const [editingShift, setEditingShift] = useState(null);
    const [userDetails, setUserDetails] = useState({});
    const [hoveredShiftId, setHoveredShiftId] = useState(null);

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
            const fetchFunction = deleted ? fetchDeletedShifts : fetchShifts;
            const response = await fetchFunction(page, pagination.pageSize);

            // Both fetchShifts and fetchDeletedShifts return { items, page, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage }
            const items = Array.isArray(response.items) ? response.items : [];

            setShifts(items);

            setPagination({
                page: parseInt(response.page) || page,
                pageSize: parseInt(response.pageSize) || pagination.pageSize,
                totalCount: parseInt(response.totalCount) || items.length,
                totalPages: parseInt(response.totalPages) || Math.ceil(items.length / pagination.pageSize),
                hasPreviousPage: Boolean(response.hasPreviousPage),
                hasNextPage: Boolean(response.hasNextPage),
            });

            // Log the data for debugging
        } catch (err) {
            setError(err.response?.data?.message || err.message || 'Failed to fetch shifts.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page, showDeleted);
        // eslint-disable-next-line
    }, [pagination.page, showDeleted]);

    const handlePageClick = (page) => {
        setPagination((p) => ({ ...p, page }));
    };

    const handleViewUsersClick = (shift) => {
        setSelectedShift(shift);
        setSelectedShiftUsers(shift.assignedUsers || []);
        setViewUsersModalOpen(true);
    };

    const handleEditClick = (shift) => {
        setSelectedShiftToEdit(shift);
        setEditForm({
            name: shift.name || '',
            description: shift.description || '',
            startTime: shift.startTime ? shift.startTime.slice(0, 16) : '', // for datetime-local input
            durationHours: shift.durationMinutes ? Math.round(shift.durationMinutes / 60) : 1, // Convert minutes to hours
            mode: String(shift.mode || '0'), // Ensure mode is a string for the select
            autoRepeatDaily: !!shift.autoRepeatDaily,
        });
        setEditFormErrors({});
        setEditModalOpen(true);
    };

    const validateForm = (formData) => {
        const errors = {};
        if (!formData.name?.trim()) errors.name = 'Name is required';
        if (!formData.startTime) errors.startTime = 'Start time is required';
        if (formData.durationHours <= 0 || !Number.isInteger(Number(formData.durationHours))) {
            errors.durationHours = 'Duration must be a positive whole number';
        }
        return errors;
    };

    const handleEditFormChange = (e) => {
        const { name, value, type, checked } = e.target;

        // Handle numeric inputs
        let processedValue = value;
        if (name === 'durationHours') {
            processedValue = value === '' ? '' : Math.max(1, Math.floor(Number(value)));
        }

        const updatedForm = {
            ...editForm,
            [name]: type === 'checkbox' ? checked : processedValue,
        };

        setEditForm(updatedForm);

        // Clear error for the current field when user types
        if (editFormErrors[name]) {
            const errors = { ...editFormErrors };
            delete errors[name];
            setEditFormErrors(errors);
        }
    };

    const handleEditFormSubmit = async (e) => {
        e.preventDefault();
        if (!selectedShiftToEdit) return;

        // Validate form
        const errors = validateForm(editForm);
        if (Object.keys(errors).length > 0) {
            setEditFormErrors(errors);
            return;
        }

        try {
            // Convert startTime to ISO string and hours to minutes
            const payload = {
                ...editForm,
                startTime: editForm.startTime ? new Date(editForm.startTime).toISOString() : '',
                durationMinutes: Math.round(Number(editForm.durationHours) * 60), // Convert hours to minutes
                mode: Number(editForm.mode) // Ensure mode is a number
            };

            await updateShift(selectedShiftToEdit.id, payload);
            showMessage('Shift updated successfully!', 'success');
            setEditModalOpen(false);
            setSelectedShiftToEdit(null);
            setEditFormErrors({});
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage(err?.response?.data?.message || 'Failed to update shift.', 'error');
        }
    };

    const handleEditModalClose = () => {
        setEditModalOpen(false);
        setSelectedShiftToEdit(null);
        setEditFormErrors({});
    };

    const handleDeleteClick = (shift) => {
        setShiftToDelete(shift);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!shiftToDelete) return;
        try {
            await deleteShift(shiftToDelete.id);
            showMessage('Shift deleted successfully!', 'success');
            setDeleteModalOpen(false);
            setShiftToDelete(null);
            loadData(pagination.page, showDeleted);
        } catch (err) {
            showMessage('Failed to delete shift.', 'error');
        }
    };

    const handleCancelDelete = () => {
        setDeleteModalOpen(false);
        setShiftToDelete(null);
    };

    const handleAddClick = () => {
        setAddForm({
            name: '',
            description: '',
            startTime: '',
            durationMinutes: 0,
            mode: 0,
            autoRepeatDaily: false,
        });
        setAddError('');
        setAddSuccess('');
        setAddModalOpen(true);
    };

    const handleAddFormChange = (e) => {
        const { name, value, type, checked } = e.target;

        // Handle numeric inputs
        let processedValue = value;
        if (name === 'durationHours') {
            processedValue = value === '' ? '' : Math.max(1, Math.floor(Number(value)));
        }

        const updatedForm = {
            ...addForm,
            [name]: type === 'checkbox' ? checked : processedValue,
        };

        setAddForm(updatedForm);

        // Clear error for the current field when user types
        if (addFormErrors[name]) {
            const errors = { ...addFormErrors };
            delete errors[name];
            setAddFormErrors(errors);
        }
    };

    const handleAddFormSubmit = async (e) => {
        e.preventDefault();

        // Validate form
        const errors = validateForm(addForm);
        if (Object.keys(errors).length > 0) {
            setAddFormErrors(errors);
            return;
        }

        setAddLoading(true);
        setAddError('');
        setAddSuccess('');

        try {
            const payload = {
                name: addForm.name.trim(),
                description: addForm.description.trim(),
                startTime: addForm.startTime ? new Date(addForm.startTime).toISOString() : '',
                durationMinutes: Math.round(Number(addForm.durationHours) * 60), // Convert hours to minutes
                mode: Number(addForm.mode), // Ensure mode is a number
                autoRepeatDaily: addForm.autoRepeatDaily,
            };

            await createShift(payload);
            setAddSuccess('Shift created successfully!');
            setAddFormErrors({});
            loadData(pagination.page, showDeleted);

            // Reset form but keep it open for adding another shift
            setAddForm({
                name: '',
                description: '',
                startTime: '',
                durationHours: 1,
                mode: '0',
                autoRepeatDaily: false,
            });

        } catch (err) {
            setAddError(err?.response?.data?.message || err.message || 'Failed to create shift.');
        } finally {
            setAddLoading(false);
        }
    };

    const handleAddModalClose = () => {
        setAddModalOpen(false);
        setAddFormErrors({});
        // Reset form when closing
        setAddForm({
            name: '',
            description: '',
            startTime: '',
            durationHours: 1,
            mode: '0',
            autoRepeatDaily: false,
        });
    };

    const handleLogsClick = (shift) => {
        setLogsShift(shift);
        if (shift?.createdBy) loadUserDetails(shift.createdBy);
        if (shift?.updatedBy) loadUserDetails(shift.updatedBy);
        setLogsModalOpen(true);
    };

    const handleLogsModalClose = () => {
        setLogsModalOpen(false);
        setLogsShift(null);
    };

    // Fetch user email by ID and cache it
    const loadUserDetails = async (userId) => {
        if (!userId || userDetails[userId]) return;
        try {
            const user = await fetchUserById(userId);
            setUserDetails(prev => ({ ...prev, [userId]: user.email }));
        } catch (error) {
            setUserDetails(prev => ({ ...prev, [userId]: 'Unknown' }));
        }
    };

    const fetchAllShifts = async (showDeleted = false) => {
        let allShifts = [];
        let currentPage = 1;
        const pageSize = 50; // Larger page size to reduce number of requests
        let hasMore = true;

        const fetchFunction = showDeleted ? fetchDeletedShifts : fetchShifts;

        try {
            while (hasMore) {
                const response = await fetchFunction(currentPage, pageSize);
                if (response.items && response.items.length > 0) {
                    allShifts = [...allShifts, ...response.items];
                    // Check if there are more pages
                    hasMore = response.hasNextPage === true &&
                        response.items.length === pageSize;
                    currentPage++;
                } else {
                    hasMore = false;
                }
            }
            return allShifts;
        } catch (error) {
            throw error;
        }
    };

    const generatePDF = async () => {
        setLoading(true);
        try {
            const allShifts = await fetchAllShifts(showDeleted);
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
            doc.text('SHIFTS REPORT', R - badgeW / 2, 9.5, { align: 'center' });
            doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(...gray);
            doc.text(`Generated: ${dayjs().format('DD MMM YYYY HH:mm')}`, R, 16, { align: 'right' });

            // Amber divider
            doc.setDrawColor(...accent); doc.setLineWidth(0.8);
            doc.line(L, 23, R, 23);

            // Summary stats
            let y = 27;
            const statW = (TW - 8) / 3;
            const activeCount = allShifts.filter(s => s.status === 3).length;
            const stats = [
                { label: 'TOTAL SHIFTS',  value: `${allShifts.length}` },
                { label: 'ACTIVE SHIFTS', value: `${activeCount}` },
                { label: 'REPORT DATE',   value: dayjs().format('DD MMM YYYY') },
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
            const body = allShifts.map((shift, idx) => {
                const type   = shift.recurrenceType === 1 ? 'Daily' : shift.recurrenceType === 2 ? 'Weekly' : shift.recurrenceType === 3 ? 'Monthly' : 'Single';
                const status = shift.status === 3 ? 'ACTIVE' : shift.status === 2 ? 'PUBLISHED' : shift.status === 1 ? 'COMPLETED' : 'DRAFT';
                return [
                    idx + 1,
                    shift.name || 'N/A',
                    shift.description || 'N/A',
                    formatTimeOnlyString(shift.startTime) || 'N/A',
                    formatTimeOnlyString(shift.endTime) || 'N/A',
                    type,
                    status,
                    shift.requiredStaffCount || 0,
                    shift.assignedUsers || 0,
                    shift.createdAt ? dayjs(shift.createdAt).format('DD MMM YY HH:mm') : 'N/A',
                ];
            });

            autoTable(doc, {
                startY: y,
                margin: { left: L, right: L },
                head: [['#', 'Name', 'Description', 'Start', 'End', 'Type', 'Status', 'Req.', 'Assigned', 'Created']],
                body,
                styles: { fontSize: 6.5, cellPadding: 1.5, textColor: black, lineColor: borderCol },
                headStyles: { fillColor: accent, textColor: accentHeaderText, fontStyle: 'bold', fontSize: 7, halign: 'center', lineColor: accentDark },
                columnStyles: {
                    0: { halign: 'center', cellWidth: 8 },
                    6: { halign: 'center' },
                    7: { halign: 'center', cellWidth: 12 },
                    8: { halign: 'center', cellWidth: 16 },
                },
                didParseCell: (data) => {
                    if (data.column.index === 6 && data.section === 'body') {
                        const raw = String(data.cell.raw || '');
                        if (raw === 'ACTIVE')    { data.cell.styles.textColor = green;      data.cell.styles.fontStyle = 'bold'; }
                        else if (raw === 'PUBLISHED') { data.cell.styles.textColor = accentDark; data.cell.styles.fontStyle = 'bold'; }
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

            doc.save(`shifts-report-${dayjs().format('YYYY-MM-DD')}.pdf`);
            return true;
        } catch (error) {
            showMessage('Failed to generate PDF: ' + (error.message || 'Unknown error'), 'error');
            return false;
        } finally {
            setLoading(false);
        }
    };

    const handleDownloadPDF = async () => {
        const success = await generatePDF();
        if (success) {
            showMessage('PDF downloaded successfully!', 'success');
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
            <div className="px-4 py-3 flex items-center justify-between flex-wrap gap-2" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
                <h2 className="text-base font-bold" style={{ color: "var(--cs-appbar-text)" }}>Shifts</h2>
                <div className="flex items-center gap-2">
                    <label htmlFor="show-deleted" className="flex items-center gap-1.5 text-xs font-medium cursor-pointer" style={{ color: "var(--cs-appbar-text)" }}>
                        <input
                            id="show-deleted"
                            type="checkbox"
                            checked={showDeleted}
                            onChange={(e) => {
                                setShowDeleted(e.target.checked);
                                setPagination((p) => ({ ...p, page: 1 }));
                            }}
                            className="h-3.5 w-3.5 rounded border-gray-300"
                        />
                        Show Deleted
                    </label>
                    <button
                        onClick={handleDownloadPDF}
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold cs-solid-chip-btn rounded transition-colors"
                    >
                        <Download size={13} />
                        <span>PDF</span>
                    </button>
                    <button
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow transition-all"
                        style={{ backgroundColor: "#ffffff", color: "var(--cs-appbar-bg)" }}
                        onClick={() => setAddModalOpen(true)}
                    >
                        <PlusCircle size={13} />
                        <span>Add Shift</span>
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
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
                    <tr>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Name</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Time</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Date Range</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Type</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Status</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide w-48">Staff</th>
                        <th scope="col" className="px-3 py-2 text-left text-[9px] font-bold text-amber-900 uppercase tracking-wide">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                    {shifts.map((shift) => {
                        const startDate = new Date(shift.startDate);
                        const endDate = new Date(shift.endDate);
                        const isRecurring = shift.recurrenceType !== 0;

                        return (
                            <tr key={shift.id} className="border-b border-gray-100 hover:bg-amber-50 transition-all">
                                <td
                                    className="px-3 py-2 whitespace-nowrap relative"
                                    onMouseEnter={() => setHoveredShiftId(shift.id)}
                                    onMouseLeave={() => setHoveredShiftId(null)}
                                >
                                    <div className="text-[11px] font-semibold text-gray-900">{shift.name}</div>
                                    {hoveredShiftId === shift.id && shift.description && (
                                        <div className="absolute z-10 w-64 p-2 text-sm leading-tight text-amber-800 bg-amber-100 border border-amber-200 rounded-lg shadow-lg bottom-full left-0 mb-2">
                                            {shift.description}
                                        </div>
                                    )}
                                    <div className="mt-1">
                      <span
                          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                              shift.mode === 0 ? 'bg-blue-100 text-blue-800' : 'bg-purple-100 text-purple-800'
                          }`}
                      >
                        {shift.mode === 0 ? (
                            <>
                                <Unlock className="w-3 h-3 mr-1" />
                                Open
                            </>
                        ) : (
                            <>
                                <Lock className="w-3 h-3 mr-1" />
                                Closed
                            </>
                        )}
                      </span>
                                    </div>
                                </td>

                                <td className="px-3 py-2 whitespace-nowrap">
                                    <div className="text-[10px]">
                                        <div className="font-medium">{formatTimeOnlyString(shift.startTime) || '--:--'}</div>
                                        <div className="font-medium text-gray-700">to {formatTimeOnlyString(shift.endTime) || '--:--'}</div>
                                    </div>
                                </td>

                                <td className="px-3 py-2 whitespace-nowrap">
                                    <div className="text-[10px]">
                                        <div>{startDate.toLocaleDateString()}</div>
                                        {isRecurring && (
                                            <div className="text-[10px] text-gray-600">
                                                to {endDate.toLocaleDateString()}
                                            </div>
                                        )}
                                    </div>
                                </td>

                                <td className="px-3 py-2 whitespace-nowrap">
                                    <div className="flex flex-col space-y-1">
                      <span className="text-[10px] text-gray-900">
                        {shift.type === 1 ? 'Single' : shift.type === 2 ? 'Recurring' : ''}
                      </span>
                                        {isRecurring && (
                                            <span className="font-medium text-gray-700">
                          {shift.recurrenceType === 1 ? 'Daily' :
                              shift.recurrenceType === 2 ? 'Weekly' :
                                  shift.recurrenceType === 3 ? 'Monthly' : 'Custom'}
                                                {shift.recurrenceInterval > 1 ? ` (Every ${shift.recurrenceInterval})` : ''}
                        </span>
                                        )}
                                    </div>
                                </td>

                                <td className="px-3 py-2 whitespace-nowrap">
                                    <div className="flex flex-col space-y-1">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          shift.status === 3
                              ? 'bg-green-100 text-green-800'
                              : shift.status === 2
                                  ? 'bg-blue-100 text-blue-800'
                                  : 'bg-gray-100 text-gray-800'
                      }`}>
                        {shift.status === 3 ? 'Active' : shift.status === 1 ? 'completed' : 'Draft'}
                      </span>
                                        {shift.totalInstances > 0 && (
                                            <span className="text-xs text-gray-600">
                          {shift.totalInstances} instance{shift.totalInstances !== 1 ? 's' : ''}
                        </span>
                                        )}
                                    </div>
                                </td>

                                <td className="pl-4 pr-3 py-2 whitespace-nowrap">
                                    <div className="flex items-center w-full">
                                        <div
                                            className="flex items-center w-32 cursor-pointer group relative"
                                            title={`${shift.assignedUsers || 0} of ${shift.requiredStaffCount || 0} staff assigned${(shift.assignedUsers || 0) > (shift.requiredStaffCount || 0) ? ` (${(shift.assignedUsers || 0) - (shift.requiredStaffCount || 0)} over limit)` : ''}`}
                                        >
                                            <div className="flex items-center w-full">
                                                <div className="w-24 mr-2">
                                                    <div className="bg-gray-200 rounded-full h-2 overflow-hidden w-full">
                                                        <div
                                                            className="h-2 rounded-full transition-all duration-300"
                                                            style={{
                                                                width: `${Math.min(100, ((shift.assignedUsers || 0) / Math.max(shift.requiredStaffCount || 1, shift.assignedUsers || 1)) * 100)}%`,
                                                                backgroundColor: getProgressBarColor((shift.assignedUsers || 0) / (shift.requiredStaffCount || 1))
                                                            }}
                                                        />
                                                    </div>
                                                    <div className="absolute left-0 right-0 text-center text-xs text-gray-500 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                                        {shift.assignedUsers || 0}/{shift.requiredStaffCount || 0}
                                                        {(shift.assignedUsers || 0) > (shift.requiredStaffCount || 0) && (
                                                            <span className="text-red-500"> ({(shift.assignedUsers || 0) - (shift.requiredStaffCount || 0)} over)</span>
                                                        )}
                                                    </div>
                                                    <div className="absolute left-0 right-0 text-center text-xs text-gray-500 group-hover:opacity-0 transition-opacity duration-200">
                                                        {(shift.assignedUsers || 0) > (shift.requiredStaffCount || 0) ? (
                                                            <span className="text-red-500">+{(shift.assignedUsers || 0) - (shift.requiredStaffCount || 0)}</span>
                                                        ) : (
                                                            <>{Math.max(0, (shift.requiredStaffCount || 0) - (shift.assignedUsers || 0))} more</>
                                                        )}
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                                <td className="px-3 py-2 text-[10px] font-medium space-x-2">
                                    <div className="flex items-center space-x-2">
                                        <button
                                            className="text-blue-600 hover:text-blue-800 transition-colors"
                                            title="View Instances"
                                            onClick={() => {
                                                setSelectedShiftForInstances(shift);
                                                setShowInstancesModal(true);
                                            }}
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                                                <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                                            </svg>
                                        </button>
                                        <button
                                            className="text-gray-600 hover:text-gray-900 transition-colors"
                                            title="Logs"
                                            onClick={() => handleLogsClick(shift)}
                                        >
                                            <FileText size={18} />
                                        </button>
                                        {!showDeleted && (
                                            <>
                                                <button
                                                    className="text-amber-600 hover:text-amber-900 transition-colors"
                                                    title="Edit Shift"
                                                    onClick={() => setEditingShift(shift)}
                                                >
                                                    <Edit size={18} />
                                                </button>
                                                <button
                                                    className="text-red-600 hover:text-red-800 transition-colors"
                                                    title="Delete Shift"
                                                    onClick={() => handleDeleteClick(shift)}
                                                >
                                                    <Trash2 size={18} />
                                                </button>
                                            </>
                                        )}
                                    </div>
                                </td>
                            </tr>
                        );
                    })}
                    </tbody>
                </table>

                {/* Mobile card view */}
                <div className="space-y-4 md:hidden">
                    {shifts.map((shift) => (
                        <div
                            key={shift.id}
                            className="border rounded-lg p-4 shadow-sm bg-white space-y-2"
                        >
                            <div className="flex justify-between items-center">
                                <h3 className="font-semibold text-gray-900">{shift.name}</h3>
                                <span
                                    className={`px-2 py-1 text-xs rounded-full ${
                                        shift.status === 3
                                            ? 'bg-green-100 text-green-800'
                                            : shift.status === 2
                                                ? 'bg-blue-100 text-blue-800'
                                                : 'bg-gray-100 text-gray-800'
                                    }`}
                                >
                  {shift.status === 3 ? 'Active' : shift.status === 2 ? 'Published' : 'Draft'}
                </span>
                            </div>
                            <p className="text-sm text-gray-600">{shift.description}</p>
                            <div className="text-sm text-gray-500">
                                <p>
                                    <span className="font-medium text-gray-700">Start:</span>{' '}
                                    {isValidDateString(shift.startTime)
                                        ? format(parseISO(shift.startTime), 'PPP p')
                                        : formatTimeOnlyString(shift.startTime)}
                                </p>
                                <p>
                                    <span className="font-medium text-gray-700">End:</span>{' '}
                                    {isValidDateString(shift.endTime)
                                        ? format(parseISO(shift.endTime), 'PPP p')
                                        : formatTimeOnlyString(shift.endTime)}
                                </p>
                                <p>
                                    <span className="font-medium text-gray-700">Mode:</span>{' '}
                                    {shift.mode}
                                </p>
                            </div>
                            <div className="flex justify-between items-center pt-2">
                                <button
                                    onClick={() => handleViewUsersClick(shift)}
                                    className="flex items-center text-amber-500 hover:text-amber-600 transition-colors text-sm"
                                >
                                    <FileText size={16} className="mr-1" />
                                    <span>{shift.assignedUsersCount || 0} Users</span>
                                </button>
                                <div className="flex space-x-3">
                                    {!showDeleted && (
                                        <>
                                            <button
                                                className="text-amber-600 hover:text-amber-900 transition-colors"
                                                onClick={() => handleEditClick(shift)}
                                            >
                                                <Edit size={16} />
                                            </button>
                                            <button
                                                className="text-red-600 hover:text-red-800 transition-colors"
                                                onClick={() => handleDeleteClick(shift)}
                                            >
                                                <Trash2 size={16} />
                                            </button>
                                        </>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
                <TablePagination
                    page={pagination.page}
                    totalPages={pagination.totalPages}
                    onPageChange={handlePageClick}
                    itemCount={pagination.totalCount}
                    itemLabel="shifts total"
                />
            </div>

            <Modal isOpen={isViewUsersModalOpen}>
                <h3 className="text-lg font-semibold text-gray-800 mb-4">Users — {selectedShift?.name}</h3>
                {selectedShiftUsers.length > 0 ? (
                    <ul className="space-y-2">
                        {selectedShiftUsers.map(user => (
                            <li key={user.id} className="bg-amber-50 border border-amber-100 px-3 py-2 rounded-md text-sm font-medium text-gray-700">
                                {user.firstName} {user.lastName} ({user.email})
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="text-sm text-gray-500">No users assigned to this shift.</p>
                )}
                <div className="flex justify-end pt-4">
                    <button
                        type="button"
                        onClick={() => setViewUsersModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                    >
                        Close
                    </button>
                </div>
            </Modal>

            <Modal isOpen={isDeleteModalOpen}>
                <h3 className="text-lg font-bold mb-4 text-red-700">Confirm Delete</h3>
                <p className="mb-4">Are you sure you want to delete the shift <span className="font-semibold">{shiftToDelete?.name}</span>? This action cannot be undone.</p>
                <div className="flex justify-end gap-2">
                    <button onClick={handleCancelDelete} className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50">Cancel</button>
                    <button onClick={handleConfirmDelete} className="px-4 py-2 bg-red-600 text-white rounded-md text-sm font-medium hover:bg-red-700">Delete</button>
                </div>
            </Modal>

            {/* Add Shift Modal - Uses its own modal implementation */}
            <AddShift
                isOpen={isAddModalOpen}
                onClose={() => setAddModalOpen(false)}
                onShiftAdded={(newShift) => {
                    // Ensure the new shift has all required properties with defaults
                    const formattedShift = {
                        ...newShift,
                        startDate: newShift.startDate || new Date().toISOString(),
                        endDate: newShift.endDate || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
                        recurrenceType: newShift.recurrenceType || 0,
                        status: newShift.status || 1, // Default to active status
                        assignedUsers: newShift.assignedUsers || 0,
                        requiredStaffCount: newShift.requiredStaffCount || 1
                    };

                    // Add the new shift to the beginning of the list
                    setShifts(prevShifts => [formattedShift, ...prevShifts]);

                    // Update the total count
                    setPagination(prev => ({
                        ...prev,
                        totalCount: prev.totalCount + 1
                    }));

                    // Show success message
                    setFeedbackMessage({
                        text: 'Shift created successfully!',
                        type: 'success'
                    });
                }}
            />

            {editingShift && (
                <ShiftEdit
                    isOpen={!!editingShift}
                    onClose={() => setEditingShift(null)}
                    shift={editingShift}
                    onSave={() => {
                        loadData(pagination.page, showDeleted);
                        setEditingShift(null);
                    }}
                />
            )}

            {/* Shift Instances Modal */}
            <Modal isOpen={showInstancesModal} onClose={() => setShowInstancesModal(false)} size="large">
                <div className="w-full max-w-8xl max-h-[90vh] overflow-y-auto">
                    <div className="bg-white p-2 rounded-lg shadow-md w-full">
                        <div className="flex justify-between items-center mb-1">
                            <h3 className="text-lg font-bold">
                                {selectedShiftForInstances?.name} - Shift Instances
                            </h3>
                            <button
                                onClick={() => setShowInstancesModal(false)}
                                className="text-gray-400 hover:text-gray-500"
                            >
                                <span className="sr-only">Close</span>
                                <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>
                        <div className="w-full">
                            <ShiftInstances shiftId={selectedShiftForInstances?.id} />
                        </div>
                    </div>
                </div>
            </Modal>

            {/* Logs Modal */}
            <Modal isOpen={isLogsModalOpen}>
                <div className="bg-gray-100 p-6 rounded-lg shadow-md">
                    <h3 className="text-lg font-bold mb-4">Audit Logs for "{logsShift?.name}"</h3>
                    {logsShift && (
                        <div className="space-y-4">
                            {/* Deleted badge if applicable */}
                            {logsShift.isDeleted && (
                                <div className="flex items-center mb-2">
                                    <span className="bg-red-100 text-red-700 px-3 py-1 rounded-full text-xs font-semibold">Deleted Shift</span>
                                </div>
                            )}
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created At:</p>
                                <p className="text-gray-600">{isValidISODate(logsShift.createdAt) ? format(parseISO(logsShift.createdAt), "PPP p") : '-'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Created By:</p>
                                <p className="text-gray-600">{userDetails[logsShift.createdBy] || logsShift.createdBy || 'N/A'}</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated At:</p>
                                <p className="text-gray-600">{
                                    isValidISODate(logsShift.updatedAt) ? format(parseISO(logsShift.updatedAt), "PPP p") : '-'
                                }</p>
                            </div>
                            <div className="grid grid-cols-[140px_1fr] gap-x-6 items-start py-2 border-b">
                                <p className="font-semibold text-gray-700">Updated By:</p>
                                <p className="text-gray-600">{userDetails[logsShift.updatedBy] || logsShift.updatedBy || 'N/A'}</p>
                            </div>
                        </div>
                    )}
                    <div className="flex justify-end mt-6">
                        <button
                            type="button"
                            onClick={handleLogsModalClose}
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

export default Shifts;