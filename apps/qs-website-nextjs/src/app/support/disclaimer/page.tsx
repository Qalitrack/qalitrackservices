import React from 'react';

export default function Disclaimer() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-4">Disclaimer</h1>
      <p className="text-gray-700 mb-4">
        The information provided on this website is for general informational purposes only.
        Qalibrated Systems Limited makes no warranties about the accuracy or reliability of the content.
      </p>
      <ul className="list-disc pl-5 space-y-2 text-gray-700">
        <li>
          All content is provided &quot;as is&quot; without warranty of any kind, expressed or implied.
        </li>
        <li>
          We are not liable for any losses or damages resulting from reliance on the content of this site.
        </li>
        <li>
          Links to external sites are provided for convenience and do not constitute endorsement.
        </li>
        <li>
          We do not guarantee uninterrupted access to the platform due to maintenance or third-party outages.
        </li>
      </ul>
    </div>
  );
}

