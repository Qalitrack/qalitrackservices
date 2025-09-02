import { lazy } from "react";

// Lazy load all pages for code splitting
const MainLayout = lazy(() => import("../layouts/MainLayout.jsx"));
const Dashboard = lazy(() => import("../pages/weighing/Dashboard.jsx"));
const FactoryWeighing = lazy(() => import("../pages/weighing/Weighing.jsx"));
const Automation = lazy(() => import("../pages/weighing/Automation.jsx"));
const Calibrations = lazy(() => import("../pages/weighing/Calibrations.jsx"));
const Analytics = lazy(() => import("../pages/weighing/Analytics.jsx"));
const Reports = lazy(() => import("../pages/weighing/Reports.jsx"));
const System = lazy(() => import("../pages/weighing/System.jsx"));
const Vehicle = lazy(() => import("../components/weighing/Vehicles.jsx"));
const Drivers = lazy(() => import("../components/weighing/Drivers.jsx"));
const Login = lazy(() => import("../pages/userservice/Login.jsx"));
 
//user services


export const routes = [
  // Authentication route
  { path: "/login", component: Login },

  // Main application routes, nested under a layout component
  {
    path: "/",
    component: MainLayout,
    children: [
      { path: "", component: Dashboard },
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
];
