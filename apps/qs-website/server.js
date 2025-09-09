// server.js - Node.js Express Server (ES Modules)

import express from 'express';
import mongoose from 'mongoose';
import cors from 'cors'; // Import cors middleware
import bcrypt from 'bcryptjs'; // Import bcrypt for password hashing
import jwt from 'jsonwebtoken'; // Import jsonwebtoken for JWT
import process from 'process'; // Import process for ES Modules
import dotenv from 'dotenv'; // Import dotenv for environment variables

// Load environment variables
dotenv.config();

const app = express();
const port = 5000; // The port your React app expects the API to be on

// --- Configuration ---
// Define a JWT Secret Key. IMPORTANT: In a production environment, use a strong,
// randomly generated key stored securely (e.g., in environment variables).
const JWT_SECRET = process.env.JWT_SECRET || 'your_super_secret_jwt_key'; 

// --- Middleware ---
app.use(cors()); // Enable CORS for all routes
app.use(express.json()); // Enable parsing of JSON request bodies

// --- MongoDB Connection ---
const mongoURI = process.env.MONGODB_URI || `mongodb://${process.env.MONGO_USERNAME || ''}:${process.env.MONGO_PASSWORD || ''}@${process.env.MONGO_HOST || 'localhost'}:${process.env.MONGO_PORT || '27017'}/${process.env.MONGO_DATABASE || 'products_dashboard_db'}`; 

mongoose.connect(mongoURI)
  .then(() => console.log('MongoDB connected successfully!'))
  .catch(err => console.error('MongoDB connection error:', err));

// --- Mongoose Schemas and Models ---

// Product Schema
const productSchema = new mongoose.Schema({
  name: { type: String, required: true },
  category: { type: String, required: true },
  stock: { type: Number, required: true, min: 0 },
  price: { type: Number, required: true, min: 0 },
  status: { type: String, enum: ['Active', 'Low Stock', 'Inactive'], default: 'Active' },
  image: { type: String, default: 'https://placehold.co/60x60/cccccc/333333?text=N/A' }, // Placeholder image
  createdAt: { type: Date, default: Date.now }
});
const Product = mongoose.model('Product', productSchema);

// User Schema (updated for authentication)
const userSchema = new mongoose.Schema({
  name: { type: String, required: true },
  email: { type: String, required: true, unique: true },
  password: { type: String, required: true }, // Added password field
  role: { type: String, enum: ['Admin', 'Client', 'Guest'], default: 'Client' },
  status: { type: String, enum: ['Active', 'Pending', 'Inactive'], default: 'Active' },
  lastLogin: { type: Date, default: Date.now }
});

// Pre-save hook to hash password before saving or updating a user
userSchema.pre('save', async function(next) {
  // Only hash the password if it has been modified (or is new)
  if (this.isModified('password')) {
    const salt = await bcrypt.genSalt(10); // Generate a salt
    this.password = await bcrypt.hash(this.password, salt); // Hash the password
  }
  next();
});

const User = mongoose.model('User', userSchema);

// --- Authentication Middleware (Optional, but good for securing routes) ---
const authenticateToken = (req, res, next) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

  if (token == null) return res.status(401).json({ message: 'Authentication token required' });

  jwt.verify(token, JWT_SECRET, (err, user) => {
    if (err) return res.status(403).json({ message: 'Invalid or expired token' });
    req.user = user; // Attach user payload to request
    next();
  });
};


// --- API Routes ---

