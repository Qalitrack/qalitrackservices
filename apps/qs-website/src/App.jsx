import React, { useEffect } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { Link } from "react-router-dom";

// Import routes
import routes from "./App/routes";

// Cookie consent
import CookieConsent from "react-cookie-consent";
import Cookies from "js-cookie";
import loadScript from "./utils/LoadScript";

function App() {
  useEffect(() => {
    const consent = Cookies.get("qalibrated_cookie_consent");
    if (consent === "true") {
      enableAnalytics();
    }
  }, []);

  const enableAnalytics = () => {
    // Example: Google Analytics 4 (replace with your Measurement ID)
    loadScript(
      "https://www.googletagmanager.com/gtag/js?id=G-XXXXXXX",
      "ga-script"
    );

    window.dataLayer = window.dataLayer || [];
    function gtag() {
      window.dataLayer.push(arguments);
    }
    gtag("js", new Date());
    gtag("config", "G-XXXXXXX"); // Replace with your GA ID
  };

  return (
    <Router>
      {/* Navbar */}
      {/* <Navbar /> */}

      {/* Routes */}
      <Routes>
        {routes.map((route, index) => (
          <Route key={index} path={route.path} element={route.element} />
        ))}
      </Routes>

      {/* Footer */}
      {/* <Footer /> */}

      {/* Cookie Consent */}
      <CookieConsent
        location="bottom"
        buttonText="Accept"
        declineButtonText="Decline"
        enableDeclineButton
        cookieName="qalibrated_cookie_consent"
        style={{ background: "#2B373B", fontSize: "16px" }}
        buttonStyle={{
          background: "#f59e0b",
          color: "#fff",
          fontSize: "14px",
          borderRadius: "8px",
        }}
        declineButtonStyle={{
          background: "#ccc",
          color: "#000",
          fontSize: "13px",
          borderRadius: "6px",
        }}
        expires={150}
        onAccept={() => enableAnalytics()} // 🔑 Load GA only after accept
      >
        We use cookies to improve your browsing experience. By clicking{" "}
        <strong>Accept</strong>, you agree to our cookie policy.{" "}
        <Link
  to="/support/privacy"
  className="bg-amber-400 text-black px-6 py-3 rounded-full font-medium hover:opacity-90 transition inline-block"
>
  Learn More
</Link>
      </CookieConsent>
    </Router>
  );
}

export default App;
