"use client";

import { useEffect, useState } from "react";
import { X } from "lucide-react";

export default function AIChatWidget() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<{ role: string; content: string }[]>(
    []
  );
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);

  // ✅ Load messages from localStorage on mount
  useEffect(() => {
    const storedMessages = localStorage.getItem("chat_messages");
    if (storedMessages) {
      setMessages(JSON.parse(storedMessages));
    }
  }, []);

  // ✅ Save messages whenever they change
  useEffect(() => {
    if (messages.length > 0) {
      localStorage.setItem("chat_messages", JSON.stringify(messages));
    }
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const newMessages = [...messages, { role: "user", content: input }];
    setMessages(newMessages);
    setInput("");
    setLoading(true);

    try {
      const res = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ messages: newMessages }),
      });

      const data = await res.json();
      const reply =
        data?.choices?.[0]?.message?.content ||
        "Sorry, I didn’t understand that.";

      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: reply },
      ]);
    } catch (err) {
      console.error(err);
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: "⚠️ Something went wrong. Try again." },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  return (
    <div className="fixed bottom-4 right-4 z-50">
      {/* Floating button */}
      {!isOpen && (
        <button
          onClick={() => setIsOpen(true)}
          className="bg-amber-500 text-white px-6 py-2 rounded-full shadow-lg hover:bg-amber
          -600 transition"
        >
          💬 Hi Friend
        </button>
      )}

      {/* Chat window */}
      {isOpen && (
        <div className="w-80 h-96 bg-white shadow-2xl rounded-2xl flex flex-col border border-gray-200 overflow-hidden">
          {/* Header */}
          <div className="bg-amber-500 text-white px-4 py-2 font-semibold flex justify-between items-center">
            <span>Qalibrated AI Assistant</span>
            <button
              onClick={() => setIsOpen(false)}
              className="hover:bg-amber-600 p-1 rounded transition"
              aria-label="Close chat"
            >
              <X size={18} />
            </button>
          </div>

          {/* Messages */}
          <div className="flex-1 p-3 space-y-2 overflow-y-auto text-sm">
            {messages.map((m, i) => (
              <div
                key={i}
                className={`max-w-[75%] px-3 py-2 rounded-lg ${
                  m.role === "user"
                    ? "ml-auto bg-amber-100 text-gray-800"
                    : "mr-auto bg-gray-100 text-gray-800"
                }`}
              >
                {m.content}
              </div>
            ))}
            {loading && (
              <p className="text-gray-400 italic text-sm">Thinking...</p>
            )}
          </div>

          {/* Input */}
          <div className="p-2 border-t flex items-center">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={handleKeyDown}
              className="flex-1 px-3 py-2 text-sm border rounded-l-lg focus:outline-none focus:ring-2 focus:ring-amber
              -400"
              placeholder="Type a message..."
            />
            <button
              onClick={sendMessage}
              disabled={loading}
              className="bg-amber-500 text-white px-4 py-2 rounded-r-lg hover:bg-amber
              -600 disabled:opacity-50"
            >
              Send
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
