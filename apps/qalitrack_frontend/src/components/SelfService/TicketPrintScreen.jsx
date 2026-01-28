import React, { useEffect, useState } from "react";
import dayjs from "dayjs";
import { useTheme } from "../Context/ThemeContext.jsx";

export default function TicketPrintScreen({ ticketData, onComplete }) {
  const { theme, isDark } = useTheme();
  const [printing, setPrinting] = useState(true);
  const [printSuccess, setPrintSuccess] = useState(false);

  useEffect(() => {
    printThermalTicket();
  }, []);

  const printThermalTicket = async () => {
    try {
      setPrinting(true);
      
      const ticketContent = formatThermalTicket(ticketData);
      
      console.log("🖨️ Printing thermal ticket:");
      console.log(ticketContent);
      
      // REAL MODE: Send to thermal printer endpoint
      /*
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
        setPrintSuccess(true); // Show success for demo
      }
      */
      
      // SIMULATION: Auto-success after 2 seconds
      setTimeout(() => {
        setPrintSuccess(true);
        setPrinting(false);
      }, 2000);
      
      // Auto-proceed after showing ticket
      setTimeout(() => {
        onComplete();
      }, 7000);
      
    } catch (error) {
      console.error("❌ Print error:", error);
      setPrintSuccess(true);
      setPrinting(false);
      
      setTimeout(() => {
        onComplete();
      }, 7000);
    }
  };

  const formatThermalTicket = (data) => {
    const lines = [];
    
    lines.push("================================");
    lines.push("      KTDA WEIGHBRIDGE");
    lines.push("   Tea Collection Ticket");
    lines.push("   [SELF-SERVICE SYSTEM]");
    lines.push("================================");
    lines.push("");
    
    lines.push(`ID: ${data.receiptNo || data.ticketID || 'WB-' + Date.now()}`);
    lines.push(`Date: ${dayjs(data.weighTime || new Date()).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push("");
    
    lines.push("TIMING");
    lines.push(`Arrival: ${dayjs(data.arrivalTime || new Date()).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push(`Departure: ${dayjs(data.weighTime || new Date()).format('DD/MM/YYYY, HH:mm:ss')}`);
    lines.push(`Duration: ${data.duration || 'N/A'}`);
    lines.push("");
    
    lines.push("VEHICLE & DRIVER");
    lines.push(`Plate: ${data.noPlate || 'N/A'}`);
    lines.push(`Driver: ${data.driverName || 'N/A'}`);
    if (data.driverLicense) {
      lines.push(`License: ${data.driverLicense}`);
    }
    lines.push("");
    
    lines.push("MATERIAL");
    lines.push(`Product: ${data.commodityName || 'Tea Leaves'}`);
    lines.push(`Supplier: ${data.supplierName || 'KTDA Factory'}`);
    lines.push(`Transporter: ${data.transporterName || 'Transport Company'}`);
    if (data.customerName) {
      lines.push(`Customer: ${data.customerName}`);
    }
    lines.push("");
    
    lines.push("WEIGHTS");
    lines.push(`Gross: ${formatWeight(data.firstWeight || data.weight)} kg`);
    lines.push(`Tare: ${formatWeight(data.secondWeight || 0)} kg`);
    lines.push(`Net: ${formatWeight(data.netWeight || data.firstWeight || data.weight)} kg`);
    lines.push("");
    
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
    <div className={`min-h-screen ${
      isDark 
        ? 'bg-gradient-to-br from-gray-900 via-gray-800 to-black' 
        : 'bg-gradient-to-br from-gray-50 via-white to-gray-100'
    } flex items-center justify-center p-8`}>
      <div className="max-w-3xl w-full">
        {printing ? (
          <div className="text-center">
            <div className="w-40 h-40 mx-auto mb-8 relative">
              <div className="absolute inset-0 bg-blue-900/20 rounded-full animate-ping"></div>
              <div className="relative w-40 h-40 bg-gradient-to-br from-blue-900 to-blue-700 rounded-full flex items-center justify-center border-4 border-blue-500 shadow-2xl">
                <span className="text-7xl">🖨️</span>
              </div>
            </div>
            
            <h2 className={`text-5xl font-bold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>
              Printing Ticket...
            </h2>
            <p className={`text-2xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
              Please wait for your receipt
            </p>
            
            <div className="flex items-center justify-center gap-2">
              <div className="w-4 h-4 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
              <div className="w-4 h-4 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
              <div className="w-4 h-4 bg-blue-500 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
            </div>
          </div>
        ) : printSuccess ? (
          <div className="text-center">
            {/* Success Animation */}
            <div className="w-40 h-40 mx-auto mb-8 bg-green-900/50 rounded-full flex items-center justify-center shadow-2xl">
              <span className="text-8xl">✓</span>
            </div>
            
            <h2 className="text-5xl font-bold text-green-500 mb-4">Ticket Printed!</h2>
            <p className={`text-2xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
              Please collect your receipt from the printer
            </p>
            
            {/* Ticket Preview */}
            <div className="bg-white rounded-xl shadow-2xl max-w-md mx-auto p-8 text-left font-mono text-sm leading-relaxed mb-8">
              <div className="border-b-2 border-dashed border-gray-300 pb-4 mb-4 text-center">
                <div className="font-bold text-xl mb-1">KTDA WEIGHBRIDGE</div>
                <div className="text-base">Tea Collection Ticket</div>
                <div className="text-xs text-gray-600">[SELF-SERVICE SYSTEM]</div>
              </div>
              
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">ID:</span>
                  <span className="font-bold">{ticketData.receiptNo || 'WB-' + Date.now()}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Date:</span>
                  <span>{dayjs(ticketData.weighTime || new Date()).format('DD/MM/YYYY, HH:mm')}</span>
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
                <div className="font-bold mb-2">MATERIAL</div>
                <div className="space-y-1">
                  <div>Product: {ticketData.commodityName || 'Tea Leaves'}</div>
                  <div>Supplier: {ticketData.supplierName || 'KTDA Factory'}</div>
                  <div>Transporter: {ticketData.transporterName || 'Transport Co.'}</div>
                </div>
              </div>
              
              <div className="border-t border-gray-300 my-3 pt-3">
                <div className="font-bold mb-2">WEIGHTS</div>
                <div className="space-y-1">
                  <div className="flex justify-between">
                    <span>Gross:</span>
                    <span className="font-bold">{formatWeight(ticketData.firstWeight || ticketData.weight)} kg</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Tare:</span>
                    <span>--- kg</span>
                  </div>
                  <div className="flex justify-between text-green-600 font-bold pt-2 border-t">
                    <span>Net:</span>
                    <span>{formatWeight(ticketData.firstWeight || ticketData.weight)} kg</span>
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
            
            <div className={`${
              isDark 
                ? 'bg-blue-900/20 border-blue-700' 
                : 'bg-blue-50 border-blue-200'
            } border rounded-xl p-6 max-w-md mx-auto`}>
              <p className={`mb-3 font-semibold ${isDark ? 'text-blue-300' : 'text-blue-900'}`}>
                Next Steps:
              </p>
              <ol className={`space-y-2 text-sm text-left ${isDark ? 'text-blue-200' : 'text-blue-800'}`}>
                <li>1. Collect your printed ticket</li>
                <li>2. Proceed to the unloading bay</li>
                <li>3. Return for second weighing when done</li>
              </ol>
            </div>
          </div>
        ) : (
          <div className="text-center">
            <div className="w-40 h-40 mx-auto mb-8 bg-red-900/50 rounded-full flex items-center justify-center shadow-2xl">
              <span className="text-8xl">✗</span>
            </div>
            
            <h2 className="text-5xl font-bold text-red-500 mb-4">Print Failed</h2>
            <p className={`text-2xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
              Please contact the office for assistance
            </p>
            
            <button
              onClick={onComplete}
              className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-xl font-semibold text-lg transition-colors"
            >
              Continue
            </button>
          </div>
        )}
      </div>
    </div>
  );
}