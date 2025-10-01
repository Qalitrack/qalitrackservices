import React from 'react';  
export default function TermsConditions() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-4">Terms and Conditions</h1>
      <p className="text-gray-700 mb-4">
        These Terms and Conditions govern your use of Qalibrated Systems Limited’s platform. By accessing or using our services, you agree to comply with these terms.
      </p>
      <ul className="list-decimal pl-5 space-y-2 text-gray-700">
        <li>You must be 18 years or older to use our services or have parental consent.</li>
        <li>You are responsible for the accuracy of information you provide and must keep your credentials secure.</li>
        <li>Unauthorized or fraudulent use of our platform may result in account suspension or legal action.</li>
        <li>We reserve the right to update these terms at any time. Continued use implies acceptance of changes.</li>
        <li>All transactions on the platform are subject to Kenyan commercial law and international procurement principles.</li>
        <li>We are not liable for procurement losses due to misinterpretation of tender requirements.</li>
      </ul>
    </div>
  );
}