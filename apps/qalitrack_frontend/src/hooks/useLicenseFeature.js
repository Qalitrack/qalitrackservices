import { useState, useEffect } from "react";
import { getLicenseStatus } from "../utils/licenseUtils";

/**
 * useLicenseFeature(feature)
 *
 * Returns true if the active license includes the given feature flag.
 * Returns false while loading or if not licensed.
 *
 * Use this for inline elements (e.g. hiding a field or button) where
 * a full FeatureLicenseGate lock placeholder would break the layout.
 *
 * Example:
 *   const rfidLicensed = useLicenseFeature(LicenseFeatures.RFID);
 *   {rfidLicensed && <RfidSearchBar />}
 */
export function useLicenseFeature(feature) {
  const [licensed, setLicensed] = useState(false);

  useEffect(() => {
    getLicenseStatus(feature).then(s => setLicensed(s.valid));
  }, [feature]);

  return licensed;
}
