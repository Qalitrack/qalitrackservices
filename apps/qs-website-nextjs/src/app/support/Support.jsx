import React from 'react';
export default function Support() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-4">Support</h1>
      <p className="text-gray-700 mb-4">
        Need help? Our support team is here for you. Use the channels below to reach out for assistance.
      </p>
      <ul className="list-disc pl-5 space-y-2 text-gray-700">
        <li>Email us at <a href="mailto:support@qalibrated.co.ke" className="text-amber-500 underline">support@qalibrated.co.ke</a></li>
        <li>Call our support line at <strong>+254-714-999996</strong> (Mon–Fri, 8:00 AM–5:00 PM EAT)</li>
        <li>Browse our <a href="/faq" className="text-amber-500 underline">FAQ</a> page for quick answers</li>
        <li>Use the live chat button on our platform for real-time assistance during business hours</li>
        <li>Join our community forum to share experiences, ask questions, and learn from others</li>
      </ul>
    </div>
  );
}