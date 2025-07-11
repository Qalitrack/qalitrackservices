import React, { useState } from 'react';
import { Input } from '../inputs';
import { Button } from '../button';

const Login: React.FC = () => {
  const [form, setForm] = useState({ email: '', password: '' });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  return (
    <div className="min-h-screen flex">
      <div className="w-1/2 bg-white p-10 flex flex-col justify-center">
        <h1 className="text-3xl font-bold mb-2">Qualitrack</h1>
        <h2 className="text-xl font-semibold mb-6">Welcome Back<span role="img">👋</span></h2>
        <Input label="Email Address" name="email" type="email" placeholder="Daphne Smith" value={form.email} onChange={handleChange} />
        <Input label="Password" name="password" type="password" placeholder="********" value={form.password} onChange={handleChange} />
        <Button text="Login" type="submit" />
        <p className="mt-4 text-sm">Don't have an account? <a href="/signup" className="text-yellow-500 font-semibold">Sign up</a></p>
      </div>
      <div className="w-1/2 bg-yellow-400 text-white flex flex-col justify-center p-12">
        <h2 className="text-2xl font-bold mb-4">Unlock a world of procurement opportunities</h2>
        <p className="mb-6">Access your dashboard to manage bids, track tender submissions, and connect with potential clients</p>
        <div className="bg-white w-full h-64 rounded-xl"></div>
      </div>
    </div>
  );
};

export default Login;