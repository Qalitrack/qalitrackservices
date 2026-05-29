import React, { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import {
    fetchShifts,
    fetchShiftUsers,
    assignShiftToUser,
    removeShiftFromUser,
    removeShiftFromRole,
    assignShiftToRole,
    fetchDeletedUserShifts,
    fetchShiftById,
} from "../../api/helpers/UserService/Shifts/shiftAssignment.js";
import { ChevronLeft, ChevronRight, Users, Tag, Download } from "lucide-react";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";
import { format, parseISO } from "date-fns";
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { getTicketSettings, resolveReportColors } from '../../utils/ticketThemeConfig';

function isValidDateString(dateString) {
    if (!dateString) return false;
    const d = parseISO(dateString);
    return d instanceof Date && !isNaN(d);
}

function formatTimeOnlyString(timeString) {
    if (!timeString) return "-";
    const match = timeString.match(/^(\d{2}):(\d{2})(:(\d{2}))?(\.(\d+))?$/);
    if (!match) return timeString;
    const [, hours, minutes, , seconds] = match;
    return `${hours}:${minutes}${seconds ? ":" + seconds : ""}`;
}

const Modal = ({ children, isOpen }) => {
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-lg m-4 relative max-h-[90vh] overflow-y-auto">
                {children}
            </div>
        </div>
    );
};

const ConfirmationModal = ({ 
    isOpen, 
    onClose, 
    onConfirm, 
    title, 
    message,
    confirmText = "Confirm",
    cancelText = "Cancel"
}) => {
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md m-4">
                <h3 className="text-lg font-bold mb-4">{title}</h3>
                <p className="text-gray-600 mb-6">{message}</p>
                <div className="flex justify-end space-x-3">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm bg-gray-50 hover:bg-gray-100"
                    >
                        {cancelText}
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 bg-red-500 text-white rounded-md text-sm hover:bg-red-600"
                    >
                        {confirmText}
                    </button>
                </div>
            </div>
        </div>
    );
};

