import React from 'react';

export default function PrivacyPolicy() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-4">Privacy Policy</h1>
      <p className="text-gray-700 mb-4">
        At Qalibrated Systems Limited, your privacy is important to us. This Privacy Policy explains how we collect,
        use, and protect your information when you use our services.
      </p>
      <ul className="list-disc pl-5 space-y-2 text-gray-700">
        <li>
          We collect personal data such as your name, email, and company details when you register or interact with our platform.
        </li>
        <li>
          We use cookies and analytics to enhance user experience and monitor platform performance.
        </li>
        <li>
          Your data is stored securely using industry-standard encryption and only accessible to authorized personnel.
        </li>
        <li>
          We do not sell your data to third parties. We may share it only with verified partners for service delivery.
        </li>
        <li>
          You can request deletion, correction, or review of your personal data anytime by contacting our support team.
        </li>
        <li>
          We comply with applicable data protection laws, including Kenya&apos;s Data Protection Act and international standards.
        </li>
      </ul>
    </div>
  );
}
