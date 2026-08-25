import React, { useState, useEffect } from 'react';
import { Edit2, X, Check, Lock, Mail } from 'lucide-react';
import { fetchPasswordPolicy, updatePasswordPolicy } from '../../api/helpers/UserService/PasswordPolicy/passwordpolicy';
import { fetchEmailSettings, updateEmailSettings, sendTestEmail } from '../../api/helpers/UserService/EmailSettings/emailSettings';
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

// SMTP settings this install actually sends 2FA/notification email through —
// different clients use different Gmail accounts, so this is per-install
// configurable rather than baked into deployment config. The password field
// is write-only: the server never sends the saved password back, so this
// only ever shows blank (type a new one to change it, leave it blank to keep
// what's already saved).
function EmailSettingsSection() {
    const [settings, setSettings] = useState(null);
    const [passwordInput, setPasswordInput] = useState('');
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState({ text: '', type: '' });
    const [testEmail, setTestEmail] = useState('');
    const [testing, setTesting] = useState(false);

    const loadSettings = async () => {
        setLoading(true);
        try {
            const data = await fetchEmailSettings();
            setSettings(data);
        } catch (err) {
            setMessage({ text: err.message || 'Failed to load email settings.', type: 'error' });
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadSettings();
    }, []);

    const setField = (key, value) => setSettings((prev) => ({ ...prev, [key]: value }));

    const handleSave = async () => {
        setSaving(true);
        setMessage({ text: '', type: '' });
        try {
            const updated = await updateEmailSettings({
                smtpHost: settings.smtpHost,
                smtpPort: Number(settings.smtpPort) || 587,
                smtpUsername: settings.smtpUsername,
                smtpPassword: passwordInput || undefined,
                fromEmail: settings.fromEmail,
                fromName: settings.fromName,
                enableSsl: settings.enableSsl,
            });
            setSettings(updated);
            setPasswordInput('');
            setMessage({ text: 'Email settings saved.', type: 'success' });
        } catch (err) {
            setMessage({ text: err.response?.data || err.message || 'Failed to save email settings.', type: 'error' });
        } finally {
            setSaving(false);
            setTimeout(() => setMessage({ text: '', type: '' }), 5000);
        }
    };

    const handleTest = async () => {
        if (!testEmail) return;
        setTesting(true);
        setMessage({ text: '', type: '' });
        try {
            await sendTestEmail(testEmail);
            setMessage({ text: `Test email sent to ${testEmail}.`, type: 'success' });
        } catch (err) {
            setMessage({ text: err.response?.data?.message || err.message || 'Failed to send test email.', type: 'error' });
        } finally {
            setTesting(false);
        }
    };

    if (loading || !settings) {
        return (
            <div className="max-w-md mx-auto flex items-center justify-center py-8">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-amber-500"></div>
            </div>
        );
    }

    return (
        <div className="max-w-md mx-auto space-y-4 pb-6 mb-6 border-b border-gray-200">
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                    <Mail className="w-4 h-4 text-gray-500" />
                    <h3 className="text-sm font-bold text-gray-800">Email / SMTP Settings</h3>
                </div>
                <span className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${
                    settings.isConfigured ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'
                }`}>
                    {settings.isConfigured ? 'Configured' : 'Not configured'}
                </span>
            </div>

            {message.text && (
                <div className={`px-3 py-2 rounded-md text-xs font-medium border ${
                    message.type === 'success'
                        ? 'bg-green-50 border-green-200 text-green-700'
                        : 'bg-red-50 border-red-200 text-red-700'
                }`}>
                    {message.text}
                </div>
            )}

            <div className="grid grid-cols-2 gap-3">
                <div>
                    <label className="block text-xs font-semibold text-gray-700 mb-1">SMTP Host</label>
                    <input
                        value={settings.smtpHost || ''}
                        onChange={(e) => setField('smtpHost', e.target.value)}
                        placeholder="smtp.gmail.com"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                    />
                </div>
                <div>
                    <label className="block text-xs font-semibold text-gray-700 mb-1">Port</label>
                    <input
                        type="number"
                        value={settings.smtpPort || 587}
                        onChange={(e) => setField('smtpPort', e.target.value)}
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                    />
                </div>
            </div>

            <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">Gmail Address (App Username)</label>
                <input
                    value={settings.smtpUsername || ''}
                    onChange={(e) => setField('smtpUsername', e.target.value)}
                    placeholder="yourcompany@gmail.com"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                />
            </div>

            <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">Gmail App Password</label>
                <input
                    type="password"
                    value={passwordInput}
                    onChange={(e) => setPasswordInput(e.target.value)}
                    placeholder={settings.isPasswordSet ? 'Leave blank to keep the saved password' : 'Enter the 16-character app password'}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none font-mono"
                />
            </div>

            <div className="grid grid-cols-2 gap-3">
                <div>
                    <label className="block text-xs font-semibold text-gray-700 mb-1">From Email</label>
                    <input
                        value={settings.fromEmail || ''}
                        onChange={(e) => setField('fromEmail', e.target.value)}
                        placeholder="noreply@qalitrack.com"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                    />
                </div>
                <div>
                    <label className="block text-xs font-semibold text-gray-700 mb-1">From Name</label>
                    <input
                        value={settings.fromName || ''}
                        onChange={(e) => setField('fromName', e.target.value)}
                        placeholder="QaliTrack System"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                    />
                </div>
            </div>

            <label className="flex items-center justify-between px-4 py-3 rounded-md border bg-white border-gray-200 cursor-pointer">
                <span className="text-sm font-medium text-gray-700">Use TLS (recommended)</span>
                <input
                    type="checkbox"
                    checked={!!settings.enableSsl}
                    onChange={(e) => setField('enableSsl', e.target.checked)}
                    className="h-4 w-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500"
                />
            </label>

            <button
                onClick={handleSave}
                disabled={saving}
                className="w-full h-8 text-xs font-semibold rounded shadow-sm disabled:opacity-50 bg-amber-500 hover:bg-amber-600 text-white"
            >
                {saving ? 'Saving...' : 'Save Email Settings'}
            </button>

            <div className="flex gap-2 pt-1">
                <input
                    value={testEmail}
                    onChange={(e) => setTestEmail(e.target.value)}
                    placeholder="Send a test email to…"
                    className="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-1 focus:ring-amber-400 focus:border-amber-400 outline-none"
                />
                <button
                    onClick={handleTest}
                    disabled={testing || !testEmail}
                    className="h-9 px-3 text-xs font-semibold rounded border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 shrink-0"
                >
                    {testing ? 'Sending...' : 'Send Test'}
                </button>
            </div>
        </div>
    );
}

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
            const serverMessage = typeof err.response?.data === 'string' ? err.response.data : err.response?.data?.message;
            setUpdateMessage({ text: serverMessage || err.message || 'Failed to update policy.', type: 'error' });
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
            <div className="shrink-0 flex items-center justify-end gap-2 px-3 py-2 border-b border-gray-200 bg-gray-50">
                {!isEditing ? (
                    <button
                        onClick={() => setIsEditing(true)}
                        className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow-sm transition-all bg-amber-500 hover:bg-amber-600 text-white"
                    >
                        <Edit2 className="w-3 h-3" />
                        Edit
                    </button>
                ) : (
                    <>
                        <button
                            onClick={handleCancel}
                            className="flex items-center gap-1.5 h-7 px-3 text-xs font-medium border border-gray-300 rounded text-gray-600 hover:bg-gray-100"
                        >
                            <X className="w-3 h-3" /> Cancel
                        </button>
                        <button
                            onClick={handleUpdate}
                            disabled={isUpdating}
                            className="flex items-center gap-1.5 h-7 px-3 text-xs font-semibold rounded shadow-sm disabled:opacity-50 bg-amber-500 hover:bg-amber-600 text-white"
                        >
                            <Check className="w-3 h-3" />
                            {isUpdating ? 'Saving...' : 'Save'}
                        </button>
                    </>
                )}
            </div>

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
                <EmailSettingsSection />

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
