import React, { useState } from 'react';
import { Eye, EyeOff, ChevronLeft, ChevronRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import useAuth from '../../helpers/auth.js'; // Adjust path as needed

export default function Login() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [currentSlide, setCurrentSlide] = useState(0);
    const [verificationCode, setVerificationCode] = useState('');

    const navigate = useNavigate();
    const {
        login,
        verify2FA,
        loading,
        error,
        requires2FA,
        maskedEmail,
        clearError,
        reset2FAState,
        getCurrentUser,
    } = useAuth();

    const slides = [
        {
            title: 'Unlock a world of Weighing and Construction opportunities',
            subtitle:
                'Access your dashboard to manage bids, track tender submissions, and connect with potential clients.',
            description:
                'Upload your documents and bid for tenders that match your expertise to impress potential clients.',
        },
        {
            title: 'Streamline Your Construction Projects',
            subtitle:
                'Manage all your weighing and construction projects from one centralized platform.',
            description:
                'Track progress, monitor deadlines, and collaborate with your team effectively.',
        },
        {
            title: 'Connect with Industry Leaders',
            subtitle:
                'Network with top construction professionals and expand your business reach.',
            description:
                'Build lasting relationships and discover new partnership opportunities.',
        },
    ];

    const handleRedirect = () => {
        const user = getCurrentUser();
        const userRoles = user?.userRoles || [];
        const primaryRole = userRoles[0];

        if (primaryRole === 'Admin') {
            navigate('/admin', { replace: true });
        } else if (primaryRole === 'Operator') {
            navigate('/operator', { replace: true });
        } else {
            navigate('/login', { replace: true }); // Fallback if no valid role
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!email || !password) {
            return; // Let the browser handle required field validation
        }

        const result = await login(email, password);

        if (result.success) {
            if (result.requires2FA) {
                console.log('2FA required, showing verification form');
            } else {
                // Direct login success (no 2FA required)
                handleRedirect();
            }
        }
        // Errors are handled by the useAuth hook and displayed via the error state
    };

    const handle2FASubmit = async (e) => {
        e.preventDefault();

        if (!verificationCode) {
            return; // Let the browser handle required field validation
        }

        const result = await verify2FA(verificationCode);

        if (result.success) {
            handleRedirect();
        }
        // Errors are handled by the useAuth hook and displayed via the error state
    };

    const handleBackToLogin = () => {
        reset2FAState();
        setVerificationCode('');
    };

    const nextSlide = () => setCurrentSlide((prev) => (prev + 1) % slides.length);
    const prevSlide = () =>
        setCurrentSlide((prev) => (prev - 1 + slides.length) % slides.length);

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4 sm:p-8">
            <div className="w-full max-w-6xl bg-white rounded-xl sm:rounded-2xl shadow-2xl overflow-hidden flex flex-col lg:flex-row">
                {/* Left Panel */}
                <div className="flex-1 flex items-center justify-center px-4 py-8 sm:px-8 sm:py-12 bg-gray-50">
                    <div className="w-full max-w-md">
                        <div className="mb-6 sm:mb-8">
                            <h1 className="text-xl sm:text-2xl font-bold text-gray-900">
                                Qalitrack
                            </h1>
                        </div>

                        <div className="mb-6 sm:mb-8">
                            <h2 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-2">
                                {requires2FA ? 'Enter Verification Code' : 'Welcome Back'}
                            </h2>
                            {requires2FA && (
                                <p className="text-sm text-gray-600">
                                    We've sent a verification code to {maskedEmail}
                                </p>
                            )}
                        </div>

                        {error && (
                            <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
                                {error}
                            </div>
                        )}

                        {!requires2FA ? (
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
                            <form onSubmit={handle2FASubmit} className="space-y-4 sm:space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Verification Code
                                    </label>
                                    <input
                                        type="text"
                                        value={verificationCode}
                                        onChange={(e) => {
                                            const value = e.target.value.replace(/\D/g, '').slice(0, 6);
                                            setVerificationCode(value);
                                            clearError();
                                        }}
                                        maxLength="6"
                                        required
                                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 text-center text-2xl tracking-widest font-mono"
                                        placeholder="000000"
                                    />
                                </div>

                                <button
                                    type="submit"
                                    disabled={loading || verificationCode.length !== 6}
                                    className="w-full py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                >
                                    {loading ? 'Verifying...' : 'Verify Code'}
                                </button>

                                <button
                                    type="button"
                                    onClick={handleBackToLogin}
                                    className="w-full py-2 px-4 text-sm text-gray-600 hover:text-gray-800 transition-colors"
                                >
                                    ← Back to Login
                                </button>
                            </form>
                        )}
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