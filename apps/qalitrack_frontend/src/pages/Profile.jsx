import React, { useState, useEffect } from 'react';
import {
  User, Mail, Shield, Key, Edit2, Check, X,
  Eye, EyeOff, Loader2,
} from 'lucide-react';
import useAuth from '../api/helpers/auth';
import { fetchUserById, updateUser } from '../api/helpers/UserService/Users/users';
import { apiClient } from '../api/helpers/apiClients';
import toast from 'react-hot-toast';

// ─── helpers ────────────────────────────────────────────────────────────────

const extractApiError = (err) => {
  const d = err?.response?.data;
  return d?.data?.message || d?.message || d?.error || err?.message || 'Something went wrong.';
};

const initials = (first, last, email) => {
  if (first && last)  return `${first[0]}${last[0]}`.toUpperCase();
  if (first)          return first[0].toUpperCase();
  if (email)          return email[0].toUpperCase();
  return '?';
};

// ─── sub-components ─────────────────────────────────────────────────────────

const SectionCard = ({ title, icon: Icon, children }) => (
  <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
    <div className="flex items-center gap-2 px-6 py-4 border-b border-gray-100">
      <Icon size={18} className="text-amber-500" />
      <h2 className="text-base font-semibold text-gray-800">{title}</h2>
    </div>
    <div className="p-6">{children}</div>
  </div>
);

const FieldRow = ({ label, value }) => (
  <div className="flex items-center justify-between py-3 border-b border-gray-50 last:border-0">
    <span className="text-sm text-gray-500 w-36 shrink-0">{label}</span>
    <span className="text-sm font-medium text-gray-800 text-right break-all">{value || '—'}</span>
  </div>
);

const InputField = ({ label, name, value, onChange, type = 'text', disabled }) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">{label}</label>
    <input
      type={type}
      name={name}
      value={value}
      onChange={onChange}
      disabled={disabled}
      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg
        focus:outline-none focus:ring-2 focus:ring-amber-400 focus:border-amber-400
        disabled:bg-gray-50 disabled:text-gray-400 transition-colors"
    />
  </div>
);

const PasswordField = ({ label, name, value, onChange, visible, onToggle, disabled }) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">{label}</label>
    <div className="relative">
      <input
        type={visible ? 'text' : 'password'}
        name={name}
        value={value}
        onChange={onChange}
        disabled={disabled}
        className="w-full px-3 py-2 pr-10 text-sm border border-gray-300 rounded-lg
          focus:outline-none focus:ring-2 focus:ring-amber-400 focus:border-amber-400
          disabled:bg-gray-50 transition-colors"
      />
      <button
        type="button"
        onClick={onToggle}
        className="absolute inset-y-0 right-3 flex items-center text-gray-400 hover:text-gray-600"
        tabIndex={-1}
      >
        {visible ? <EyeOff size={16} /> : <Eye size={16} />}
      </button>
    </div>
  </div>
);

// ─── main component ──────────────────────────────────────────────────────────

