
import { useState } from 'react';

function FAQAccordion() {
  const faqs = [
    { q: "Can I request a custom weighbridge size?", a: "Yes, we build custom systems tailored to your site's requirements." },
    { q: "Do you offer support across Kenya?", a: "Yes. Our team is based in Nairobi, Mombasa and Kisumu but supports all counties." },
    { q: "Are your systems legal-for-trade certified?", a: "Yes, we are approved by the Weights and Measures Department." },
    { q: "How fast is your installation process?", a: "Most systems can be deployed within 3–7 working days depending on type." },
  ];

  const [openIndex, setOpenIndex] = useState(null);

  return (
    <section className="bg-white py-16">
      <div className="max-w-4xl mx-auto px-6">
        <h2 className="text-3xl font-semibold text-center text-gray-800 mb-10">Frequently Asked Questions</h2>
        <div className="space-y-4">
          {faqs.map((item, index) => (
            <div key={index} className="border rounded-lg overflow-hidden">
              <button
                className="w-full px-6 py-4 text-left font-medium text-black bg-gray-100 hover:bg-gray-200 focus:outline-none"
                onClick={() => setOpenIndex(openIndex === index ? null : index)}
              >
                {item.q}
              </button>
              {openIndex === index && (
                <div className="px-6 py-4 text-gray-700 bg-white border-t">{item.a}</div>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

export default FAQAccordion;
