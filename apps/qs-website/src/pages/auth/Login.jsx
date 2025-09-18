import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate, Link } from "react-router-dom";
import { motion } from "framer-motion";
import { API_BASE_URL } from "../../utils/config";
import { ArrowLeft } from "lucide-react";

const Login = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Redirect if already logged in
  useEffect(() => {
    const token = localStorage.getItem("authToken");
    if (token) navigate("/dashboard");
  }, [navigate]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await axios.post(`${API_BASE_URL}/auth/login`, formData);

      if (res.data.token) {
        localStorage.setItem("authToken", res.data.token);
        navigate("/dashboard");
      } else {
        setError("Login failed: No token received.");
      }
    } catch (err) {
      if (err.response?.status === 401) {
        setError("Invalid email or password.");
      } else if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError("Unexpected server error. Please try again later.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-amber-400 via-orange-500 to-red-500">
      <motion.div
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6 }}
        className="bg-white/90 backdrop-blur-lg p-8 rounded-2xl shadow-2xl w-full max-w-md"
      >
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">
          Welcome Back 👋
        </h2>

        {error && (
          <motion.p
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-red-600 mb-4 text-center font-medium"
          >
            {error}
          </motion.p>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label
              htmlFor="email"
              className="block mb-1 font-medium text-gray-700"
            >
              Email Address
            </label>
            <input
              id="email"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-amber-500 focus:outline-none"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label
              htmlFor="password"
              className="block mb-1 font-medium text-gray-700"
            >
              Password
            </label>
            <input
              id="password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
              className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-amber-500 focus:outline-none"
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            disabled={loading || !formData.email || !formData.password}
            className={`w-full py-3 px-4 rounded-lg text-white font-semibold transition-colors ${
              loading || !formData.email || !formData.password
                ? "bg-gray-400 cursor-not-allowed"
                : "bg-amber-500 hover:bg-amber-600"
            }`}
          >
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>

        <div className="mt-6 text-center">
          {/* <p className="text-sm text-gray-600">
            Don’t have an account?{" "}
            <Link to="/signup" className="text-amber-600 hover:underline">
              Sign up
            </Link>
          </p> */}
        </div>

        {/* Go back home button */}
        <div className="mt-6 flex justify-center">
          <Link
            to="/"
            className="flex items-center gap-2 text-gray-600 hover:text-amber-600 transition-colors"
          >
            <ArrowLeft size={18} /> Go Back Home
          </Link>
        </div>
      </motion.div>
    </div>
  );
};

export default Login;