export default function Profile() {
  const { getCurrentUser } = useAuth();
  const sessionUser = getCurrentUser();

  // ── profile data ──
  const [profile, setProfile]         = useState(null);
  const [loading, setLoading]         = useState(true);

  // ── personal info edit ──
  const [isEditing, setIsEditing]     = useState(false);
  const [editForm, setEditForm]       = useState({ firstName: '', lastName: '', email: '' });
  const [saving, setSaving]           = useState(false);

  // ── password change ──
  const [pwForm, setPwForm]           = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [showPw, setShowPw]           = useState({ current: false, new: false, confirm: false });
  const [changingPw, setChangingPw]   = useState(false);

  // ── load profile ──────────────────────────────────────────────────────────
  useEffect(() => {
    if (!sessionUser?.id) { setLoading(false); return; }

    fetchUserById(sessionUser.id)
      .then((res) => {
        const user = res?.data ?? res;
        setProfile(user);
        setEditForm({
          firstName: user.firstName || '',
          lastName:  user.lastName  || '',
          email:     user.email     || '',
        });
      })
      .catch(() => {
        // fall back to session cache
        setProfile(sessionUser);
        setEditForm({
          firstName: sessionUser.firstName || '',
          lastName:  sessionUser.lastName  || '',
          email:     sessionUser.email     || '',
        });
      })
      .finally(() => setLoading(false));
  }, []);

  // ── save personal info ────────────────────────────────────────────────────
  const handleSaveInfo = async () => {
    if (!editForm.firstName.trim() || !editForm.lastName.trim()) {
      toast.error('First name and last name are required.');
      return;
    }
    setSaving(true);
    try {
      await updateUser(sessionUser.id, {
        firstName: editForm.firstName.trim(),
        lastName:  editForm.lastName.trim(),
        email:     editForm.email.trim(),
      });
      setProfile(prev => ({ ...prev, ...editForm }));
      setIsEditing(false);
      toast.success('Profile updated successfully!');
    } catch (err) {
      toast.error(extractApiError(err));
    } finally {
      setSaving(false);
    }
  };

  const handleCancelEdit = () => {
    setEditForm({
      firstName: profile?.firstName || '',
      lastName:  profile?.lastName  || '',
      email:     profile?.email     || '',
    });
    setIsEditing(false);
  };

  // ── change password ───────────────────────────────────────────────────────
  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (!pwForm.currentPassword || !pwForm.newPassword || !pwForm.confirmPassword) {
      toast.error('Please fill in all password fields.');
      return;
    }
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      toast.error('New password and confirmation do not match.');
      return;
    }
    if (pwForm.newPassword.length < 6) {
      toast.error('New password must be at least 6 characters.');
      return;
    }
    setChangingPw(true);
    try {
      await apiClient.put(`/Auth/update-password/${sessionUser.id}`, {
        currentPassword: pwForm.currentPassword,
        newPassword:     pwForm.newPassword,
        confirmPassword: pwForm.confirmPassword,
      });
      toast.success('Password changed successfully!');
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } catch (err) {
      toast.error(extractApiError(err));
    } finally {
      setChangingPw(false);
    }
  };

  // ── derived display values ────────────────────────────────────────────────
  const displayName = profile
    ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || profile.email
    : sessionUser?.email || 'User';

  const role = (() => {
    const r = (profile?.userRoles || sessionUser?.userRoles || [])[0];
    if (!r) return 'User';
    return typeof r === 'string' ? r : r.name || r.role || 'User';
  })();

  const avatarText = initials(profile?.firstName, profile?.lastName, profile?.email);

  // ── render ────────────────────────────────────────────────────────────────
  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 size={32} className="animate-spin text-amber-500" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8 space-y-6">

      {/* ── profile header ─────────────────────────────────────────────── */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex flex-col sm:flex-row items-center sm:items-start gap-5">
          {/* Avatar */}
          <div className="h-20 w-20 rounded-full bg-amber-100 flex items-center justify-center shrink-0 border-2 border-amber-200">
            <span className="text-2xl font-bold text-amber-600">{avatarText}</span>
          </div>

          {/* Name / role / ID */}
          <div className="flex-1 text-center sm:text-left">
            <h1 className="text-xl font-bold text-gray-900">{displayName}</h1>
            <p className="text-sm text-gray-500 mt-0.5">{profile?.email || sessionUser?.email}</p>
            <div className="mt-2 flex items-center justify-center sm:justify-start gap-2">
              <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-100 text-amber-700 border border-amber-200">
                <Shield size={11} />
                {role}
              </span>
            </div>
            {(profile?.id || sessionUser?.id) && (
              <p className="text-xs text-gray-400 mt-2 font-mono">
                ID: {profile?.id || sessionUser?.id}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* ── personal information ───────────────────────────────────────── */}
      <SectionCard title="Personal Information" icon={User}>
        {isEditing ? (
          <div className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <InputField
                label="First Name"
                name="firstName"
                value={editForm.firstName}
                onChange={(e) => setEditForm(p => ({ ...p, firstName: e.target.value }))}
                disabled={saving}
              />
              <InputField
                label="Last Name"
                name="lastName"
                value={editForm.lastName}
                onChange={(e) => setEditForm(p => ({ ...p, lastName: e.target.value }))}
                disabled={saving}
              />
            </div>
            <InputField
              label="Email Address"
              name="email"
              type="email"
              value={editForm.email}
              onChange={(e) => setEditForm(p => ({ ...p, email: e.target.value }))}
              disabled={saving}
            />

            <div className="flex items-center gap-3 pt-2">
              <button
                onClick={handleSaveInfo}
                disabled={saving}
                className="flex items-center gap-2 px-5 py-2 bg-amber-500 hover:bg-amber-600
                  text-white text-sm font-medium rounded-lg transition-colors
                  disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {saving
                  ? <Loader2 size={15} className="animate-spin" />
                  : <Check size={15} />}
                {saving ? 'Saving…' : 'Save Changes'}
              </button>
              <button
                onClick={handleCancelEdit}
                disabled={saving}
                className="flex items-center gap-2 px-5 py-2 border border-gray-300
                  text-gray-600 text-sm font-medium rounded-lg hover:bg-gray-50 transition-colors"
              >
                <X size={15} /> Cancel
              </button>
            </div>
          </div>
        ) : (
          <div>
            <FieldRow label="First Name"  value={profile?.firstName} />
            <FieldRow label="Last Name"   value={profile?.lastName} />
            <FieldRow label="Email"       value={profile?.email} />
            <FieldRow label="Role"        value={role} />

            <div className="mt-4">
              <button
                onClick={() => setIsEditing(true)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-amber-600
                  border border-amber-300 rounded-lg hover:bg-amber-50 transition-colors"
              >
                <Edit2 size={14} /> Edit Profile
              </button>
            </div>
          </div>
        )}
      </SectionCard>

      {/* ── change password ────────────────────────────────────────────── */}
      <SectionCard title="Change Password" icon={Key}>
        <form onSubmit={handleChangePassword} className="space-y-4">
          <PasswordField
            label="Current Password"
            name="currentPassword"
            value={pwForm.currentPassword}
            onChange={(e) => setPwForm(p => ({ ...p, currentPassword: e.target.value }))}
            visible={showPw.current}
            onToggle={() => setShowPw(p => ({ ...p, current: !p.current }))}
            disabled={changingPw}
          />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <PasswordField
              label="New Password"
              name="newPassword"
              value={pwForm.newPassword}
              onChange={(e) => setPwForm(p => ({ ...p, newPassword: e.target.value }))}
              visible={showPw.new}
              onToggle={() => setShowPw(p => ({ ...p, new: !p.new }))}
              disabled={changingPw}
            />
            <PasswordField
              label="Confirm New Password"
              name="confirmPassword"
              value={pwForm.confirmPassword}
              onChange={(e) => setPwForm(p => ({ ...p, confirmPassword: e.target.value }))}
              visible={showPw.confirm}
              onToggle={() => setShowPw(p => ({ ...p, confirm: !p.confirm }))}
              disabled={changingPw}
            />
          </div>

          {/* password strength hint */}
          {pwForm.newPassword && (
            <p className={`text-xs ${pwForm.newPassword.length >= 8 ? 'text-green-600' : 'text-amber-600'}`}>
              {pwForm.newPassword.length >= 8
                ? '✓ Password length looks good'
                : `Password should be at least 8 characters (${pwForm.newPassword.length}/8)`}
            </p>
          )}

          <div className="pt-1">
            <button
              type="submit"
              disabled={changingPw}
              className="flex items-center gap-2 px-5 py-2 bg-amber-500 hover:bg-amber-600
                text-white text-sm font-medium rounded-lg transition-colors
                disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {changingPw
                ? <Loader2 size={15} className="animate-spin" />
                : <Key size={15} />}
              {changingPw ? 'Updating…' : 'Update Password'}
            </button>
          </div>
        </form>
      </SectionCard>

    </div>
  );
}
