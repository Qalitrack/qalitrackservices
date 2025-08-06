# S/4HANA Bamburi Cement Data Setup

This script automatically creates Bamburi Cement sample data in your S/4HANA Cloud Public Edition trial using Playwright web automation.

## What This Script Creates

### 🏭 Business Partners (Customers & Vendors)
- **China Road & Bridge Corporation** - Large infrastructure contractor
- **Nairobi Hardware Dealers Ltd** - Regional cement dealer  
- **John Mwangi Construction** - Individual customer
- **Mombasa Transport SACCO Ltd** - Transportation vendor

### 📦 Materials (Cement Products)
- **PowerMax Premium Cement 50kg** (PWRMAX50) - KES 1,250/bag
- **Nguvu General Purpose Cement 50kg** (NGUVU50) - KES 835/bag
- **Fundi Affordable Cement 50kg** (FUNDI50) - KES 750/bag
- **BamburiBlox Paving Blocks** (BBLOX01) - KES 45/piece

### 🛒 Sales Orders
- Large infrastructure project order for CRBC
- Regional dealer bulk order
- Individual customer order

## Prerequisites

1. **Active S/4HANA Cloud Public Edition trial**
2. **Node.js** installed (v16 or higher)
3. **Internet connection** for Playwright browser download

## Installation

```bash
cd scripts
npm install
npx playwright install chromium
```

## Configuration

The script is pre-configured with your trial details:
- **System URL**: `https://my305028.s4hana.ondemand.com`
- **Username**: `surgbc@gmail.com`
- **Password**: `qaliHANA12`

## Usage

### Run the Complete Setup
```bash
npm run setup
```

### Or run directly with Node.js
```bash
node s4hana-bamburi-data-setup.js
```

## Script Features

### 🤖 Automated Navigation
- Handles SAML authentication
- Navigates Fiori Launchpad
- Searches for and opens required apps
- Manages form filling and submissions

### 🛡️ Error Handling
- Multiple selector strategies for robust element finding
- Graceful handling of missing fields
- Timeout management for slow S/4HANA responses
- Comprehensive error logging

### 📊 Progress Tracking
- Real-time progress updates
- Created data tracking
- Final summary report
- Generated ID capture

### 🎯 Browser Options
- **Headless Mode**: Set `headless: true` for background execution
- **Slow Mode**: Configurable delay for visibility
- **Screenshots**: Automatic capture on errors

## Expected Runtime

- **Total Time**: 15-25 minutes
- **Business Partners**: ~3 minutes each
- **Materials**: ~4 minutes each  
- **Sales Orders**: ~5 minutes each

## Troubleshooting

### Common Issues

#### 1. Login Problems
```bash
Error: Could not find username field
```
**Solution**: The SAML login page layout may have changed. Check the browser window and manually complete login if needed.

#### 2. App Not Found
```bash
Could not navigate to [App Name]
```
**Solution**: 
- Ensure your trial has the required apps enabled
- Try searching manually in S/4HANA first
- Some apps may have different names in your trial

#### 3. Form Field Issues
```bash
Could not find field: [Field Name]
```
**Solution**: S/4HANA forms vary by configuration. The script will skip missing fields and continue.

#### 4. Timeout Errors
```bash
Timeout exceeded
```
**Solution**: S/4HANA can be slow. Increase timeouts in the script or run during off-peak hours.

### Debug Mode

To run with visible browser and slower execution:
```javascript
// In the script, modify:
this.browser = await chromium.launch({ 
    headless: false,    // Shows browser
    slowMo: 2000       // 2 second delays
});
```

### Manual Verification

After script completion, verify data in S/4HANA:

1. **Business Partners**: Search for "CRBC", "NHWD", etc.
2. **Materials**: Search for "PWRMAX50", "NGUVU50", etc.
3. **Sales Orders**: Check created order numbers

## Script Output

### Success Output
```
🚀 Initializing Playwright browser...
🔐 Logging into S/4HANA trial...
✅ Successfully logged in

👥 Creating Business Partner: China Road & Bridge Corporation...
✅ Created Business Partner: China Road & Bridge Corporation (1234567890)

📦 Creating Material: PowerMax Premium Cement 50kg...
✅ Created Material: PowerMax Premium Cement 50kg

🛒 Creating Sales Order for: CRBC...
✅ Created Sales Order: 4500001234 for CRBC

📊 BAMBURI CEMENT SAMPLE DATA CREATION REPORT
============================================================

👥 Business Partners Created:
   ✅ China Road & Bridge Corporation (1234567890)
   ✅ Nairobi Hardware Dealers Ltd (1234567891)
   ✅ John Mwangi Construction (1234567892)
   ✅ Mombasa Transport SACCO Ltd (1234567893)

📦 Materials Created:
   ✅ PowerMax Premium Cement 50kg (PWRMAX50)
   ✅ Nguvu General Purpose Cement 50kg (NGUVU50)
   ✅ Fundi Affordable Cement 50kg (FUNDI50)
   ✅ BamburiBlox Paving Blocks (BBLOX01)

🛒 Sales Orders Created:
   ✅ 4500001234 - CRBC (KES 16675000)

🎉 Data creation completed successfully!
```

## Next Steps

After successful data creation:

1. **Verify Data**: Log into S/4HANA and confirm all data was created
2. **Document IDs**: Save the generated Business Partner and Material numbers
3. **Test APIs**: Use the created data to test OData API access
4. **QaliTrack Integration**: Begin integration development with realistic data

## Customization

### Adding More Data

To add additional business partners, modify the `businessPartners` array:

```javascript
const businessPartners = [
    // Existing entries...
    {
        name: 'Your New Customer',
        searchTerm: 'YOURNEW',
        type: 'Customer',
        country: 'KE',
        city: 'Nairobi',
        street: 'Your Street',
        postalCode: '00100',
        phone: '+254-xxx-xxxxxx'
    }
];
```

### Modifying Materials

Update the `materials` array for different products:

```javascript
const materials = [
    // Existing entries...
    {
        number: 'NEWPROD01',
        type: 'FERT',
        description: 'New Product Description',
        baseUnit: 'KG',
        group: 'CEMENT',
        weight: '25',
        price: '500'
    }
];
```

## Support

For issues:
1. Check the browser window for visual debugging
2. Review S/4HANA trial app availability  
3. Verify trial account permissions
4. Contact QaliTrack development team

---

**Created for**: Bamburi Cement S/4HANA Integration Project  
**Author**: QaliTrack Development Team  
**Version**: 1.0.0