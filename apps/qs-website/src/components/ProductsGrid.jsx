import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

// --- Configuration ---
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL
console.log({API_BASE_URL})

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
    <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center p-4 z-50 overflow-y-auto">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
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
              alt={product.name}
              className="w-full h-auto object-cover rounded-lg shadow-md"
              onError={(e) => {
                e.target.onerror = null;
                e.target.src = "https://placehold.co/400x400/cccccc/333333?text=No+Image";
              }}
            />
          </div>
          <div className="flex-grow w-full md:w-1/2">
            <p className="text-lg text-gray-700 mb-2"><strong>Category:</strong> {product.category}</p>
            <p className="text-lg text-gray-700 mb-2"><strong>Stock:</strong> {product.stock}</p>
            
            {/* Removed Status Display */}

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
          
          {/* Contact Us Button (Link to Contact page) */}
          <Link
            to="/contact"
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
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await axios.get(`${API_BASE_URL}/products`);
        
        // Shuffle and pick 6 products
        const shuffled = shuffleArray(response.data);
        setProducts(shuffled.slice(0, 6));
      } catch (err) {
        console.error("Error fetching products:", err);
        setError("Failed to load products. Please ensure your backend server is running and accessible.");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

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
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
          {products.map((product) => (
            <div
              key={product._id}
              className="bg-white rounded-lg shadow-lg overflow-hidden cursor-pointer transform hover:scale-105 transition-transform duration-300 flex flex-col"
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
                  <p className="text-gray-600 text-sm mb-1">Category: {product.category}</p>
                </div>
                {/* Removed Status Badge */}
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
