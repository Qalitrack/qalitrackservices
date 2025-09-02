import React, { useState } from "react";
import { Eye, EyeOff, ChevronLeft, ChevronRight } from "lucide-react";
import { useNavigate } from "react-router-dom"; // 👈 import navigation hook

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [currentSlide, setCurrentSlide] = useState(0);
  const [show2FA, setShow2FA] = useState(false);
  const [verificationCode, setVerificationCode] = useState("");
  const [maskedEmail, setMaskedEmail] = useState("");
  const [twoFAError, setTwoFAError] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const navigate = useNavigate(); // 👈 hook to redirect

  const slides = [
    {
      title: "Unlock a world of Weighing and Construction opportunities",
      subtitle:
        "Access your dashboard to manage bids, track tender submissions, and connect with potential clients.",
      description:
        "Upload your documents and bid for tenders that match your expertise to impress potential clients.",
    },
    {
      title: "Streamline Your Construction Projects",
      subtitle:
        "Manage all your weighing and construction projects from one centralized platform.",
      description:
        "Track progress, monitor deadlines, and collaborate with your team effectively.",
    },
    {
      title: "Connect with Industry Leaders",
      subtitle:
        "Network with top construction professionals and expand your business reach.",
      description:
        "Build lasting relationships and discover new partnership opportunities.",
    },
  ];

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    await new Promise((resolve) => setTimeout(resolve, 1000));

    if (!email || !password) {
      setError("Please enter both email and password.");
      setLoading(false);
      return;
    }

    if (email === "Daphne Smith" && password === "correctpassword") {
      setMaskedEmail("d*******s@example.com");
      setShow2FA(true);
    } else if (email === "testuser" && password === "testpassword") {
      navigate("/dashboard"); // 👈 redirect directly to dashboard
    } else {
      setError("Invalid email or password. Please try again.");
    }

    setLoading(false);
  };

  const handle2FASubmit = async (e) => {
    e.preventDefault();
    setTwoFAError("");
    setLoading(true);

    await new Promise((resolve) => setTimeout(resolve, 1000));

    if (!verificationCode) {
      setTwoFAError("Please enter the verification code.");
      setLoading(false);
      return;
    }

    if (verificationCode === "123456") {
      navigate("/dashboard"); // 👈 redirect after successful 2FA
    } else {
      setTwoFAError("The verification code is invalid. Please try again.");
    }

    setLoading(false);
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
                {show2FA ? "Enter Verification Code" : "Welcome Back 👋"}
              </h2>
              {show2FA && (
                <p className="text-sm text-gray-600">
                  We've sent a verification code to {maskedEmail}
                </p>
              )}
            </div>

            {(error || twoFAError) && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
                {twoFAError || error}
              </div>
            )}

            {!show2FA ? (
              <div className="space-y-4 sm:space-y-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Email Address
                  </label>
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Password
                  </label>
                  <div className="relative">
                    <input
                      type={showPassword ? "text" : "password"}
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 pr-10"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500"
                    >
                      {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                  </div>
                </div>

                <button
                  type="button"
                  onClick={handleSubmit}
                  disabled={loading}
                  className="w-full py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600"
                >
                  {loading ? "Logging in..." : "Login"}
                </button>
              </div>
            ) : (
              <div className="space-y-4 sm:space-y-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Verification Code
                  </label>
                  <input
                    type="text"
                    value={verificationCode}
                    onChange={(e) => setVerificationCode(e.target.value)}
                    maxLength="6"
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 text-center text-2xl tracking-widest font-mono"
                  />
                </div>

                <button
                  type="button"
                  onClick={handle2FASubmit}
                  disabled={loading || !verificationCode}
                  className="w-full py-2.5 px-4 rounded-lg font-medium bg-amber-500 text-white hover:bg-amber-600"
                >
                  {loading ? "Verifying..." : "Verify Code"}
                </button>

                <button
                  type="button"
                  onClick={() => {
                    setShow2FA(false);
                    setVerificationCode("");
                    setError("");
                    setTwoFAError("");
                  }}
                  className="w-full py-2 px-4 text-sm text-gray-600 hover:text-gray-800"
                >
                  ← Back to Login
                </button>
              </div>
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
                  className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30"
                >
                  <ChevronLeft size={20} className="text-white" />
                </button>

                <div className="flex space-x-1.5 sm:space-x-2">
                  {slides.map((_, index) => (
                    <button
                      key={index}
                      onClick={() => setCurrentSlide(index)}
                      className={`w-2.5 h-2.5 rounded-full ${
                        index === currentSlide ? "bg-white" : "bg-white/40"
                      }`}
                    />
                  ))}
                </div>

                <button
                  onClick={nextSlide}
                  className="p-1.5 sm:p-2 rounded-full bg-white/20 hover:bg-white/30"
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
