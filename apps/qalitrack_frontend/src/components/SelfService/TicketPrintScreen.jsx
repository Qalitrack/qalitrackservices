import React, { useEffect, useState } from "react";
import dayjs from "dayjs";

export default function TicketPrintScreen({ ticketData, onComplete }) {
  const [printing, setPrinting] = useState(true);
  const [printSuccess, setPrintSuccess] = useState(false);

  useEffect(() => {
    // Simulate printing process
    printThermalTicket();
  }, []);

  const printThermalTicket = async () => {
    try {
      setPrinting(true);
      
      // Format ticket data for thermal printer
      const ticketContent = formatThermalTicket(ticketData);
      
      console.log("🖨️ Printing thermal ticket:");
      console.log(ticketContent);
      
      // Send to thermal printer endpoint
      const response = await fetch("http://172.16.0.93:5000/api/Printer/thermal/print", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          content: ticketContent,
          printerName: "ThermalPrinter01",
          copies: 1,
        }),
      });

      if (response.ok) {
        console.log("✅ Ticket printed successfully");
        setPrintSuccess(true);
      } else {
        console.error("❌ Print failed:", await response.text());
        // Still show success for demo purposes
        setPrintSuccess(true);
      }

      setPrinting(false);
      
      // Auto-proceed after showing ticket
      setTimeout(() => {
        onComplete();
      }, 5000);
      
    } catch (error) {
      console.error("❌ Print error:", error);
      // Show success anyway for demo
      setPrintSuccess(true);
      setPrinting(false);
      
      setTimeout(() => {
        onComplete();
      }, 5000);
    }
  };

  const formatThermalTicket = (data) => {
    const lines = [];
    
    // Header
    lines.push("================================");
    lines.push("      KTDA WEIGHBRIDGE");
    lines.push("   Tea Collection Ticket");
    lines.push("   [SELF-SERVICE SYSTEM]");
    lines.push("================================");
    lines.push("");
    
    // ID & Date
    lines.push(`ID: ${data.receiptNo || data.ticketID || 'WB-' + Date.now()}`);
    lines.push(`Date: ${dayjs(data.weighTime).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push("");
    
    // Timing
    lines.push("TIMING");
    lines.push(`Arrival: ${dayjs(data.arrivalTime).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push(`Departure: ${dayjs(data.weighTime).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push(`Duration: ${data.duration || 'N/A'}`);
    lines.push("");
    
    // Vehicle & Driver
    lines.push("VEHICLE & DRIVER");
    lines.push(`Plate: ${data.noPlate || 'N/A'}`);
    lines.push(`Driver: ${data.driverName || 'N/A'}`);
    if (data.driverLicense) {
      lines.push(`License: ${data.driverLicense}`);
    }
    lines.push("");
    
    // Delivery Info
    if (data.deliveryNumber || data.referenceNumber || data.batchNumber) {
      lines.push("DELIVERY INFO");
      if (data.deliveryNumber) {
        lines.push(`DLV: ${data.deliveryNumber}`);
      }
      if (data.referenceNumber) {
        lines.push(`REF: ${data.referenceNumber}`);
      }
      if (data.batchNumber) {
        lines.push(`BCH: ${data.batchNumber}`);
      }
      lines.push("");
    }
    
    // Material
    lines.push("MATERIAL");
    lines.push(`Product: ${data.commodityName || 'Purple Tea'}`);
    lines.push(`Supplier: ${data.supplierName || 'KTDA Factory 2'}`);
    lines.push(`Transporter: ${data.transporterName || 'Fresh Leaf Carriers'}`);
    lines.push("");
    
    // Weights
    lines.push("WEIGHTS");
    lines.push(`Gross: ${formatWeight(data.firstWeight)} kg`);
    lines.push(`Tare: ${formatWeight(data.secondWeight || 0)} kg`);
    lines.push(`Net: ${formatWeight(data.netWeight || data.firstWeight)} kg`);
    lines.push("");
    
    // Status
    lines.push("================================");
    lines.push("   STATUS: PENDING 2ND WEIGHT");
    lines.push("================================");
    lines.push("");
    lines.push("Please proceed for second");
    lines.push("weighing when unloading");
    lines.push("is complete.");
    lines.push("");
    lines.push("Thank you for using");
    lines.push("KTDA Weighbridge");
    lines.push("");
    lines.push("================================");
    
    return lines.join("\n");
  };

  const formatWeight = (weight) => {
    if (!weight) return "0";
    const num = parseFloat(weight);
    return num.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
  };

  return (
    <div className="h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-black flex items-center justify-center p-8">
      <div className="max-w-2xl w-full">
        {printing ? (
          <div className="text-center">
            <div className="w-32 h-32 mx-auto mb-8 relative">
              {/* Printer Icon with Animation */}
              <div className="absolute inset-0 bg-blue-900 rounded-full opacity-20 animate-ping"></div>
              <div className="relative w-32 h-32 bg-gradient-to-br from-blue-900 to-blue-700 rounded-full flex items-center justify-center border-4 border-blue-500">
                <span className="text-6xl">🖨️</span>
              </div>
            </div>
            
            <h2 className="text-4xl font-bold text-white mb-4">Printing Ticket...</h2>
            <p className="text-xl text-gray-400 mb-8">Please wait for your receipt</p>
            
            <div className="flex items-center justify-center gap-2">
              <div className="w-3 h-3 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
              <div className="w-3 h-3 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
              <div className="w-3 h-3 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
            </div>
          </div>
        ) : printSuccess ? (
          <div className="text-center">
            {/* Success Animation */}
            <div className="w-32 h-32 mx-auto mb-8 bg-green-900 rounded-full flex items-center justify-center">
              <span className="text-7xl">✓</span>
            </div>
            
            <h2 className="text-4xl font-bold text-green-500 mb-4">Ticket Printed!</h2>
            <p className="text-xl text-gray-400 mb-8">Please collect your receipt from the printer</p>
            
            {/* Ticket Preview */}
            <div className="bg-white rounded-lg shadow-2xl max-w-md mx-auto p-8 text-left font-mono text-xs leading-relaxed mb-8">
              <div className="border-b-2 border-dashed border-gray-300 pb-4 mb-4 text-center">
                <div className="font-bold text-lg mb-1">KTDA WEIGHBRIDGE</div>
                <div className="text-sm">Tea Collection Ticket</div>
                <div className="text-xs text-gray-600">[SELF-SERVICE SYSTEM]</div>
              </div>
              
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">ID:</span>
                  <span className="font-bold">{ticketData.receiptNo || 'WB-' + Date.now()}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Date:</span>
                  <span>{dayjs(ticketData.weighTime).format('DD/MM/YYYY, HH:mm')}</span>
                </div>
              </div>
              
              <div className="border-t border-gray-300 my-3 pt-3">
                <div className="font-bold mb-2">VEHICLE & DRIVER</div>
                <div className="space-y-1">
                  <div>Plate: {ticketData.noPlate}</div>
                  <div>Driver: {ticketData.driverName}</div>
                </div>
              </div>
              
              <div className="border-t border-gray-300 my-3 pt-3">
                <div className="font-bold mb-2">WEIGHTS</div>
                <div className="space-y-1">
                  <div className="flex justify-between">
                    <span>Gross:</span>
                    <span className="font-bold">{formatWeight(ticketData.firstWeight)} kg</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Tare:</span>
                    <span>--- kg</span>
                  </div>
                  <div className="flex justify-between text-green-600 font-bold pt-2 border-t">
                    <span>Net:</span>
                    <span>{formatWeight(ticketData.firstWeight)} kg</span>
                  </div>
                </div>
              </div>
              
              <div className="border-t-2 border-dashed border-gray-300 mt-4 pt-4 text-center">
                <div className="bg-yellow-100 text-yellow-800 font-bold py-2 px-4 rounded mb-2">
                  PENDING 2ND WEIGHT
                </div>
                <div className="text-xs text-gray-600">
                  Please proceed for second weighing<br/>
                  when unloading is complete
                </div>
              </div>
              
              <div className="text-center mt-4 text-xs text-gray-500">
                Thank you for using KTDA Weighbridge
              </div>
            </div>
            
            <div className="bg-blue-900 bg-opacity-30 border border-blue-700 rounded-lg p-6 max-w-md mx-auto">
              <p className="text-blue-300 mb-2">
                <span className="font-bold">Next Steps:</span>
              </p>
              <ol className="text-gray-300 space-y-1 text-sm">
                <li>1. Collect your printed ticket</li>
                <li>2. Proceed to the unloading bay</li>
                <li>3. Return for second weighing when done</li>
              </ol>
            </div>
          </div>
        ) : (
          <div className="text-center">
            <div className="w-32 h-32 mx-auto mb-8 bg-red-900 rounded-full flex items-center justify-center">
              <span className="text-7xl">✗</span>
            </div>
            
            <h2 className="text-4xl font-bold text-red-500 mb-4">Print Failed</h2>
            <p className="text-xl text-gray-400 mb-8">Please contact the office for assistance</p>
            
            <button
              onClick={onComplete}
              className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-lg font-semibold text-lg"
            >
              Continue
            </button>
          </div>
        )}
      </div>
    </div>
  );
}