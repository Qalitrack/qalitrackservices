// generate-placeholders.js
import sharp from "sharp";
import fs from "fs";
import path from "path";

const images = [
  { src: "src/assets/hero/trans.jpg", dest: "public/placeholders/trans-small.jpg" },
  { src: "src/assets/hero/calibration.png", dest: "public/placeholders/calibration-small.jpg" },
  { src: "src/assets/hero/rnd34.jpeg", dest: "public/placeholders/rnd34-small.jpg" },
  { src: "src/assets/hero/cargotracking.jpeg", dest: "public/placeholders/cargotracking-small.jpg" },
  { src: "src/assets/hero/weighing.png", dest: "public/placeholders/weighing-small.jpg" },
];

// Ensure placeholders folder exists
const outDir = "public/placeholders";
if (!fs.existsSync(outDir)) {
  fs.mkdirSync(outDir, { recursive: true });
}

images.forEach(async ({ src, dest }) => {
  try {
    await sharp(src)
      .resize(40)        // shrink down to tiny size
      .blur(10)          // apply heavy blur for smooth look
      .toFile(dest);

    console.log(`✅ Created placeholder: ${dest}`);
  } catch (err) {
    console.error(`❌ Failed for ${src}:`, err);
  }
});