// Products API (no changes, but could be protected with authenticateToken)
app.get('/api/products', async (req, res) => {
  try {
    const products = await Product.find();
    res.json(products);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

app.post('/api/products', authenticateToken, async (req, res) => {
  const product = new Product({
    name: req.body.name,
    category: req.body.category,
    stock: req.body.stock,
    price: req.body.price,
    status: req.body.status,
    image: req.body.image
  });
  try {
    const newProduct = await product.save();
    res.status(201).json(newProduct);
  } catch (err) {
    res.status(400).json({ message: err.message });
  }
});

app.put('/api/products/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const updatedProduct = await Product.findByIdAndUpdate(id, req.body, { new: true, runValidators: true });
    if (!updatedProduct) {
      return res.status(404).json({ message: 'Product not found' });
    }
    res.json(updatedProduct);
  } catch (err) {
    res.status(400).json({ message: err.message });
  }
});

app.delete('/api/products/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const deletedProduct = await Product.findByIdAndDelete(id);
    if (!deletedProduct) {
      return res.status(404).json({ message: 'Product not found' });
    }
    res.json({ message: 'Product deleted successfully' });
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

// Users API (updated post and put for password handling, could be protected)
app.get('/api/users', async (req, res) => {
  try {
    // Exclude password field from results for security
    const users = await User.find().select('-password'); 
    res.json(users);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

app.post('/api/users', async (req, res) => {
  // Password hashing is handled by the pre-save hook in the schema
  const user = new User({
    name: req.body.name,
    email: req.body.email,
    password: req.body.password, // This will be hashed by the pre-save hook
    role: req.body.role,
    status: req.body.status,
    lastLogin: req.body.lastLogin // You might want to set this on login instead
  });
  try {
    const newUser = await user.save();
    // Return user data without the password
    const { password: _unused, ...userWithoutPassword } = newUser._doc; 
    res.status(201).json(userWithoutPassword);
  } catch (err) {
    // Handle duplicate email error
    if (err.code === 11000) {
      return res.status(409).json({ message: 'Email already exists.' });
    }
    res.status(400).json({ message: err.message });
  }
});

app.put('/api/users/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const updateData = { ...req.body };

    // If password is provided in the update, hash it
    if (updateData.password) {
      const salt = await bcrypt.genSalt(10);
      updateData.password = await bcrypt.hash(updateData.password, salt);
    }

    const updatedUser = await User.findByIdAndUpdate(id, updateData, { new: true, runValidators: true });
    if (!updatedUser) {
      return res.status(404).json({ message: 'User not found' });
    }
    // Return updated user data without the password
    const { password: _unused, ...userWithoutPassword } = updatedUser._doc;
    res.json(userWithoutPassword);
  } catch (err) {
    // Handle duplicate email error on update
    if (err.code === 11000) {
      return res.status(409).json({ message: 'Email already exists.' });
    }
    res.status(400).json({ message: err.message });
  }
});

app.delete('/api/users/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const deletedUser = await User.findByIdAndDelete(id);
    if (!deletedUser) {
      return res.status(404).json({ message: 'User not found' });
    }
    res.json({ message: 'User deleted successfully' });
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

// --- Authentication Routes ---
app.post('/api/auth/login', async (req, res) => {
  const { email, password } = req.body;

  try {
    // 1. Find the user by email
    const user = await User.findOne({ email });
    if (!user) {
      return res.status(401).json({ message: 'Invalid email or password.' });
    }

    // 2. Compare the provided password with the stored hashed password
    const isMatch = await bcrypt.compare(password, user.password);
    if (!isMatch) {
      return res.status(401).json({ message: 'Invalid email or password.' });
    }

    // 3. Generate a JWT token
    // The payload can include user ID, role, etc. Do NOT include sensitive info like password.
    const token = jwt.sign(
      { id: user._id, role: user.role, email: user.email },
      JWT_SECRET,
      { expiresIn: '1h' } // Token expires in 1 hour
    );

    // Update lastLogin timestamp
    user.lastLogin = Date.now();
    await user.save(); // Save the updated user (pre-save hook won't re-hash if password isn't modified)

    // 4. Send the token back to the client
    res.json({ message: 'Login successful', token });

  } catch (err) {
    console.error('Login error:', err);
    res.status(500).json({ message: 'Server error during login.' });
  }
});

// Example of a protected route (uncomment and use authenticateToken to test)
// app.get('/api/protected-data', authenticateToken, (req, res) => {
//   res.json({ message: `Welcome, ${req.user.email}! This is protected data.` });
// });


// --- Start the server ---
app.listen(port, () => {
  console.log(`Server running on http://localhost:${port}`);
});
