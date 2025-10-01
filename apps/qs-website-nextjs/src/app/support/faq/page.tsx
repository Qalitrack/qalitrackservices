"use client";
import React, { useState } from "react";

const faqs = [
  {
    question: "How do I register as a supplier?",
    answer:
      "Click on “Register” in the navigation bar and select the supplier option. Fill in the required details, verify your email, and submit the registration form.",
  },
  {
    question: "Is there a subscription fee?",
    answer:
      "Basic features are free. Premium tools and analytics may require a subscription, which is billed monthly or annually.",
  },
  {
    question: "Can I track my tender submissions?",
    answer:
      "Yes. Log in and go to your dashboard to view your submitted tenders, bid status, and performance analytics.",
  },
  {
    question: "What file formats are accepted for bid documents?",
    answer:
      "We support PDF, DOCX, XLSX, and ZIP formats. Make sure files are under 20MB each.",
  },
  {
    question: "How secure is my data?",
    answer:
      "We use encryption, two-factor authentication, and secure cloud infrastructure to protect your information.",
  },
  {
    question: "Who do I contact for technical support?",
    answer:
      "You can reach our support team via the Contact page or email us at support@qalibrated.co.ke.",
  },
  {
    question: "Can I request a demo?",
    answer:
      "Yes! Simply head to the Contact page and request a demo. Our team will reach out to schedule it.",
  },
];

export default function FAQ () {
  const [openIndex, setOpenIndex] = useState<number | null>(null);
  const [search, setSearch] = useState("");

  const filteredFaqs = faqs.filter((faq) =>
    faq.question.toLowerCase().includes(search.toLowerCase())
  );

  const toggle = (index: number) => {
    setOpenIndex(openIndex === index ? null : index);
  };

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6 text-center">Frequently Asked Questions</h1>

      {/* Search */}
      <input
        type="text"
        placeholder="Search a question..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="w-full p-3 mb-6 border rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
      />

      <div className="space-y-4">
        {filteredFaqs.length > 0 ? (
          filteredFaqs.map((faq, index) => (
            <div
              key={index}
              className="border border-gray-200 rounded-lg shadow-sm overflow-hidden"
            >
              <button
                onClick={() => toggle(index)}
                className="w-full text-left px-4 py-3 font-semibold flex justify-between items-center bg-gray-50 hover:bg-amber-50"
              >
                {faq.question}
                <span className="ml-2">{openIndex === index ? "−" : "+"}</span>
              </button>
              {openIndex === index && (
                <div className="px-4 py-3 text-gray-700 bg-white">
                  {faq.answer}
                </div>
              )}
            </div>
          ))
        ) : (
          <p className="text-gray-500">No questions found.</p>
        )}
      </div>
    </div>
  );
};
