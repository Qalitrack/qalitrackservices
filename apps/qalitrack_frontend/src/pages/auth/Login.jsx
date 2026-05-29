import React, { useState } from 'react';
import qalitrackLogoFull from '/src/assets/qalitrack_logo_full.png';
import { Eye, EyeOff, ChevronLeft, ChevronRight, AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import useAuth from '../../api/helpers/auth';

export default function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [currentSlide, setCurrentSlide] = useState(0);
    // Password change form states
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showCurrentPassword, setShowCurrentPassword] = useState(false);
    const [showNewPassword, setShowNewPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    const navigate = useNavigate();
    const {
        login,
        updatePassword,
        loading,
        error,
        requiresPasswordChange,
        clearError,
        resetPasswordChangeState,
        getCurrentUser,
    } = useAuth();

    const slides = [
        {
            title: 'Next-Level Weighing, Simplified',
            subtitle:
                'Capture accurate first and second weights without delay and generate tickets on demand.',
            description:
                'With real-time scale integration, every reading is reliably recorded and fully protected.',
        },
        {
            title: 'Engineered for Precise Weighing',
            subtitle:
                'Capture every weigh-in accurately and issue tickets instantly.',
            description:
                'Real-time integration ensures each record is consistent, secure, and safeguarded from tampering.',
        },
        {
            title: 'Smart Number Plate Recognition',
            subtitle:
                'Automatically capture vehicle plates with integrated camera support.',
            description:
                'Speed up weighbridge operations and reduce manual entry errors with NPR technology.',
        },
    ];

    const handleRedirect = () => {
        const user = getCurrentUser();
        const userRoles = user?.userRoles || [];
        const primaryRole = userRoles[0];

        if (primaryRole === 'Admin') {
            navigate('/admin', { replace: true });
        } else {
            // Operator and all other authenticated roles land on factory weighing
            navigate('/operator', { replace: true });
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!email || !password) {
            return; // Let the browser handle required field validation
        }

        const result = await login(email, password);

        if (result.success) {
            if (result.requiresPasswordChange) {
            } else {
                handleRedirect();
            }
        }
        // Errors are handled by the useAuth hook and displayed via the error state
    };

    const handlePasswordChangeSubmit = async (e) => {
        e.preventDefault();

        if (!currentPassword || !newPassword || !confirmPassword) {
            return;
        }

        if (newPassword !== confirmPassword) {
            return; // disabled button already prevents this; inline hint shown below
        }

        const result = await updatePassword(currentPassword, newPassword, confirmPassword);

        if (result.success) {
            handleRedirect();
        }
        // Errors are handled by the useAuth hook and displayed via the error state
    };

    const handleBackToLoginFromPasswordChange = () => {
        resetPasswordChangeState();
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
    };

    const nextSlide = () => setCurrentSlide((prev) => (prev + 1) % slides.length);
    const prevSlide = () =>
        setCurrentSlide((prev) => (prev - 1 + slides.length) % slides.length);

    const getFormTitle = () => {
        if (requiresPasswordChange) return 'Update Your Password';
        return 'Welcome Back';
    };

    const getFormSubtitle = () => {
        if (requiresPasswordChange) return 'Please update your password to continue';
        return null;
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4 sm:p-8">
            <div className="w-full max-w-6xl bg-white rounded-xl sm:rounded-2xl shadow-2xl overflow-hidden flex flex-col lg:flex-row">
                {/* Left Panel */}
                <div className="flex-1 flex items-center justify-center px-4 py-8 sm:px-8 sm:py-12 bg-gray-50">
                    <div className="w-full max-w-md">
                        <div className="mb-6 sm:mb-8">
                            <img
                                src={qalitrackLogoFull}
                                alt="QaliTrack"
                                className="h-12 w-auto object-contain"
                            />
                        </div>

                        <div className="mb-6 sm:mb-8">
                            <h2 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-2">
                                {getFormTitle()}
                            </h2>
                            {getFormSubtitle() && (
                                <p className="text-sm text-gray-600">
                                    {getFormSubtitle()}
                                </p>
                            )}
                        </div>

                        {error && (
                            <div className="mb-4 p-3 bg-red-50 border border-red-300 text-red-700 rounded-lg text-sm flex items-start gap-2">
                                <AlertCircle size={16} className="mt-0.5 shrink-0 text-red-500" />
                                <div>
                                    <p className="font-medium">{error}</p>
                                    {!requiresPasswordChange && (
                                        <p className="text-red-500 text-xs mt-0.5">
                                            Check your email and password and try again.
                                        </p>
                                    )}
                                </div>
                            </div>
                        )}

                        {!requiresPasswordChange ? (
                            // Regular login form
                            <form onSubmit={handleSubmit} className="space-y-4 sm:space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Email Address
                                    </label>
                                    <input
                                        type="email"
                                        value={email}
                                        onChange={(e) => {
                                            setEmail(e.target.value);
                                            clearError();
                                        }}
                                        required
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Password
                                    </label>
                                    <div className="relative">
                                        <input
                                            type={showPassword ? 'text' : 'password'}
                                            value={password}
                                            onChange={(e) => {
                                                setPassword(e.target.value);
                                                clearError();
                                            }}
                                            required
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 pr-10"
                                        />
                                        <button
                                            type="button"
                                            onClick={() => setShowPassword(!showPassword)}
                                            className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                                        >
                                            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                        </button>
                                    </div>
                                </div>

                                <button
                                    type="submit"
                                    disabled={loading}
                                    className="w-full py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                >
                                    {loading ? 'Logging in...' : 'Login'}
                                </button>
                            </form>
                        ) : (
                            // Password change form
                            <form onSubmit={handlePasswordChangeSubmit} className="space-y-4 sm:space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Current Password
                                    </label>
                                    <div className="relative">
                                        <input
                                            type={showCurrentPassword ? 'text' : 'password'}
                                            value={currentPassword}
                                            onChange={(e) => {
                                                setCurrentPassword(e.target.value);
                                                clearError();
                                            }}
                                            required
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 pr-10"
                                        />
                                        <button
                                            type="button"
                                            onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                                            className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                                        >
                                            {showCurrentPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                        </button>
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        New Password
                                    </label>
                                    <div className="relative">
                                        <input
                                            type={showNewPassword ? 'text' : 'password'}
                                            value={newPassword}
                                            onChange={(e) => {
                                                setNewPassword(e.target.value);
                                                clearError();
                                            }}
                                            required
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 pr-10"
                                        />
                                        <button
                                            type="button"
                                            onClick={() => setShowNewPassword(!showNewPassword)}
                                            className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                                        >
                                            {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                        </button>
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Confirm New Password
                                    </label>
                                    <div className="relative">
                                        <input
                                            type={showConfirmPassword ? 'text' : 'password'}
                                            value={confirmPassword}
                                            onChange={(e) => {
                                                setConfirmPassword(e.target.value);
                                                clearError();
                                            }}
                                            required
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 pr-10"
                                        />
                                        <button
                                            type="button"
                                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                            className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
                                        >
                                            {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                        </button>
                                    </div>
                                    {newPassword && confirmPassword && newPassword !== confirmPassword && (
                                        <p className="text-red-500 text-sm mt-1">Passwords do not match</p>
                                    )}
                                </div>

                                <button
                                    type="submit"
                                    disabled={loading || !currentPassword || !newPassword || !confirmPassword || newPassword !== confirmPassword}
                                    className="w-full py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                >
                                    {loading ? 'Updating Password...' : 'Update Password'}
                                </button>

                                <button
                                    type="button"
                                    onClick={handleBackToLoginFromPasswordChange}
                                    className="w-full py-2 px-4 text-sm text-gray-600 hover:text-gray-800 transition-colors"
                                >
                                    ← Back to Login
                                </button>
                            </form>
                        )}
                        <div className="mt-8 pt-6 border-t border-gray-200 text-center">
                            <p className="text-xs text-gray-400">
                                Powered by <span className="font-semibold text-gray-500">Qalibrated Systems</span>
                            </p>
                            <p className="text-xs text-gray-400 mt-0.5">
                                &copy; {new Date().getFullYear()} Qalibrated Systems. All rights reserved.
                            </p>
                        </div>
                    </div>
                </div>

                {/* Right Panel (Carousel) */}
                <div className="flex-1 bg-gradient-to-br from-amber-400 via-amber-500 to-amber-600 relative overflow-hidden min-h-[400px] lg:min-h-0">
                    <div className="h-full flex items-center justify-center p-4 sm:p-8">
                        <div className="text-center text-white max-w-md w-full">
                            <div className="mb-4 sm:mb-6">
                                <h2 className="text-lg sm:text-xl lg:text-2xl font-bold mb-2 leading-tight">
                                    {slides[currentSlide].title}
                                </h2>
                                <p className="text-amber-50 mb-3 sm:mb-4 text-sm sm:text-base">
                                    {slides[currentSlide].subtitle}
                                </p>
                            </div>

                            <p className="text-amber-50 text-xs sm:text-sm mb-4 sm:mb-6 px-2">
                                {slides[currentSlide].description}
                            </p>

                            <div className="flex items-center justify-between px-2">
                                <button
                                    onClick={prevSlide}
                                    className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30 transition-colors"
                                >
                                    <ChevronLeft size={20} className="text-white" />
                                </button>

                                <div className="flex space-x-1.5 sm:space-x-2">
                                    {slides.map((_, index) => (
                                        <button
                                            key={index}
                                            onClick={() => setCurrentSlide(index)}
                                            className={`w-2.5 h-2.5 rounded-full transition-colors ${
                                                index === currentSlide ? 'bg-white' : 'bg-white/40'
                                            }`}
                                        />
                                    ))}
                                </div>

                                <button
                                    onClick={nextSlide}
                                    className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30 transition-colors"
                                >
                                    <ChevronRight size={20} className="text-white" />
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}