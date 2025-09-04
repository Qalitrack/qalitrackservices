import { lazy } from "react";
import { Navigate } from "react-router-dom";

// Layout
const MainLayout = lazy(() => import("../layouts/MainLayout.jsx"));

// Auth
const Login = lazy(() => import("../pages/auth/Login.jsx"));

// Admin
const AdminDashboard = lazy(() => import("../components/user/AdminDashboard.jsx"));

// Operator
const Dashboard = lazy(() => import("../pages/weighing/Dashboard.jsx"));
const FactoryWeighing = lazy(() => import("../pages/weighing/Weighing.jsx"));
const Automation = lazy(() => import("../pages/weighing/Automation.jsx"));
const Calibrations = lazy(() => import("../pages/weighing/Calibrations.jsx"));
const Analytics = lazy(() => import("../pages/weighing/Analytics.jsx"));
const Reports = lazy(() => import("../pages/weighing/Reports.jsx"));
const System = lazy(() => import("../pages/weighing/System.jsx"));
const Vehicle = lazy(() => import("../components/weighing/Vehicles.jsx"));
const Drivers = lazy(() => import("../components/weighing/Drivers.jsx"));

export const routes = [
  // Redirect root ("/") → login
  { path: "/", component: () => <Navigate to="/admin" replace /> },

  // Login
  { path: "/login", component: Login },

  // Admin routes
  // {
  //   path: "/admin",
  //   component: MainLayout,
  //   children: [
  //     { index: true, component: AdminDashboard },
  //     { path: "operator", component: Dashboard },
  //     { path: "operator/weighing/factory", component: FactoryWeighing },
  //     { path: "operator/weighing/vehicle", component: Vehicle },
  //     { path: "operator/weighing/drivers", component: Drivers },
  //     { path: "operator/automation", component: Automation },
  //     { path: "operator/calibrations", component: Calibrations },
  //     { path: "operator/analytics", component: Analytics },
  //     { path: "operator/reports", component: Reports },
  //     { path: "operator/system", component: System },
  //   ],
  // },

  // Operator routes
  {
    path: "/admin",
    component: MainLayout,
    children: [
       { index: true, component: AdminDashboard },
      { index: true, component: Dashboard },
      { path: "weighing/factory", component: FactoryWeighing },
      { path: "weighing/vehicle", component: Vehicle },
      { path: "weighing/drivers", component: Drivers },
      { path: "automation", component: Automation },
      { path: "calibrations", component: Calibrations },
      { path: "analytics", component: Analytics },
      { path: "reports", component: Reports },
      { path: "system", component: System },
    ],
  },
    {
    path: "/operator",
    component: MainLayout,
    children: [
      { index: true, component: Dashboard },
      { path: "weighing/factory", component: FactoryWeighing },
      { path: "weighing/vehicle", component: Vehicle },
      { path: "weighing/drivers", component: Drivers },
      { path: "automation", component: Automation },
      { path: "calibrations", component: Calibrations },
      { path: "analytics", component: Analytics },
      { path: "reports", component: Reports },
      { path: "system", component: System },
    ],
  },

  // 404
  { path: "*", component: () => <div>404 - Page Not Found</div> },
];
