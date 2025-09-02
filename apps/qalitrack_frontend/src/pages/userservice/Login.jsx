import { useState } from "react";
import { Eye, EyeOff, ChevronLeft, ChevronRight } from "lucide-react";
import useAuth from "./Auth/Auth.js";

export default function Login() {
    const [email, setEmail] = useState("Daphne Smith");
    const [password, setPassword] = useState("••••••••••••");
    const [showPassword, setShowPassword] = useState(false);
    const [currentSlide, setCurrentSlide] = useState(0);

    const slides = [
        {
            title: "Unlock a world of Weighing and construction opportunities",
            subtitle: "Access your dashboard to manage bids, track tender submissions, and connect with potential clients.",
            description: "Upload your documents and bid for tenders that match your expertise to impress potential clients."
        },
        {
            title: "Streamline Your Construction Projects",
            subtitle: "Manage all your weighing and construction projects from one centralized platform.",
            description: "Track progress, monitor deadlines, and collaborate with your team effectively."
        },
        {
            title: "Connect with Industry Leaders",
            subtitle: "Network with top construction professionals and expand your business reach.",
            description: "Build lasting relationships and discover new partnership opportunities."
        }
    ];

    const { login, loading, error, clearError } = useAuth();

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Clear any previous errors
        clearError();

        // Validate inputs
        if (!email || !password) {
            alert("Please enter both email and password");
            return;
        }

        try {
            const result = await login(email, password);

            if (result.success) {
                console.log("Login successful!");
            } else {
                // Login failed - error is already set in the hook
                console.log("Login failed:", result.error);
            }
        } catch (err) {
            console.error("Unexpected error during login:", err);
        }
    };

    const nextSlide = () => {
        setCurrentSlide((prev) => (prev + 1) % slides.length);
    };

    const prevSlide = () => {
        setCurrentSlide((prev) => (prev - 1 + slides.length) % slides.length);
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4 sm:p-8">
            <div className="w-full max-w-6xl bg-white rounded-xl sm:rounded-2xl shadow-2xl overflow-hidden flex flex-col lg:flex-row">
                {/* Left Panel - Login Form */}
                <div className="flex-1 flex items-center justify-center px-4 py-8 sm:px-8 sm:py-12 bg-gray-50">
                    <div className="w-full max-w-md">
                        {/* Logo */}
                        <div className="mb-6 sm:mb-8">
                            <h1 className="text-xl sm:text-2xl font-bold text-gray-900">Qalitrack</h1>
                        </div>

                        {/* Welcome Message */}
                        <div className="mb-6 sm:mb-8">
                            <h2 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-2">
                                Welcome Back👋
                            </h2>
                        </div>

                        {/* Login Form */}
                        <div className="space-y-4 sm:space-y-6">
                            <div>
                                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                                    Email Address
                                </label>
                                <input
                                    id="email"
                                    type="email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="w-full px-3 py-2 sm:px-4 sm:py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 transition-colors bg-white text-gray-900 text-sm sm:text-base"
                                    required
                                />
                            </div>

                            <div>
                                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                                    Password
                                </label>
                                <div className="relative">
                                    <input
                                        id="password"
                                        type={showPassword ? "text" : "password"}
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        className="w-full px-3 py-2 sm:px-4 sm:py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 transition-colors bg-white text-gray-900 pr-10 sm:pr-12 text-sm sm:text-base"
                                        required
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowPassword(!showPassword)}
                                        className="absolute right-3 sm:right-4 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors"
                                    >
                                        {showPassword ? <EyeOff size={18} className="sm:w-5 sm:h-5" /> : <Eye size={18} className="sm:w-5 sm:h-5" />}
                                    </button>
                                </div>
                            </div>

                            <button
                                type="button"
                                onClick={handleSubmit}
                                className="w-full bg-amber-500 text-white py-2.5 sm:py-3 px-4 rounded-lg font-medium hover:bg-amber-600 focus:ring-2 focus:ring-amber-500 focus:ring-offset-2 transition-colors text-sm sm:text-base"
                            >
                                Login
                            </button>
                        </div>

                        {/* Sign Up Link */}
                        <p className="text-center text-gray-600 mt-4 sm:mt-6 text-sm">
                            Don't have an account?{" "}
                            <a href="#" className="text-amber-500 hover:text-amber-600 font-medium transition-colors">
                                Sign up
                            </a>
                        </p>
                    </div>
                </div>

                {/* Right Panel - Carousel */}
                <div className="flex-1 bg-gradient-to-br from-amber-400 via-amber-500 to-amber-600 relative overflow-hidden min-h-[400px] lg:min-h-0">
                    <div className="h-full flex items-center justify-center p-4 sm:p-8">
                        <div className="text-center text-white max-w-md w-full">
                            {/* Main Content */}
                            <div className="mb-4 sm:mb-6">
                                <h2 className="text-lg sm:text-xl lg:text-2xl font-bold mb-2 sm:mb-3 leading-tight">
                                    {slides[currentSlide].title}
                                </h2>
                                <p className="text-amber-50 mb-3 sm:mb-4 text-sm sm:text-base">
                                    {slides[currentSlide].subtitle}
                                </p>
                            </div>

                            {/* 3D Illustration */}
                            <div className="mb-4 sm:mb-6 relative">
                                <div className="w-48 h-36 sm:w-56 sm:h-40 lg:w-64 lg:h-48 mx-auto bg-gradient-to-b from-amber-300 to-amber-400 rounded-lg sm:rounded-xl shadow-xl transform perspective-1000 rotate-x-12 relative overflow-hidden">
                                    {/* Construction Site Illustration */}
                                    <div className="absolute inset-2 sm:inset-4 bg-gray-700 rounded-md sm:rounded-lg">
                                        {/* Road */}
                                        <div className="absolute bottom-0 left-0 right-0 h-6 sm:h-8 bg-gray-600">
                                            <div className="absolute top-1/2 left-0 right-0 h-0.5 bg-yellow-400 transform -translate-y-1/2"></div>
                                        </div>

                                        {/* Buildings */}
                                        <div className="absolute bottom-6 sm:bottom-8 left-2 sm:left-4 w-8 sm:w-12 h-10 sm:h-16 bg-blue-600 rounded-t-lg shadow-lg"></div>
                                        <div className="absolute bottom-6 sm:bottom-8 right-2 sm:right-4 w-10 sm:w-16 h-8 sm:h-12 bg-gray-300 rounded shadow-lg"></div>

                                        {/* Trucks */}
                                        <div className="absolute bottom-6 sm:bottom-8 left-12 sm:left-20 w-6 sm:w-8 h-3 sm:h-4 bg-white rounded shadow-md"></div>
                                        <div className="absolute bottom-6 sm:bottom-8 left-20 sm:left-32 w-7 sm:w-10 h-3 sm:h-5 bg-blue-500 rounded shadow-md"></div>

                                        {/* Construction Elements */}
                                        <div className="absolute top-2 sm:top-4 left-4 sm:left-8 w-1.5 sm:w-2 h-1.5 sm:h-2 bg-orange-500 rounded-full"></div>
                                        <div className="absolute top-3 sm:top-6 left-6 sm:left-12 w-1.5 sm:w-2 h-1.5 sm:h-2 bg-orange-500 rounded-full"></div>
                                        <div className="absolute top-2 sm:top-4 right-4 sm:right-8 w-1.5 sm:w-2 h-1.5 sm:h-2 bg-orange-500 rounded-full"></div>

                                        {/* Green area */}
                                        <div className="absolute bottom-0 right-0 w-4 sm:w-6 h-4 sm:h-6 bg-green-500 rounded-tl-lg"></div>
                                    </div>
                                </div>
                            </div>

                            {/* Bottom Description */}
                            <p className="text-amber-50 text-xs sm:text-sm mb-4 sm:mb-6 px-2">
                                {slides[currentSlide].description}
                            </p>

                            {/* Navigation */}
                            <div className="flex items-center justify-between px-2">
                                <button
                                    onClick={prevSlide}
                                    className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30 transition-colors"
                                >
                                    <ChevronLeft size={20} className="text-white sm:w-6 sm:h-6" />
                                </button>

                                {/* Dots Indicator */}
                                <div className="flex space-x-1.5 sm:space-x-2">
                                    {slides.map((_, index) => (
                                        <button
                                            key={index}
                                            onClick={() => setCurrentSlide(index)}
                                            className={`w-2.5 h-2.5 sm:w-3 sm:h-3 rounded-full transition-colors ${
                                                index === currentSlide ? "bg-white" : "bg-white/40"
                                            }`}
                                        />
                                    ))}
                                </div>

                                <button
                                    onClick={nextSlide}
                                    className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30 transition-colors"
                                >
                                    <ChevronRight size={20} className="text-white sm:w-6 sm:h-6" />
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}