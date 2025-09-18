// src/components/FlyingBeeSwarm.jsx
import React, { useState, useEffect } from "react";
import { motion } from "framer-motion";
import bee from "../assets/bee.svg"; // bee asset
import beehive from "../assets/beehives.png"; // beehive asset

// Utility: pick random corner (not the hive corner)
const getRandomCorner = (width, height, exclude = null) => {
  const corners = [
    { x: 0, y: 0 }, // top-left
    { x: width, y: 0 }, // top-right
    { x: 0, y: height }, // bottom-left (hive)
    { x: width, y: height }, // bottom-right
  ];
  let choice = corners[Math.floor(Math.random() * corners.length)];
  if (exclude && choice.x === exclude.x && choice.y === exclude.y) {
    return getRandomCorner(width, height, exclude);
  }
  return choice;
};

// Generate a smooth curved path
const generatePath = (start, end, width, height) => {
  const cp1 = { x: Math.random() * width, y: Math.random() * height };
  const cp2 = { x: Math.random() * width, y: Math.random() * height };
  return `M${start.x},${start.y} C${cp1.x},${cp1.y} ${cp2.x},${cp2.y} ${end.x},${end.y}`;
};

const Bee = ({ fromHive = false, speed = 8, size = "w-10 h-10", footerHeight = 220, delay = 0 }) => {
  const [footerWidth, setFooterWidth] = useState(window.innerWidth);
  const hiveCorner = { x: 0, y: footerHeight }; // bottom-left hive

  const [path, setPath] = useState("");

  useEffect(() => {
    const handleResize = () => setFooterWidth(window.innerWidth);
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  // Initial path
  useEffect(() => {
    let start = fromHive ? hiveCorner : getRandomCorner(footerWidth, footerHeight);
    let end = fromHive
      ? getRandomCorner(footerWidth, footerHeight, hiveCorner)
      : getRandomCorner(footerWidth, footerHeight);
    setPath(generatePath(start, end, footerWidth, footerHeight));
  }, [footerWidth, footerHeight, fromHive]);

  // On complete → pick new path
  const handleComplete = () => {
    let start, end;
    if (fromHive) {
      // If last path ended at hive → fly out, else return
      const returning = path.endsWith(`${hiveCorner.x},${hiveCorner.y}`);
      if (returning) {
        start = hiveCorner;
        end = getRandomCorner(footerWidth, footerHeight, hiveCorner);
      } else {
        start = getRandomCorner(footerWidth, footerHeight, hiveCorner);
        end = hiveCorner;
      }
    } else {
      start = getRandomCorner(footerWidth, footerHeight);
      end = getRandomCorner(footerWidth, footerHeight);
    }
    setPath(generatePath(start, end, footerWidth, footerHeight));
  };

  return (
    <motion.img
      key={path}
      src={bee}
      alt="Flying bee"
      className={`${size} absolute pointer-events-none z-20`}
      style={{
        offsetPath: `path("${path}")`,
        offsetRotate: "auto",
      }}
      animate={{
        offsetDistance: ["0%", "100%"],
        rotate: [0, 10, -10, 5, -5, 0],
      }}
      transition={{
        duration: speed,
        ease: "easeInOut",
        delay,
      }}
      onAnimationComplete={handleComplete}
    />
  );
};

const FlyingBeeSwarm = () => {
  return (
    <>
      {/* Glowing Beehive */}
      <motion.div
        className="absolute bottom-4 right-4 w-24 h-24 rounded-full flex items-center justify-center z-10"
        
        transition={{ duration: 2, repeat: Infinity, repeatType: "mirror" }}
      >
        <img src={beehive} alt="Beehive" className="w-20 h-20" />
      </motion.div>

      {/* Bee 1 → hive ↔ random corners */}
      <Bee fromHive={true} speed={9} size="w-12 h-12" footerHeight={220} />

      {/* Bee 2 → free buzzing */}
      <Bee fromHive={false} speed={6} size="w-8 h-8" footerHeight={220} delay={1.5} />
    </>
  );
};

export default FlyingBeeSwarm;