const ShiftAssignment = () => {
    const { state } = useLocation();
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
    const [showDeleted, setShowDeleted] = useState(false);

    // Modals
    const [isViewUsersModalOpen, setViewUsersModalOpen] = useState(false);
    const [isViewRolesModalOpen, setViewRolesModalOpen] = useState(false);

    // Confirmation modals
    const [isConfirmUserActionOpen, setConfirmUserActionOpen] = useState(false);
    const [isConfirmRoleActionOpen, setConfirmRoleActionOpen] = useState(false);
    const [userActionType, setUserActionType] = useState(''); // 'add' or 'remove'
    const [selectedUserForAction, setSelectedUserForAction] = useState(null);
    const [selectedRoleForAction, setSelectedRoleForAction] = useState(null);
    const [roleActionType, setRoleActionType] = useState(''); // 'add' or 'remove'

    // State for selected shift
    const [selectedShift, setSelectedShift] = useState(null);
    const [allUsers, setAllUsers] = useState([]);
    const [allRoles, setAllRoles] = useState([]);

    // Modal loading & errors
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);

    // Checkbox states (visual only for users)
    const [checkboxStateUsers, setCheckboxStateUsers] = useState({});
    const [checkboxStateRoles, setCheckboxStateRoles] = useState({});

    // Messages
    const [usersModalMessage, setUsersModalMessage] = useState({
        text: "",
        type: "",
    });
    const [rolesModalMessage, setRolesModalMessage] = useState({
        text: "",
        type: "",
    });

    // Auto clear messages after few seconds
    useEffect(() => {
        if (usersModalMessage.text) {
            const timer = setTimeout(
                () => setUsersModalMessage({ text: "", type: "" }),
                4000
            );
            return () => clearTimeout(timer);
        }
    }, [usersModalMessage]);

    useEffect(() => {
        if (rolesModalMessage.text) {
            const timer = setTimeout(
                () => setRolesModalMessage({ text: "", type: "" }),
                4000
            );
            return () => clearTimeout(timer);
        }
    }, [rolesModalMessage]);

    // Load shifts
    const loadData = async (page) => {
        setLoading(true);
        setError(null);
        try {
            let data;
            if (showDeleted) {
                data = await fetchDeletedUserShifts(page, pagination.pageSize);
                // Group by shiftId
                const grouped = data.items.reduce((acc, item) => {
                    const shiftKey = item.shiftId;
                    if (!acc[shiftKey]) {
                        acc[shiftKey] = {
                            id: shiftKey,
                            shiftName: item.shiftName || 'Unknown Shift',
                            userAssignments: [],
                            removedCount: 0,
                        };
                    }
                    acc[shiftKey].userAssignments.push({
                        userId: item.userId,
                        assignedAt: item.assignedAt,
                        updatedAt: item.updatedAt,
                        updatedBy: item.updatedBy,
                        deletedAt: item.deletedAt || item.assignedAt,
                    });
                    acc[shiftKey].removedCount++;
                    return acc;
                }, {});
                let processedShifts = Object.values(grouped).map(group => ({
                    ...group,
                    name: `${group.shiftName} (${group.removedCount} removed users)`,
                    startTime: null,
                    endTime: null,
                    mode: 0,
                    assignedUsersCount: group.removedCount,
                    assignedRolesCount: 0,
                    createdAt: group.userAssignments[0]?.deletedAt, // Use first one's deletedAt as representative
                    userIds: group.userAssignments.map(ua => ua.userId), // For compatibility if needed
                }));
                // Fetch shift details if possible
                try {
                    const uniqueShiftIds = processedShifts.map(s => s.id);
                    const shiftPromises = uniqueShiftIds.map(async (id) => {
                        try {
                            const shift = await fetchShiftById(id);
                            return { id, shift };
                        } catch (err) {
                            return { id, shift: null };
                        }
                    });
                    const shiftResults = await Promise.all(shiftPromises);
                    const shiftMap = shiftResults.reduce((acc, { id, shift }) => {
                        if (shift) acc[id] = shift;
                        return acc;
                    }, {});
                    processedShifts = processedShifts.map(s => ({
                        ...s,
                        name: shiftMap[s.id]?.name ? `${shiftMap[s.id].name} (${s.removedCount} removed users)` : s.name,
                        startTime: shiftMap[s.id]?.startTime || s.startTime,
                        endTime: shiftMap[s.id]?.endTime || s.endTime,
                        mode: shiftMap[s.id]?.mode || s.mode,
                    }));
                } catch (err) {
                }
                setShifts(processedShifts);
            } else {
                data = await fetchShifts(page, pagination.pageSize);
                setShifts(data.items || []);
            }
            setPagination({
                page: data.page || 1,
                pageSize: data.pageSize || 10,
                totalCount: data.totalCount || 0,
                totalPages: data.totalPages || 1,
                hasPreviousPage: data.hasPreviousPage || false,
                hasNextPage: data.hasNextPage || false,
            });
        } catch (err) {
            setError(err.message || "Failed to fetch shifts.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(pagination.page);
    }, [pagination.page, showDeleted]);

    // Pagination handlers
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

    // Handle Users Modal
    const handleViewUsersClick = async (shift) => {
        setSelectedShift(shift);
        setModalLoading(true);
        setModalError(null);
        try {
            if (showDeleted) {
                const removedAssignments = shift.userAssignments || [];
                const usersData = await fetchUsers(1, 1000);
                const users = usersData.items || [];
                const usersMap = users.reduce((acc, u) => {
                    acc[u.id] = u;
                    return acc;
                }, {});
                // Collect all relevant user IDs for fetching (removed users + updatedBy)
                const allRelevantUserIds = [...new Set([
                    ...removedAssignments.map(ua => ua.userId),
                    ...removedAssignments.map(ua => ua.updatedBy).filter(Boolean)
                ])];
                // If more users needed, but assuming 1000 is sufficient; otherwise, adjust fetchUsers call
                const removedUsersWithDetails = removedAssignments
                    .map(ua => {
                        const user = usersMap[ua.userId];
                        if (!user) return null;
                        const updatedByUser = ua.updatedBy ? usersMap[ua.updatedBy] : null;
                        return {
                            ...user,
                            assignment: ua,
                            updatedByUser: updatedByUser ? `${updatedByUser.firstName || ''} ${updatedByUser.lastName || ''}`.trim() || 'Unknown' : 'Unknown'
                        };
                    })
                    .filter(Boolean);
                setAllUsers(removedUsersWithDetails);
                setCheckboxStateUsers(
                    removedUsersWithDetails.reduce((acc, user) => {
                        acc[user.id] = true;
                        return acc;
                    }, {})
                );
            } else {
                const usersData = await fetchUsers(1, 1000);
                const users = usersData.items || [];
                const shiftUsers = await fetchShiftUsers(shift.id);
                const normalizedShiftUsers = Array.isArray(shiftUsers)
                    ? shiftUsers
                    : shiftUsers.users || [];
                const assignedUserIds = new Set(normalizedShiftUsers.map((u) => u.id));

                setAllUsers(users);
                setCheckboxStateUsers(
                    users.reduce((acc, user) => {
                        acc[user.id] = assignedUserIds.has(user.id);
                        return acc;
                    }, {})
                );
            }
            setViewUsersModalOpen(true);
        } catch (err) {
            setModalError(err.message || "Failed to fetch users.");
        } finally {
            setModalLoading(false);
        }
    };

    const handleUserActionClick = (userId, action) => {
        setSelectedUserForAction(userId);
        setUserActionType(action);
        setConfirmUserActionOpen(true);
    };

    const confirmUserAction = async () => {
        setConfirmUserActionOpen(false);
        setModalLoading(true);
        setModalError(null);
        try {
            const user = allUsers.find(u => u.id === selectedUserForAction);
            if (!user) throw new Error('User not found');

            if (userActionType === 'add') {
                await assignShiftToUser(selectedUserForAction, selectedShift.id);
                setCheckboxStateUsers((prev) => ({ ...prev, [selectedUserForAction]: true }));
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 0) + 1 }
                            : shift
                    )
                );
                setUsersModalMessage({ text: "User assigned to shift.", type: "success" });
            } else if (userActionType === 'remove') {
                await removeShiftFromUser(selectedUserForAction, selectedShift.id);
                setCheckboxStateUsers((prev) => ({ ...prev, [selectedUserForAction]: false }));
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedUsersCount: (shift.assignedUsersCount || 1) - 1 }
                            : shift
                    )
                );
                setUsersModalMessage({ text: "User removed from shift.", type: "success" });
            }
        } catch (err) {
            setUsersModalMessage({
                text: `Failed: ${err.message}`,
                type: "error",
            });
        } finally {
            setModalLoading(false);
        }
    };

    // Handle Roles Modal
    const handleViewRolesClick = async (shift) => {
        if (showDeleted) return;
        setSelectedShift(shift);
        setModalLoading(true);
        setModalError(null);
        try {
            const rolesData = await fetchRoles();
            const roles = rolesData.items || rolesData;
            const assignedRoles = shift.roles || [];
            const assignedIds = new Set(assignedRoles.map((r) => r.id));

            setAllRoles(roles);
            setCheckboxStateRoles(
                roles.reduce((acc, role) => {
                    acc[role.id] = assignedIds.has(role.id);
                    return acc;
                }, {})
            );

            setViewRolesModalOpen(true);
        } catch (err) {
            setModalError(err.message || "Failed to fetch roles.");
        } finally {
            setModalLoading(false);
        }
    };

    const handleRoleActionClick = (roleId, action) => {
        setSelectedRoleForAction(roleId);
        setRoleActionType(action);
        setConfirmRoleActionOpen(true);
    };

    const confirmRoleAction = async () => {
        setConfirmRoleActionOpen(false);
        setModalLoading(true);
        setModalError(null);
        try {
            const role = allRoles.find(r => r.id === selectedRoleForAction);
            if (!role) throw new Error('Role not found');

            if (roleActionType === 'add') {
                await assignShiftToRole(selectedRoleForAction, selectedShift.id);
                setCheckboxStateRoles((prev) => ({ ...prev, [selectedRoleForAction]: true }));
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedRolesCount: (shift.assignedRolesCount ) }
                            : shift
                    )
                );
                setRolesModalMessage({ text: "Role assigned to shift.", type: "success" });
            } else if (roleActionType === 'remove') {
                await removeShiftFromRole(selectedRoleForAction, selectedShift.id);
                setCheckboxStateRoles((prev) => ({ ...prev, [selectedRoleForAction]: false }));
                setShifts((prev) =>
                    prev.map((shift) =>
                        shift.id === selectedShift.id
                            ? { ...shift, assignedRolesCount: (shift.assignedRolesCount) }
                            : shift
                    )
                );
                setRolesModalMessage({ text: "Role removed from shift.", type: "success" });
            }
        } catch (err) {
            setRolesModalMessage({
                text: `Failed: ${err.message}`,
                type: "error",
            });
        } finally {
            setModalLoading(false);
        }
    };

    const fetchAllShifts = async () => {
        let allShifts = [];
        let currentPage = 1;
        const pageSize = 50; // Larger page size to reduce number of requests
        let hasMore = true;

        try {
            while (hasMore) {
                const response = await fetchShifts(currentPage, pageSize);
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
        if (showDeleted) {
            setError('PDF generation not available for deleted view.');
            setLoading(false);
            return false;
        }
        setLoading(true);
        try {
            // Fetch all shifts with pagination
            const allShifts = await fetchAllShifts();

            // Fetch assigned users for each shift
            const shiftsWithUsers = await Promise.all(
                allShifts.map(async (shift) => {
                    try {
                        const shiftUsers = await fetchShiftUsers(shift.id);
                        const normalizedShiftUsers = Array.isArray(shiftUsers)
                            ? shiftUsers
                            : shiftUsers?.users || [];
                        
                        // Format users as "name: email" with line breaks
                        const formattedUsers = normalizedShiftUsers.map(user => {
                            const name = `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Unknown';
                            const email = user.email || 'no-email';
                            return `${name}: ${email}`;
                        });

                        return {
                            ...shift,
                            assignedUsersList: formattedUsers,
                            assignedUsersCount: formattedUsers.length
                        };
                    } catch (err) {
                        return {
                            ...shift,
                            assignedUsersList: ['Error fetching users'],
                            assignedUsersCount: 0
                        };
                    }
                })
            );

            const { primary: accent, primaryLight: accentLight, headerText: accentHeaderText } = resolveReportColors(getTicketSettings());
            const doc = new jsPDF({
                orientation: 'landscape'  // Use landscape for better table display
            });

            // Add title and metadata
            doc.setFontSize(18);
            doc.text('Shift Assignments Report', 14, 22);
            doc.setFontSize(11);
            doc.setTextColor(100);
            doc.text(`Generated on: ${new Date().toLocaleString()}`, 14, 30);
            doc.text(`Total Shifts: ${shiftsWithUsers.length}`, 14, 38);

            // Define the columns with increased widths for better readability
            const columns = [
                { header: 'Name', dataKey: 'name', cellWidth: 'auto' },
                { header: 'Start', dataKey: 'startTime', cellWidth: 40 },    // Increased width
                { header: 'End', dataKey: 'endTime', cellWidth: 40 },        // Increased width
                { header: 'Mode', dataKey: 'mode', cellWidth: 30 },          // Increased width
                { header: 'Assigned Users', dataKey: 'assignedUsersList', cellWidth: 'wrap' },  // Full header
                { header: 'Created At', dataKey: 'createdAt', cellWidth: 45 }   // Increased width and full header
            ];

            // Prepare the data for the table
            const data = shiftsWithUsers.map(shift => {
                // Format assigned users with line breaks
                const assignedUsers = Array.isArray(shift.assignedUsersList) && shift.assignedUsersList.length > 0
                    ? { content: shift.assignedUsersList.join('\n') }
                    : 'None';

                // Convert mode to a readable format
                let modeText = 'N/A';
                if (shift.mode !== undefined && shift.mode !== null) {
                    modeText = shift.mode === 0 ? 'Open' : 'Closed';
                }

                return {
                    name: shift.name || 'N/A',
                    startTime: isValidDateString(shift.startTime) ? format(parseISO(shift.startTime), 'PPpp') : formatTimeOnlyString(shift.startTime) || 'N/A',
                    endTime: isValidDateString(shift.endTime) ? format(parseISO(shift.endTime), 'PPpp') : formatTimeOnlyString(shift.endTime) || 'N/A',
                    mode: modeText,
                    assignedUsersList: assignedUsers,
                    assignedRoles: shift.assignedRolesCount || 0,
                    createdAt: format(parseISO(shift.createdAt), 'PPpp')
                };
            });

            // Set up column styles with explicit widths
            const columnStyles = {};
            columns.forEach((col, index) => {
                const style = {
                    cellPadding: 3,
                    overflow: 'linebreak',
                    lineWidth: 0.1,
                    minCellWidth: 20, // Minimum width for all columns
                    cellWidth: 'wrap' // Default to wrap
                };

                // Apply specific widths where defined
                if (col.cellWidth && col.cellWidth !== 'auto' && col.cellWidth !== 'wrap') {
                    style.cellWidth = col.cellWidth;
                }

                // Special handling for assigned users
                if (col.dataKey === 'assignedUsersList') {
                    style.cellWidth = 100; // Wider to accommodate the table
                    style.valign = 'top';
                    style.styles = { 
                        fontSize: 9, // Slightly larger font
                        lineHeight: 1.4,
                        font: 'monospace' // Monospace for better alignment
                    };
                }

                columnStyles[index] = style;
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
                    fillColor: accent,
                    textColor: accentHeaderText,
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
                    // This is where we can add content after the table is drawn
                },
                willDrawPage: function(data) {
                    // Add page number to bottom of each page
                    const pageSize = doc.internal.pageSize;
                    const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                    const pageNumber = data.pageNumber || 1;
                    const pageCount = data.pageCount || 1;

                    // Only add page numbers if we have valid values
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

            // Save the PDF with a timestamp in the filename
            doc.save(`shift-assignments-report-${new Date().toISOString().split('T')[0]}.pdf`);

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
            // Optional: Show success message if you have a feedback system
        }
    };

    // Loading / Error states
    if (loading) {
        return <div className="h-full flex items-center justify-center"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div></div>;
    }
    if (error) {
        return <div className="m-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm" role="alert">{error}</div>;
    }

    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <div className="px-4 py-3 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200 flex items-center justify-between flex-wrap gap-2">
                <h2 className="text-base font-bold text-gray-900">Shift Assignment</h2>
                <div className="flex items-center gap-2">
                    <label className="flex items-center gap-1.5 text-xs font-medium text-gray-700 cursor-pointer">
                        <input
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
                        disabled={showDeleted}
                        className={`flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded transition-colors ${showDeleted ? 'bg-gray-200 text-gray-400 cursor-not-allowed' : 'border border-amber-300 text-amber-700 hover:bg-amber-100'}`}
                    >
                        <Download size={13} />
                        <span>PDF</span>
                    </button>
                </div>
            </div>

            <div className="flex-1 overflow-auto">
                <table className="min-w-full">
                    <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
                    <tr>
                        <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Name</th>
                        {!showDeleted && (
                            <>
                                <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Start Time</th>
                                <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">End Time</th>
                            </>
                        )}
                        <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Mode</th>
                        <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Users</th>
                        <th className="px-4 py-2.5 text-left text-xs font-semibold text-amber-900 uppercase tracking-wider">Roles</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                    {shifts.map((shift) => (
                        <tr key={shift.id} className="border-b border-gray-100 hover:bg-amber-50 transition-all">
                            <td className="px-4 py-3 text-sm font-semibold text-gray-900">{shift.name}</td>
                            {!showDeleted && (
                                <>
                                    <td className="px-4 py-3 text-sm text-gray-700">
                                        {isValidDateString(shift.startTime)
                                            ? format(parseISO(shift.startTime), "PPP p")
                                            : formatTimeOnlyString(shift.startTime) || '-'}
                                    </td>
                                    <td className="px-4 py-3 text-sm text-gray-700">
                                        {isValidDateString(shift.endTime)
                                            ? format(parseISO(shift.endTime), "PPP p")
                                            : formatTimeOnlyString(shift.endTime) || '-'}
                                    </td>
                                </>
                            )}
                            <td className="px-4 py-3 text-sm text-gray-700">
                                {shift.mode === 0 ? 'Open' : shift.mode === 1 ? 'Closed' : shift.mode || 'N/A'}
                            </td>
                            <td className="px-4 py-3 text-sm">
                                <button
                                    onClick={() => handleViewUsersClick(shift)}
                                    className={`flex items-center gap-1 hover:underline ${(shift.assignedUsersCount || 0) > 0 ? "text-green-600 font-semibold" : "text-amber-500"}`}
                                >
                                    <Users size={15} />
                                    {shift.assignedUsersCount || 0}
                                </button>
                            </td>
                            <td className="px-4 py-3 text-sm">
                                <button
                                    onClick={() => handleViewRolesClick(shift)}
                                    disabled={showDeleted}
                                    className={`flex items-center gap-1 text-amber-600 ${showDeleted ? 'opacity-50 cursor-not-allowed' : 'hover:underline'}`}
                                >
                                    <Tag size={15} />
                                    {shift.assignedRolesCount}
                                </button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            <div className="px-4 py-2.5 border-t border-amber-100 bg-white flex justify-between items-center text-xs text-gray-700">
                <p>
          <span className="font-medium">
            {pagination.page * pagination.pageSize - pagination.pageSize + 1}
          </span>{" "}
                    to{" "}
                    <span className="font-medium">
            {Math.min(
                pagination.page * pagination.pageSize,
                pagination.totalCount
            )}
          </span>{" "}
                    of <span className="font-medium">{pagination.totalCount}</span> rows
                </p>
                <div className="flex items-center gap-1">
                    <button
                        onClick={handlePreviousPage}
                        disabled={!pagination.hasPreviousPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    >
                        <ChevronLeft size={16} />
                    </button>
                    {[...Array(pagination.totalPages).keys()].map((index) => (
                        <button
                            key={index}
                            className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                                pagination.page === index + 1
                                    ? "bg-amber-500 text-white"
                                    : "bg-white text-gray-700 hover:bg-gray-100"
                            }`}
                            onClick={() => handlePageClick(index + 1)}
                        >
                            {index + 1}
                        </button>
                    ))}
                    <button
                        onClick={handleNextPage}
                        disabled={!pagination.hasNextPage || loading}
                        className="p-2 border rounded-md text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    >
                        <ChevronRight size={16} />
                    </button>
                </div>
            </div>

            {/* Users Modal */}
            <Modal isOpen={isViewUsersModalOpen}>
                <h3 className="text-lg font-bold mb-4">
                    {showDeleted ? 'Removed Users for' : 'Manage Users for'} "{selectedShift?.name}"
                </h3>
                {usersModalMessage.text && (
                    <div
                        className={`p-3 mb-4 rounded-md text-sm font-medium ${
                            usersModalMessage.type === "success"
                                ? "bg-green-100 text-green-700"
                                : "bg-red-100 text-red-700"
                        }`}
                    >
                        {usersModalMessage.text}
                    </div>
                )}
                {modalLoading ? (
                    <div className="flex justify-center items-center h-32">
                        <div>Loading users...</div>
                    </div>
                ) : modalError ? (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md">
                        {modalError}
                    </div>
                ) : (
                    <>
                        {allUsers.length > 0 ? (
                            <ul className="space-y-2 max-h-96 overflow-y-auto">
                                {allUsers.map((user) => (
                                    <li
                                        key={user.id}
                                        className="flex flex-col bg-gray-100 p-3 rounded-md text-sm"
                                    >
                                        <div className="flex items-center justify-between mb-2">
                                            <div className="flex items-center">
                                                <input
                                                    type="checkbox"
                                                    checked={checkboxStateUsers[user.id] || false}
                                                    readOnly
                                                    className="mr-2 h-4 w-4 text-white focus:ring-amber-500 border-gray-300 rounded"
                                                />
                                                <span className="font-medium">{user.firstName} {user.lastName} ({user.email})</span>
                                            </div>
                                            <div className="space-x-2">
                                                {!showDeleted && (
                                                    <>
                                                        {!checkboxStateUsers[user.id] && (
                                                            <button
                                                                onClick={() => handleUserActionClick(user.id, 'add')}
                                                                className="px-3 py-1 bg-amber-500 text-white text-xs rounded hover:bg-amber-600"
                                                                disabled={modalLoading}
                                                            >
                                                                Add
                                                            </button>
                                                        )}
                                                        {checkboxStateUsers[user.id] && (
                                                            <button
                                                                onClick={() => handleUserActionClick(user.id, 'remove')}
                                                                className="px-3 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600"
                                                                disabled={modalLoading}
                                                            >
                                                                Remove
                                                            </button>
                                                        )}
                                                    </>
                                                )}
                                            </div>
                                        </div>
                                        {showDeleted && user.assignment && (
                                            <div className="text-xs text-gray-600 space-y-1 pl-6">
                                                <div>
                                                    <span className="font-semibold">Assigned At:</span>{' '}
                                                    {format(parseISO(user.assignment.assignedAt), 'PPP p')}
                                                </div>
                                                <div>
                                                    <span className="font-semibold">Updated At:</span>{' '}
                                                    {format(parseISO(user.assignment.updatedAt), 'PPP p')}
                                                </div>
                                                <div>
                                                    <span className="font-semibold">Updated By:</span>{' '}
                                                    {user.updatedByUser}
                                                </div>
                                            </div>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-sm text-gray-500">
                                No users available. Please check database.
                            </p>
                        )}
                    </>
                )}
                <div className="flex justify-end pt-4 sticky bottom-0 bg-white">
                    <button
                        type="button"
                        onClick={() => setViewUsersModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm bg-gray-50 hover:bg-gray-100"
                    >
                        Close
                    </button>
                </div>
            </Modal>

            {/* User Confirmation Modal */}
            <ConfirmationModal
                isOpen={isConfirmUserActionOpen}
                onClose={() => setConfirmUserActionOpen(false)}
                onConfirm={confirmUserAction}
                title={userActionType === 'add' ? 'Confirm Add User' : 'Confirm Remove User'}
                message={`Are you sure you want to ${userActionType} this user from the shift "${selectedShift?.name}"?`}
                confirmText={userActionType === 'add' ? 'Add User' : 'Remove User'}
            />

            {/* Roles Modal */}
            <Modal isOpen={isViewRolesModalOpen}>
                <h3 className="text-lg font-bold mb-4">
                    Manage Roles for "{selectedShift?.name}"
                </h3>
                {rolesModalMessage.text && (
                    <div
                        className={`p-3 mb-4 rounded-md text-sm font-medium ${
                            rolesModalMessage.type === "success"
                                ? "bg-green-100 text-green-700"
                                : "bg-red-100 text-red-700"
                        }`}
                    >
                        {rolesModalMessage.text}
                    </div>
                )}
                {modalLoading ? (
                    <div className="flex justify-center items-center h-32">
                        <div>Loading roles...</div>
                    </div>
                ) : modalError ? (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-md">
                        {modalError}
                    </div>
                ) : (
                    <>
                        {allRoles.length > 0 ? (
                            <ul className="space-y-2 max-h-96 overflow-y-auto">
                                {allRoles.map((role) => (
                                    <li
                                        key={role.id}
                                        className="flex items-center justify-between bg-gray-100 p-3 rounded-md text-sm"
                                    >
                                        <span>{role.name}</span>
                                        <div className="space-x-2">
                                            <button
                                                onClick={() => handleRoleActionClick(role.id, 'add')}
                                                className="px-3 py-1 bg-amber-500 text-white text-xs rounded hover:bg-amber-600 disabled:opacity-50"
                                                disabled={modalLoading}
                                            >
                                                Add Role
                                            </button>
                                            <button
                                                onClick={() => handleRoleActionClick(role.id, 'remove')}
                                                className="px-3 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50"
                                                disabled={modalLoading}
                                            >
                                                Remove Role
                                            </button>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <p className="text-sm text-gray-500">
                                No roles available. Please check database.
                            </p>
                        )}
                    </>
                )}
                <div className="flex justify-end pt-4 sticky bottom-0 bg-white">
                    <button
                        type="button"
                        onClick={() => setViewRolesModalOpen(false)}
                        className="px-4 py-2 border border-gray-300 rounded-md text-sm bg-gray-50 hover:bg-gray-100"
                    >
                        Close
                    </button>
                </div>
            </Modal>

            {/* Role Confirmation Modal */}
            <ConfirmationModal
                isOpen={isConfirmRoleActionOpen}
                onClose={() => setConfirmRoleActionOpen(false)}
                onConfirm={confirmRoleAction}
                title={roleActionType === 'add' ? 'Confirm Add Role' : 'Confirm Remove Role'}
                message={`Are you sure you want to ${roleActionType} this role from the shift "${selectedShift?.name}"?`}
                confirmText={roleActionType === 'add' ? 'Add Role' : 'Remove Role'}
            />
        </div>
    );
};

export default ShiftAssignment;