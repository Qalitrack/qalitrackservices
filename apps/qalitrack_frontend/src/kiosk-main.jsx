import React from "react";
import ReactDOM from "react-dom/client";
import SelfServiceWeighing from "./pages/SelfServiceWeighing.jsx";
import AppLicenseGate from "./components/AppLicenseGate.jsx";
import "./index.css";
import "antd/dist/reset.css";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AppLicenseGate>
      <SelfServiceWeighing />
    </AppLicenseGate>
  </React.StrictMode>
);
