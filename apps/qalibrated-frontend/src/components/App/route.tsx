import React, { lazy, Suspense } from "react";
import { Routes, Route } from "react-router-dom";

const Login = lazy(() => import("../pages/auth/login"));
// const Signup = lazy(() => import("../pages/auth/signup"));

const AuthRoutes: React.FC = () => {
  return (
    <Suspense fallback={<div className="p-6 text-center">Loading...</div>}>
      <Routes>
        <Route path="/login" element={<Login />} />
        {/* <Route path="/signup" element={<Signup />} /> */}
      </Routes>
    </Suspense>
  );
};

export default AuthRoutes;
