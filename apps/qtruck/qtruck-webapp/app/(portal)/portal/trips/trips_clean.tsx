// I'll work on cleaning this up properly by showing the correct workflow:

/* CORRECT TRIP WORKFLOW:

1. CREATE TRIP (no photos needed):
   - Truck selection
   - Start/End locations  
   - Material selection (optional)
   - Material Cost in KSh (when material selected) - THIS IS THE REVENUE
   - Current location capture (optional)
   - NO PHOTOS IN CREATION

2. START TRIP (photos needed):
   - Start mileage reading
   - Start mileage photo
   - Material loading photos (if material selected)

3. DURING TRIP:
   - Add expenses with receipt photos

4. END TRIP (photos needed):
   - End mileage reading
   - End mileage photo

The material cost field IS already in the form but only shows when material is selected.
Photos should ONLY be in start/end trip actions, NOT in creation.
*/