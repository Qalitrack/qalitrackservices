import React, { useState } from 'react';
import { Input } from '../inputs';
import { Button } from '../button';

export const Signup: React.FC = () => {
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
    taxId: '',
    address: ''
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  return (
    <div className="min-h-screen flex">
      <div className="w-1/2 bg-white p-10 flex flex-col justify-center">
        <h1 className="text-3xl font-bold mb-4 text-yellow-500">Qualitrack</h1>
        <h2 className="text-xl font-semibold mb-6">Create your account with us below</h2>
        <Input label="Username" name="username" type="text" placeholder="Finestock Constructions" value={form.username} onChange={handleChange} />
        <Input label="Email Address" name="email" type="email" placeholder="Enter your email address" value={form.email} onChange={handleChange} />
        <Input label="Password" name="password" type="password" placeholder="Create your password" value={form.password} onChange={handleChange} />
        <Input label="Confirm Password" name="confirmPassword" type="password" placeholder="Create your password" value={form.confirmPassword} onChange={handleChange} />
        <Input label="Phone Number" name="phone" type="tel" placeholder="+254..." value={form.phone} onChange={handleChange} />
        <Input label="Tax Identification Number" name="taxId" type="text" placeholder="A05299480N" value={form.taxId} onChange={handleChange} />
        <Input label="Postal Address" name="address" type="text" placeholder="Westlands, Ojijo road" value={form.address} onChange={handleChange} />
        <Button text="Create Account" type="submit" />
        <p className="mt-4 text-sm">Already have an account? <a href="/login" className="text-yellow-500 font-semibold">Login</a></p>
      </div>
      <div className="w-1/2 bg-yellow-400 text-white flex flex-col justify-center p-12">
        <h2 className="text-2xl font-bold mb-4">Inventing and Making Happen</h2>
        <p className="mb-4 text-sm">At Qualitrack Systems Limited, we specialize in delivering innovative, high-quality solutions...</p>
        <div className="bg-white w-full h-64 rounded-xl"></div> {/* Placeholder for illustration */}
        <div className="mt-4 p-4 bg-white rounded-xl text-black">
          <p className="font-semibold">Amelia Hendrick</p>
          <p className="text-xs">Chief Manager</p>
          <p className="mt-1 text-xs">...next opportunity awaits!</p>
        </div>
      </div>
    </div>
  );
};
