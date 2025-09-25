'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'

export default function LandingPage() {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isScrolled, setIsScrolled] = useState(false)

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 100)
    }

    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  return (
    <div className="min-h-screen">
      {/* Navigation */}
      <nav className={`fixed top-0 w-full z-50 transition-all duration-300 ${
        isScrolled ? 'glass shadow-lg' : 'bg-white/95 backdrop-blur-sm'
      }`}>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-2">
              <span className="text-3xl">🚛</span>
              <span className="text-2xl font-bold text-amber-500">QTruck</span>
            </div>
            
            <div className="hidden md:flex items-center space-x-8">
              <a href="#features" className="text-gray-700 hover:text-amber-500 transition-colors">Features</a>
              <a href="#solutions" className="text-gray-700 hover:text-amber-500 transition-colors">Solutions</a>
              <a href="#pricing" className="text-gray-700 hover:text-amber-500 transition-colors">Pricing</a>
              <a href="#contact" className="text-gray-700 hover:text-amber-500 transition-colors">Contact</a>
              <Link href="/auth/login" className="bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
                Sign In
              </Link>
              <Link href="/auth/register" className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
                Get Started
              </Link>
            </div>

            <button 
              className="md:hidden"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
            >
              <div className="space-y-1">
                <div className={`w-6 h-0.5 bg-gray-600 transition-all ${isMenuOpen ? 'rotate-45 translate-y-1.5' : ''}`}></div>
                <div className={`w-6 h-0.5 bg-gray-600 transition-all ${isMenuOpen ? 'opacity-0' : ''}`}></div>
                <div className={`w-6 h-0.5 bg-gray-600 transition-all ${isMenuOpen ? '-rotate-45 -translate-y-1.5' : ''}`}></div>
              </div>
            </button>
          </div>

          {/* Mobile Menu */}
          {isMenuOpen && (
            <div className="md:hidden bg-white border-t border-gray-200">
              <div className="px-2 pt-2 pb-3 space-y-1">
                <a href="#features" className="block px-3 py-2 text-gray-700">Features</a>
                <a href="#solutions" className="block px-3 py-2 text-gray-700">Solutions</a>
                <a href="#pricing" className="block px-3 py-2 text-gray-700">Pricing</a>
                <a href="#contact" className="block px-3 py-2 text-gray-700">Contact</a>
                <div className="pt-2 space-y-2">
                  <Link href="/auth/login" className="bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full text-center">Sign In</Link>
                  <Link href="/auth/register" className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full text-center">Get Started</Link>
                </div>
              </div>
            </div>
          )}
        </div>
      </nav>

      {/* Hero Section */}
      <section className="pt-16 min-h-screen bg-gradient-to-br from-amber-50 via-white to-green-50 relative overflow-hidden">
        <div className="absolute top-0 right-0 w-1/2 h-full bg-gradient-to-l from-amber-100/30 to-transparent animate-float"></div>
        
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div className="space-y-8">
              <h1 className="text-5xl lg:text-6xl font-bold leading-tight">
                Streamline Your Fleet 
                <span className="bg-gradient-to-r from-amber-500 to-orange-500 bg-clip-text text-transparent"> Management</span>
              </h1>
              
              <p className="text-xl text-gray-600 leading-relaxed">
                Complete fleet management solution for Kenyan logistics companies. 
                Track vehicles, manage drivers, optimize routes across Kenya, and boost efficiency.
              </p>
              
              <div className="flex flex-col sm:flex-row gap-4">
                <Link href="/auth/register" className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white font-medium text-lg px-8 py-4 rounded-lg transition-all duration-200">
                  Get Started
                </Link>
                <a href="#features" className="bg-gradient-to-r from-gray-100 to-gray-200 hover:from-gray-200 hover:to-gray-300 text-gray-800 font-medium text-lg px-8 py-4 rounded-lg transition-all duration-200 border border-gray-300">
                  Learn More
                </a>
              </div>
              
              <div className="grid grid-cols-3 gap-8 pt-8">
                <div className="text-center">
                  <div className="text-3xl font-bold text-amber-500">500+</div>
                  <div className="text-gray-600">Vehicles Managed</div>
                </div>
                <div className="text-center">
                  <div className="text-3xl font-bold text-amber-500">1M+</div>
                  <div className="text-gray-600">Miles Tracked</div>
                </div>
                <div className="text-center">
                  <div className="text-3xl font-bold text-amber-500">99.9%</div>
                  <div className="text-gray-600">Uptime</div>
                </div>
              </div>
            </div>
            
            <div className="relative">
              <div className="bg-white rounded-2xl shadow-2xl overflow-hidden transform rotate-3 hover:rotate-0 transition-transform duration-500">
                <div className="bg-amber-400 p-4 flex items-center space-x-2">
                  <div className="flex space-x-1">
                    <div className="w-3 h-3 bg-white/30 rounded-full"></div>
                    <div className="w-3 h-3 bg-white/30 rounded-full"></div>
                    <div className="w-3 h-3 bg-white/30 rounded-full"></div>
                  </div>
                  <div className="font-semibold text-gray-800">QTruck Dashboard</div>
                </div>
                <div className="flex">
                  <div className="w-48 bg-teal-600 text-white p-4 space-y-2">
                    <div className="bg-white/20 rounded p-2 text-sm">📊 Dashboard</div>
                    <div className="p-2 text-sm opacity-75">🚛 Fleet</div>
                    <div className="p-2 text-sm opacity-75">👥 Drivers</div>
                    <div className="p-2 text-sm opacity-75">🗺️ Routes</div>
                  </div>
                  <div className="flex-1 p-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div className="bg-gray-50 rounded-lg p-4">
                        <div className="text-2xl font-bold text-amber-500">42</div>
                        <div className="text-sm text-gray-600">Active Vehicles</div>
                      </div>
                      <div className="bg-gray-50 rounded-lg p-4">
                        <div className="text-2xl font-bold text-amber-500">156</div>
                        <div className="text-sm text-gray-600">Today's Trips</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-bold text-gray-900 mb-4">Powerful Features</h2>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Everything you need to manage your fleet efficiently
            </p>
          </div>
          
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[
              {
                icon: '🚛',
                title: 'Vehicle Tracking',
                description: 'Real-time GPS tracking, maintenance schedules, and performance monitoring for your entire fleet.'
              },
              {
                icon: '👥',
                title: 'Driver Management', 
                description: 'Driver profiles, licensing, performance tracking, and communication tools in one platform.'
              },
              {
                icon: '🗺️',
                title: 'Route Optimization',
                description: 'Smart routing algorithms to reduce fuel costs and improve delivery times.'
              },
              {
                icon: '📊',
                title: 'Analytics Dashboard',
                description: 'Comprehensive reporting and analytics to make data-driven decisions.'
              },
              {
                icon: '💰',
                title: 'Expense Management',
                description: 'Track fuel costs, maintenance expenses, and generate detailed financial reports.'
              },
              {
                icon: '📱',
                title: 'Mobile App',
                description: 'Native mobile apps for drivers with offline capabilities and real-time updates.'
              }
            ].map((feature, index) => (
              <div key={index} className="card card-hover text-center">
                <div className="text-4xl mb-4">{feature.icon}</div>
                <h3 className="text-xl font-semibold mb-3">{feature.title}</h3>
                <p className="text-gray-600">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Demo Section */}
      <section className="py-20 bg-gradient-to-br from-amber-50 to-green-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12 items-center">
            <div className="space-y-6">
              <h2 className="text-4xl font-bold text-gray-900">
                See QTruck in Action
              </h2>
              <p className="text-xl text-gray-600 leading-relaxed">
                Experience our platform with live demo accounts. Test both admin and driver interfaces 
                to see how QTruck can transform your fleet management.
              </p>
              
              <div className="space-y-4">
                <Link href="/auth/login" className="bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full sm:w-auto text-center">
                  🖥️ Admin Demo
                </Link>
                <Link href="/auth/login" className="bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full sm:w-auto ml-0 sm:ml-4 text-center">
                  📱 Driver Demo  
                </Link>
              </div>
              
              <div className="bg-white/80 backdrop-blur-sm rounded-lg p-6 border border-amber-200">
                <h4 className="font-semibold mb-3">Ready to Get Started?</h4>
                <p className="text-sm text-gray-600">
                  Create your account and start transforming your fleet operations today. 
                  Choose between admin dashboard for fleet managers or driver portal for your drivers.
                </p>
              </div>
            </div>
            
            <div className="relative">
              <div className="bg-gray-800 rounded-2xl p-4 shadow-2xl max-w-sm mx-auto">
                <div className="bg-white rounded-xl overflow-hidden">
                  <div className="bg-amber-400 p-4 flex justify-between items-center">
                    <div className="font-semibold text-gray-800">🚛 QTruck</div>
                    <div className="text-sm text-gray-800">James Mwangi</div>
                  </div>
                  <div className="p-6 space-y-4">
                    <div className="bg-gray-50 rounded-lg p-4 flex items-center space-x-3">
                      <div className="text-2xl">📍</div>
                      <div>
                        <div className="font-semibold">Current Trip</div>
                        <div className="text-sm text-gray-600">Nairobi → Mombasa</div>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4 text-center">
                      <div>
                        <div className="text-2xl font-bold text-amber-500">156</div>
                        <div className="text-xs text-gray-600">Total Trips</div>
                      </div>
                      <div>
                        <div className="text-2xl font-bold text-amber-500">4.9⭐</div>
                        <div className="text-xs text-gray-600">Rating</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Contact Section */}
      <section id="contact" className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-12">
            <div className="space-y-8">
              <div>
                <h2 className="text-4xl font-bold text-gray-900 mb-4">
                  Ready to Get Started?
                </h2>
                <p className="text-xl text-gray-600">
                  Transform your fleet management today. Our team is ready to help you 
                  implement the perfect solution for your business.
                </p>
              </div>
              
              <div className="space-y-6">
                {[
                  { icon: '📧', title: 'Email Us', value: 'info@qalibrated.co.ke' },
                  { icon: '📞', title: 'Call Us', value: '+254 714 999 996' },
                  { icon: '🏢', title: 'Visit Us', value: 'QSL Centre, 1st Floor, Nairobi' }
                ].map((contact, index) => (
                  <div key={index} className="flex items-center space-x-4">
                    <div className="w-12 h-12 bg-gradient-to-r from-amber-400 to-orange-500 rounded-lg flex items-center justify-center text-white text-xl">
                      {contact.icon}
                    </div>
                    <div>
                      <div className="font-semibold">{contact.title}</div>
                      <div className="text-gray-600">{contact.value}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            
            <div className="bg-gray-50 rounded-2xl p-8">
              <form className="space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="label">First Name</label>
                    <input type="text" className="input" />
                  </div>
                  <div>
                    <label className="label">Last Name</label>
                    <input type="text" className="input" />
                  </div>
                </div>
                <div>
                  <label className="label">Company</label>
                  <input type="text" className="input" />
                </div>
                <div>
                  <label className="label">Email</label>
                  <input type="email" className="input" />
                </div>
                <div>
                  <label className="label">Fleet Size</label>
                  <select className="input">
                    <option>Select fleet size</option>
                    <option>1-5 vehicles</option>
                    <option>6-25 vehicles</option>
                    <option>26-100 vehicles</option>
                    <option>100+ vehicles</option>
                  </select>
                </div>
                <div>
                  <label className="label">Message</label>
                  <textarea className="input" rows={4} placeholder="Tell us about your fleet management needs..."></textarea>
                </div>
                <button type="submit" className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
                  Send Message
                </button>
              </form>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-white py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-4 gap-8">
            <div className="space-y-4">
              <div className="flex items-center space-x-2">
                <span className="text-3xl">🚛</span>
                <span className="text-2xl font-bold text-amber-400">QTruck</span>
              </div>
              <p className="text-gray-400">
                The complete fleet management solution for modern logistics companies.
              </p>
            </div>
            
            <div>
              <h4 className="font-semibold text-amber-400 mb-4">Product</h4>
              <div className="space-y-2">
                <a href="#features" className="block text-gray-400 hover:text-white transition-colors">Features</a>
                <a href="#pricing" className="block text-gray-400 hover:text-white transition-colors">Pricing</a>
                <Link href="/auth/signin" className="block text-gray-400 hover:text-white transition-colors">Demo</Link>
              </div>
            </div>
            
            <div>
              <h4 className="font-semibold text-amber-400 mb-4">Solutions</h4>
              <div className="space-y-2">
                <a href="#solutions" className="block text-gray-400 hover:text-white transition-colors">Construction</a>
                <a href="#solutions" className="block text-gray-400 hover:text-white transition-colors">Logistics</a>
                <a href="#solutions" className="block text-gray-400 hover:text-white transition-colors">Retail</a>
              </div>
            </div>
            
            <div>
              <h4 className="font-semibold text-amber-400 mb-4">Support</h4>
              <div className="space-y-2">
                <a href="#contact" className="block text-gray-400 hover:text-white transition-colors">Contact</a>
                <a href="mailto:info@qalibrated.co.ke" className="block text-gray-400 hover:text-white transition-colors">Help Center</a>
                <a href="tel:+254714999996" className="block text-gray-400 hover:text-white transition-colors">Phone Support</a>
              </div>
            </div>
          </div>
          
          <div className="border-t border-gray-800 mt-8 pt-8 flex flex-col md:flex-row justify-between items-center">
            <p className="text-gray-400">
              &copy; 2025 QTruck Kenya. All rights reserved.
            </p>
            <div className="flex space-x-6 mt-4 md:mt-0">
              <a href="#" className="text-gray-400 hover:text-amber-400 transition-colors">LinkedIn</a>
              <a href="#" className="text-gray-400 hover:text-amber-400 transition-colors">Twitter</a>
              <a href="#" className="text-gray-400 hover:text-amber-400 transition-colors">GitHub</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}