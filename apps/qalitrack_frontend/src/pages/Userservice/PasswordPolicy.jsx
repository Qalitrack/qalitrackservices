import React, { useState, useEffect } from 'react';
import { Edit2, X, Check, Lock } from 'lucide-react';
import { fetchPasswordPolicy, updatePasswordPolicy } from '../../api/helpers/UserService/PasswordPolicy/passwordpolicy';
import PageHeader from '../../components/PageHeader.jsx';

const FIELDS = [
    { key: 'minimumLength', label: 'Minimum Length', type: 'number' },
    { key: 'maxAgeDays', label: 'Max Age (Days)', type: 'number' },
];

const CHECKBOXES = [
    { key: 'requireUppercase', label: 'Require Uppercase Letter' },
    { key: 'requireLowercase', label: 'Require Lowercase Letter' },
    { key: 'requireDigit', label: 'Require Digit' },
    { key: 'requireSpecialCharacter', label: 'Require Special Character' },
    { key: 'twoFactorEnabled', label: 'Enable Two-Factor Authentication' },
];

const PasswordPolicy = () => {
    const [policy, setPolicy] = useState(null);
    const [originalPolicy, setOriginalPolicy] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isEditing, setIsEditing] = useState(false);
    const [isUpdating, setIsUpdating] = useState(false);
    const [error, setError] = useState(null);
    const [updateMessage, setUpdateMessage] = useState({ text: '', type: '' });

    const loadPolicy = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchPasswordPolicy();
            setPolicy(data);
            setOriginalPolicy(data);
        } catch (err) {
            setError(err.message || 'Failed to fetch policy.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadPolicy();
    }, []);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setPolicy(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : Number(value),
        }));
    };

    const handleUpdate = async (e) => {
        e.preventDefault();
        setIsUpdating(true);
        setUpdateMessage({ text: 'Updating policy...', type: 'info' });
        try {
            const updatedData = await updatePasswordPolicy(policy);
            if (updatedData && typeof updatedData === 'object') {
                setPolicy(updatedData);
                setOriginalPolicy(updatedData);
            } else {
                await loadPolicy();
            }
            setUpdateMessage({ text: 'Password policy updated successfully!', type: 'success' });
            setIsEditing(false);
        } catch (err) {
            setUpdateMessage({ text: err.message || 'Failed to update policy.', type: 'error' });
        } finally {
            setIsUpdating(false);
            setTimeout(() => setUpdateMessage({ text: '', type: '' }), 5000);
        }
    };

    const handleCancel = () => {
        setPolicy(originalPolicy);
        setIsEditing(false);
    };

    if (loading) {
        return (
            <div className="h-full flex items-center justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="h-full p-4">
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md text-sm">{error}</div>
            </div>
        );
    }

    // Single form, always the same shape — read-only fields when not
    // editing instead of a separate view-mode rendering path.
    return (
        <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <PageHeader
                flush
                icon={Lock}
                title="Password Policy"
                subtitle="Rules new and changed passwords must satisfy"
                actions={
                    !isEditing ? (
                        <button
                            onClick={() => setIsEditing(true)}
                            className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow-sm transition-all"
                            style={{ backgroundColor: "#ffffff", color: "var(--cs-appbar-bg)" }}
                        >
                            <Edit2 className="w-3 h-3" />
                            Edit
                        </button>
                    ) : (
                        <>
                            <button
                                onClick={handleCancel}
                                className="flex items-center gap-1.5 h-7 px-3 text-xs font-medium border border-white/30 rounded text-white hover:bg-white/10"
                            >
                                <X className="w-3 h-3" /> Cancel
                            </button>
                            <button
                                onClick={handleUpdate}
                                disabled={isUpdating}
                                className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow-sm disabled:opacity-50"
                                style={{ backgroundColor: "#ffffff", color: "var(--cs-appbar-bg)" }}
                            >
                                <Check className="w-3 h-3" />
                                {isUpdating ? 'Saving...' : 'Save'}
                            </button>
                        </>
                    )
                }
            />

            {updateMessage.text && (
                <div className={`mx-4 mt-3 px-4 py-2 rounded-md text-sm font-medium border ${
                    updateMessage.type === 'success'
                        ? 'bg-green-50 border-green-200 text-green-700'
                        : updateMessage.type === 'error'
                        ? 'bg-red-50 border-red-200 text-red-700'
                        : 'bg-blue-50 border-blue-200 text-blue-700'
                }`}>
                    {updateMessage.text}
                </div>
            )}

            <div className="flex-1 overflow-auto p-4">
                <div className="max-w-md mx-auto space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        {FIELDS.map(({ key, label, type }) => (
                            <div key={key}>
                                <label htmlFor={key} className="block text-sm font-semibold text-gray-800 mb-1">{label}</label>
                                <input
                                    id={key} name={key} type={type}
                                    value={policy[key]} onChange={handleChange}
                                    disabled={!isEditing}
                                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none disabled:bg-gray-50 disabled:text-gray-600"
                                    min={0}
                                />
                            </div>
                        ))}
                    </div>

                    <div className="space-y-2">
                        {CHECKBOXES.map(({ key, label }) => (
                            <label
                                key={key}
                                htmlFor={key}
                                className={`flex items-center justify-between px-4 py-3 rounded-md border ${
                                    isEditing ? "bg-white border-gray-200 cursor-pointer" : "bg-gray-50 border-gray-100"
                                }`}
                            >
                                <span className="text-sm font-medium text-gray-700">{label}</span>
                                <input
                                    id={key} name={key} type="checkbox"
                                    checked={policy[key]} onChange={handleChange}
                                    disabled={!isEditing}
                                    className="h-4 w-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
                                />
                            </label>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PasswordPolicy;
