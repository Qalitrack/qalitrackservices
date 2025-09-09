import mongoose from "mongoose";

const productSchema = new mongoose.Schema({
  name: { type: String, required: true },
  category: { type: String, required: true },
  stock: { type: Number, required: true, min: 0 },
  price: { type: Number, required: true, min: 0 },
  status: {
    type: String,
    enum: ["Active", "Low Stock", "Inactive"],
    default: "Active"
  },
  image: {
    type: String,
    default: "https://placehold.co/60x60/cccccc/333333?text=N/A"
  },
  createdAt: { type: Date, default: Date.now }
});

export default mongoose.model("Product", productSchema);
