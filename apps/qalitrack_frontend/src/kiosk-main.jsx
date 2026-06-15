import React from "react";
import ReactDOM from "react-dom/client";
import SelfServiceWeighing from "./pages/SelfServiceWeighing.jsx";
import AppLicenseGate from "./components/AppLicenseGate.jsx";
import { LicenseFeatures } from "./utils/LicenseFeatures.js";
import "./index.css";
import "antd/dist/reset.css";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AppLicenseGate feature={LicenseFeatures.KIOSK}>
      <SelfServiceWeighing />
    </AppLicenseGate>
  </React.StrictMode>
);
