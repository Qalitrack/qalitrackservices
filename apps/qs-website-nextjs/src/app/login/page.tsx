'use client';

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Eye, EyeOff } from "lucide-react";
import type { AxiosError } from "axios";
import { authAPI } from "@/utils/apiFactory";

export default function Login() {
  const router = useRouter();
  const [formData, setFormData] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await authAPI.login(formData);

      if (res.data.token) {
        localStorage.setItem("authToken", res.data.token);
        router.push("/dashboard");
      } else {
        setError("Login failed: No token received.");
      }
    } catch (error: unknown) {
      const err = error as AxiosError<{ message: string }>;

      if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError("Server error during login.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-amber-400 to-amber-600">
      <div className="bg-white/90 backdrop-blur-md p-8 rounded-2xl shadow-lg w-full max-w-sm relative">
        {/* Back Button */}
        <button
          onClick={() => router.push("/")}
          className="absolute top-4 left-4 flex items-center text-theme-600 hover:text-theme-700"
        >
          <ArrowLeft className="w-5 h-5 mr-1" />
          Back
        </button>

        {/* Title */}
        <h2 className="text-3xl font-bold text-center text-theme-600 mb-6">
          Login
        </h2>

        {error && (
          <p className="text-red-500 text-center font-medium mb-4">{error}</p>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Email */}
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-theme-500 focus:outline-none"
            />
          </div>

          {/* Password with toggle */}
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700">
              Password
            </label>
            <div className="relative">
              <input
                type={showPassword ? "text" : "password"}
                name="password"
                value={formData.password}
                onChange={handleChange}
                required
                className="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-theme-500 focus:outline-none pr-10"
              />
              <button
                type="button"
                onClick={() => setShowPassword((prev) => !prev)}
                className="absolute inset-y-0 right-3 flex items-center text-gray-500 hover:text-gray-700"
              >
                {showPassword ? (
                  <EyeOff className="w-5 h-5" />
                ) : (
                  <Eye className="w-5 h-5" />
                )}
              </button>
            </div>
          </div>

          {/* Submit */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-amber-500 text-black py-2 px-4 rounded-lg shadow-md hover:bg-amber-600 transition"
          >
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>

        {/* Sign Up */}
        <p className="mt-6 text-center text-sm text-gray-700">
          Don&apos;t have an account?{" "}
          <Link href="/signup" className="text-theme-500 hover:underline">
            Sign up
          </Link>
        </p>
      </div>
    </div>
  );
}
