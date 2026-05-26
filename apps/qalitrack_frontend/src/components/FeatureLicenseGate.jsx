import { useState, useEffect } from "react";
import { getLicenseStatus, licenseErrorMessage } from "../utils/licenseUtils";
import { LicenseFeatures } from "../utils/LicenseFeatures";

/**
 * FeatureLicenseGate — wraps any module that requires a specific feature flag.
 *
 * Always pass a LicenseFeatures constant — never a bare string — to prevent typos:
 *
 *   import { LicenseFeatures } from "../utils/LicenseFeatures";
 *
 *   <FeatureLicenseGate feature={LicenseFeatures.ANPR}>
 *     <AnprSettings />
 *   </FeatureLicenseGate>
 *
 * AppLicenseGate (app-level) must already have passed before this renders.
 * This gate only checks whether the active license includes this specific feature.
 */
export default function FeatureLicenseGate({ feature, children }) {
  const [status, setStatus] = useState(null);

  // Dev-time guard — catch unknown feature strings before they reach production
  if (import.meta.env.DEV && !LicenseFeatures.isValid(feature)) {
    console.error(
      `[FeatureLicenseGate] Unknown feature "${feature}". ` +
      `Use a LicenseFeatures constant. Valid values: ${LicenseFeatures.ALL.map(f => f.value).join(", ")}`
    );
  }

  // Human-readable label for the locked UI (falls back to the raw value)
  const featureLabel = LicenseFeatures.ALL.find(f => f.value === feature)?.label ?? feature;

  useEffect(() => {
    getLicenseStatus(feature).then(setStatus);
  }, [feature]);

  if (status === null) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="w-7 h-7 border-4 border-amber-500 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (status.valid) return children;

  return (
    <div className="flex items-center justify-center py-16">
      <div className="rounded-2xl border-2 border-dashed border-amber-200 bg-amber-50 p-10 max-w-sm w-full text-center">
        <div className="flex justify-center mb-4">
          <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-lg">
            <LockIcon className="w-7 h-7 text-white" />
          </div>
        </div>
        <h2 className="text-base font-black text-gray-900 mb-1">
          {featureLabel} — Not Licensed
        </h2>
        <p className="text-xs text-gray-500 mb-4">{licenseErrorMessage(status.reason)}</p>
        <p className="text-xs text-gray-400">
          Contact{" "}
          <span className="text-amber-600 font-medium">info@qalibrated.co.ke</span>{" "}
          to add this module to your license.
        </p>
      </div>
    </div>
  );
}

function LockIcon({ className }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
    </svg>
  );
}
