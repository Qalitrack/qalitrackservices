'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { productsAPI } from '@/utils/apiFactory';

// Utility function to shuffle an array
const shuffleArray = (array) => {
  return array
    .map((item) => ({ item, sort: Math.random() }))
    .sort((a, b) => a.sort - b.sort)
    .map(({ item }) => item);
};

// Product Detail Modal Component
const ProductDetailModal = ({ show, onClose, product }) => {
  if (!show || !product) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center p-4 z-50 overflow-y-auto transition-opacity duration-300"
      role="dialog"
      aria-modal="true"
    >
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
          aria-label="Close modal"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-6 h-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <h2 className="text-3xl font-bold text-gray-800 mb-6 text-center">{product.name}</h2>

        <div className="flex flex-col md:flex-row gap-6 items-center">
          <div className="flex-shrink-0 w-full md:w-1/2">
            <img
              src={product.image || 'https://placehold.co/400x400/cccccc/333333?text=No+Image'}
              alt={product.name || 'Product image'}
              className="w-full h-auto object-cover rounded-lg shadow-md"
              onError={(e) => {
                e.target.onerror = null;
                e.target.src = "https://placehold.co/400x400/cccccc/333333?text=No+Image";
              }}
            />
          </div>
          <div className="flex-grow w-full md:w-1/2">
            <p className="text-md text-gray-600">
              <strong className="block mb-1">Description:</strong>
              {product.description || 'No detailed description available for this product.'}
            </p>
          </div>
        </div>

        {/* Buttons */}
        <div className="mt-8 flex flex-col sm:flex-row justify-center gap-4 text-center">
          <button
            onClick={onClose}
            className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold py-2 px-6 rounded-md shadow-md transition duration-200"
          >
            Close
          </button>
          <Link
            href="/contact"
            className="bg-amber-500 hover:bg-amber-600 text-white font-semibold py-2 px-6 rounded-md shadow-md transition duration-200"
          >
            Contact Us to Learn More
          </Link>
        </div>
      </div>
    </div>
  );
};

const ProductGrid = () => {
  const [allProducts, setAllProducts] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);

  // For fade animation
  const [fade, setFade] = useState(true);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await productsAPI.getAll();

        if (!response.data || response.data.length === 0) {
          setAllProducts([]);
          return;
        }

        setAllProducts(response.data);

        // Initial 9 products
        const shuffled = shuffleArray(response.data);
        setProducts(shuffled.slice(0, 9));
      } catch (err) {
        console.error("Error fetching products:", err);
        setError("Failed to load products. Please ensure your backend server is running and accessible.");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  // Rotate products every 2 minutes with fade effect
  useEffect(() => {
    if (allProducts.length === 0) return;

    const interval = setInterval(() => {
      setFade(false); // start fade-out
      setTimeout(() => {
        const shuffled = shuffleArray(allProducts);
        setProducts(shuffled.slice(0, 9));
        setFade(true); // fade-in
      }, 500); // wait for fade-out before swapping
    }, 120000); // 2 minutes

    return () => clearInterval(interval);
  }, [allProducts]);

  const handleCardClick = (product) => {
    setSelectedProduct(product);
    setShowDetailModal(true);
  };

  const handleCloseDetailModal = () => {
    setShowDetailModal(false);
    setSelectedProduct(null);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <p className="text-gray-600 text-lg">Loading products...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex justify-center items-center h-64">
        <p className="text-red-500 text-lg">{error}</p>
      </div>
    );
  }

  return (
    <section className="container mx-auto px-4 py-8">
      <h2 className="text-4xl font-bold text-gray-800 text-center mb-10">Featured Products</h2>

      {products.length === 0 ? (
        <p className="text-center text-gray-600 text-lg">No products available to display.</p>
      ) : (
        <div
          className={`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8 transition-opacity duration-500 ${
            fade ? 'opacity-100' : 'opacity-0'
          }`}
        >
          {products.map((product) => (
            <div
              key={product._id}
              className="bg-white rounded-lg shadow-lg shadow-amber-500/50 overflow-hidden cursor-pointer transform hover:scale-105 transition-transform duration-300 flex flex-col"
              onClick={() => handleCardClick(product)}
            >
              <div className="relative w-full" style={{ paddingBottom: '100%' }}>
                <img
                  src={product.image || 'https://placehold.co/400x400/cccccc/333333?text=No+Image'}
                  alt={product.name}
                  className="absolute inset-0 w-full h-full object-cover"
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = "https://placehold.co/400x400/cccccc/333333?text=No+Image";
                  }}
                />
              </div>
              <div className="p-5 flex-grow flex flex-col justify-between">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">{product.name}</h3>
                  {/* <p className="text-gray-600 text-sm">
                    {product.description || 'No description available.'}
                  </p> */}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Product Detail Modal */}
      <ProductDetailModal
        show={showDetailModal}
        onClose={handleCloseDetailModal}
        product={selectedProduct}
      />
    </section>
  );
};

export default ProductGrid;
