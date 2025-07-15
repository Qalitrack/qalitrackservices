import React from 'react';

interface ButtonProps {
  text: string;
  onClick?: () => void;
  type?: "button" | "submit";
}

export const Button: React.FC<ButtonProps> = ({ text, onClick, type = 'button' }) => (
  <button
    onClick={onClick}
    type={type}
    className="w-full bg-yellow-400 hover:bg-yellow-500 text-white font-semibold py-2 px-4 rounded-md shadow"
  >
    {text}
  </button>
);