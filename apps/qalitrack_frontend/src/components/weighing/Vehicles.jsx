import { useEffect, useState } from "react";
import { Pencil, Trash2, Truck, Plus, Search, X } from "lucide-react";
import { useLicenseFeature } from "../../hooks/useLicenseFeature";
import { LicenseFeatures } from "../../utils/LicenseFeatures";
import { message } from "antd";
import {
  getVehicles,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  getVehicleByRfid,
  updateVehicleStatus,
} from "../../api/MasterData/Vehicles";
import { getOwners } from "../../api/MasterData/Owners";
import { getAxleConfigs } from "../../api/MasterData/AxleConfigs";

export default function Vehicles() {
  const rfidLicensed = useLicenseFeature(LicenseFeatures.RFID);
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [owners, setOwners] = useState([]);
  const [axleConfigs, setAxleConfigs] = useState([]);
  const [form, setForm] = useState({
    registrationNumber: "",
    type: "",
    make: "",
    model: "",
    yearOfManufacture: "",
    color: "",
    chassisNumber: "",
    engineNumber: "",
    status: "Active",
    vehicleClass: "",
    bodyType: "",
    grossWeight: "",
    tareWeight: "",
    netWeightCapacity: "",
    seatingCapacity: "",
    fuelTankCapacity: "",
    insurancePolicyNumber: "",
    insuranceExpiryDate: "",
    roadWorthinessNumber: "",
    roadWorthinessExpiryDate: "",
    supplierId: "",
    transporterId: "",
    ownerId: "",
    axleConfigurationId: "",
    driverIds: [],
    rfiDcode: "",
  });
  const [editingVehicle, setEditingVehicle] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [showAdvanced, setShowAdvanced] = useState(false);
  const [rfidSearchTerm, setRfidSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");

  const fetchVehicles = async () => {
    try {
      setLoading(true);
      
      const data = await getVehicles(pageNumber, pageSize, searchTerm);
      
      
      // ✅ Extract items from response - try all possible structures
      let items = data?.items || data?.data?.items || (Array.isArray(data?.data) ? data.data : []) || (Array.isArray(data) ? data : []);
      
      
      // ✅ Extract total items for pagination
      const totalItems = data?.totalItems || data?.data?.totalItems || items.length;
      const pages = Math.ceil(totalItems / pageSize);
      setTotalPages(pages);
      
      if (items.length > 0) {
        
        // ✅ If backend doesn't provide names, enrich with local data
        if (!items[0].ownerName || !items[0].axleConfigurationName) {
          
          items = items.map(vehicle => {
            const owner = owners.find(o => o.id === vehicle.ownerId);
            const axleConfig = axleConfigs.find(a => a.id === vehicle.axleConfigurationId);
            
            
            return {
              ...vehicle,
              ownerName: vehicle.ownerName || owner?.name || owner?.ownerName || null,
              axleConfigurationName: vehicle.axleConfigurationName || axleConfig?.code || axleConfig?.description || axleConfig?.name || null
            };
          });
        }
      }
      
      setVehicles(Array.isArray(items) ? items : []);
    } catch (error) {
      setVehicles([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchOwners = async () => {
    try {
      const data = await getOwners(1, 100, "");
      
      const items = data?.items || data || [];
      
      if (items.length > 0) {
      }
      
      setOwners(Array.isArray(items) ? items : []);
    } catch (error) {
      setOwners([]);
    }
  };

  const fetchAxleConfigs = async () => {
    try {
      const data = await getAxleConfigs(1, 100, "");
      
      const items = data?.items || data?.data?.items || data || [];
      
      setAxleConfigs(Array.isArray(items) ? items : []);
    } catch (error) {
      setAxleConfigs([]);
    }
  };

  useEffect(() => {
    fetchVehicles();
  }, [pageNumber, searchTerm]);

  useEffect(() => {
    fetchOwners();
    fetchAxleConfigs();
  }, []);

  // ✅ Re-enrich vehicles whenever owners or axleConfigs are loaded
  useEffect(() => {
    if (vehicles.length > 0 && (owners.length > 0 || axleConfigs.length > 0)) {
      const enrichedVehicles = vehicles.map(vehicle => {
        const owner = owners.find(o => o.id === vehicle.ownerId);
        const axleConfig = axleConfigs.find(a => a.id === vehicle.axleConfigurationId);
        
        return {
          ...vehicle,
          ownerName: vehicle.ownerName || owner?.name || owner?.ownerName || null,
          axleConfigurationName: vehicle.axleConfigurationName || axleConfig?.code || axleConfig?.description || axleConfig?.name || null
        };
      });
      setVehicles(enrichedVehicles);
    }
  }, [owners, axleConfigs]);

  const handleRfidSearch = async () => {
    if (!rfidSearchTerm.trim()) {
      message.warning("Please enter an RFID code to search");
      return;
    }

    try {
      setLoading(true);
      const data = await getVehicleByRfid(rfidSearchTerm.trim());
      
      // API returns a single vehicle, wrap in array for display
      setVehicles(data ? [data] : []);
      
      if (data) {
      } else {
        message.warning("No vehicle found with this RFID code");
      }
    } catch (error) {
      message.warning("No vehicle found with RFID: " + rfidSearchTerm);
      setVehicles([]);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type } = e.target;
    setForm((prev) => ({ 
      ...prev, 
      [name]: type === "number" ? (value === "" ? "" : Number(value)) : value 
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      
      // ✅ Prepare payload - OMIT null values instead of sending them
      const payload = {};
      
      // Required fields - always include
      payload.registrationNumber = form.registrationNumber;
      payload.type = form.type;
      payload.status = form.status ? (form.status.charAt(0).toUpperCase() + form.status.slice(1).toLowerCase()) : "Active";
      payload.ownerId = form.ownerId;
      payload.axleConfigurationId = form.axleConfigurationId;
      
      // Optional string fields - only include if not empty
      if (form.make?.trim()) payload.make = form.make;
      if (form.model?.trim()) payload.model = form.model;
      if (form.color?.trim()) payload.color = form.color;
      if (form.chassisNumber?.trim()) payload.chassisNumber = form.chassisNumber;
      if (form.engineNumber?.trim()) payload.engineNumber = form.engineNumber;
      if (form.vehicleClass?.trim()) payload.vehicleClass = form.vehicleClass;
      if (form.bodyType?.trim()) payload.bodyType = form.bodyType;
      if (form.insurancePolicyNumber?.trim()) payload.insurancePolicyNumber = form.insurancePolicyNumber;
      if (form.roadWorthinessNumber?.trim()) payload.roadWorthinessNumber = form.roadWorthinessNumber;
      if (form.supplierId?.trim()) payload.supplierId = form.supplierId;
      if (form.transporterId?.trim()) payload.transporterId = form.transporterId;
      
      // Optional numeric fields - only include if value exists and is valid
      if (form.yearOfManufacture && !isNaN(form.yearOfManufacture)) {
        payload.yearOfManufacture = Number(form.yearOfManufacture);
      }
      if (form.grossWeight && !isNaN(form.grossWeight)) {
        payload.grossWeight = Number(form.grossWeight);
      }
      if (form.tareWeight && !isNaN(form.tareWeight)) {
        payload.tareWeight = Number(form.tareWeight);
      }
      if (form.netWeightCapacity && !isNaN(form.netWeightCapacity)) {
        payload.netWeightCapacity = Number(form.netWeightCapacity);
      }
      if (form.seatingCapacity && !isNaN(form.seatingCapacity)) {
        payload.seatingCapacity = Number(form.seatingCapacity);
      }
      if (form.fuelTankCapacity && !isNaN(form.fuelTankCapacity)) {
        payload.fuelTankCapacity = Number(form.fuelTankCapacity);
      }
      
      // Date fields - only include if not empty
      if (form.insuranceExpiryDate) {
        payload.insuranceExpiryDate = form.insuranceExpiryDate;
      }
      if (form.roadWorthinessExpiryDate) {
        payload.roadWorthinessExpiryDate = form.roadWorthinessExpiryDate;
      }
      
      // Driver IDs - only include if array has items
      if (form.driverIds && form.driverIds.length > 0) {
        payload.driverIds = form.driverIds;
      }

      
      // Validate required fields before sending
      if (!payload.ownerId || payload.ownerId === "") {
        message.warning("Please select an Owner from the dropdown.");
        setLoading(false);
        return;
      }
      
      if (!payload.axleConfigurationId || payload.axleConfigurationId === "") {
        message.warning("Please select an Axle Configuration from the dropdown.");
        setLoading(false);
        return;
      }

      if (editingVehicle) {
        // ✅ UPDATE: Include rfiDcode if provided
        if (form.rfiDcode?.trim()) {
          payload.rfiDcode = form.rfiDcode;
        }
        await updateVehicle(editingVehicle.id, payload);
      } else {
        // ✅ CREATE: rfiDcode already excluded (not in payload)
        await createVehicle(payload);
      }
      resetForm();
      await fetchVehicles();
    } catch (error) {
      message.error("Error saving vehicle: " + error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (vehicle) => {
    setEditingVehicle(vehicle);
    setForm({
      registrationNumber: vehicle.registrationNumber || "",
      type: vehicle.type || "",
      make: vehicle.make || "",
      model: vehicle.model || "",
      yearOfManufacture: vehicle.yearOfManufacture || "",
      color: vehicle.color || "",
      chassisNumber: vehicle.chassisNumber || "",
      engineNumber: vehicle.engineNumber || "",
      status: vehicle.status ? (vehicle.status.charAt(0).toUpperCase() + vehicle.status.slice(1).toLowerCase()) : "Active",
      vehicleClass: vehicle.vehicleClass || "",
      bodyType: vehicle.bodyType || "",
      grossWeight: vehicle.grossWeight || "",
      tareWeight: vehicle.tareWeight || "",
      netWeightCapacity: vehicle.netWeightCapacity || "",
      seatingCapacity: vehicle.seatingCapacity || "",
      fuelTankCapacity: vehicle.fuelTankCapacity || "",
      insurancePolicyNumber: vehicle.insurancePolicyNumber || "",
      insuranceExpiryDate: vehicle.insuranceExpiryDate ? vehicle.insuranceExpiryDate.split('T')[0] : "",
      roadWorthinessNumber: vehicle.roadWorthinessNumber || "",
      roadWorthinessExpiryDate: vehicle.roadWorthinessExpiryDate ? vehicle.roadWorthinessExpiryDate.split('T')[0] : "",
      supplierId: vehicle.supplierId || "",
      transporterId: vehicle.transporterId || "",
      ownerId: vehicle.ownerId || "",
      axleConfigurationId: vehicle.axleConfigurationId || "",
      driverIds: vehicle.driverIds || [],
      rfiDcode: vehicle.rfiDcode || vehicle.nfCcode || "",
    });
    setShowAdvanced(true);
  };

  const handleDelete = async (id) => {
    if (!confirm("Are you sure you want to delete this vehicle?")) return;
    try {
      await deleteVehicle(id);
      await fetchVehicles();
    } catch (error) {
    }
  };

  const handleToggleStatus = async (vehicle) => {
    const newStatus = vehicle.status?.toLowerCase() === "active" ? "Inactive" : "Active";
    try {
      await updateVehicleStatus(vehicle.id, newStatus);
      await fetchVehicles();
    } catch (err) {
      message.error("Failed to update vehicle status.");
    }
  };

  const resetForm = () => {
    setForm({
      registrationNumber: "",
      type: "",
      make: "",
      model: "",
      yearOfManufacture: "",
      color: "",
      chassisNumber: "",
      engineNumber: "",
      status: "Active",
      vehicleClass: "",
      bodyType: "",
      grossWeight: "",
      tareWeight: "",
      netWeightCapacity: "",
      seatingCapacity: "",
      fuelTankCapacity: "",
      insurancePolicyNumber: "",
      insuranceExpiryDate: "",
      roadWorthinessNumber: "",
      roadWorthinessExpiryDate: "",
      supplierId: "",
      transporterId: "",
      ownerId: "",
      axleConfigurationId: "",
      driverIds: [],
      rfiDcode: "",
    });
    setEditingVehicle(null);
    setShowAdvanced(false);
  };

  return (
    <div className="h-full flex flex-col bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Compact Header */}
      <div className="px-3 py-2 bg-gradient-to-r from-amber-50 via-orange-50 to-amber-50 border-b border-amber-200">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-sm">
              <Truck className="w-4 h-4 text-white" />
            </div>
            <div>
              <span className="text-[11px] font-bold text-gray-900 block leading-tight">
                Vehicles
              </span>
              <span className="text-[9px] text-amber-700 font-medium">
                {vehicles.length} registered vehicles
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search size={12} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search by reg/type..."
                className="w-40 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 focus:border-amber-500 shadow-sm"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            {rfidLicensed && (
              <div className="relative flex gap-1">
                <input
                  type="text"
                  placeholder="RFID Code..."
                  className="w-32 h-7 px-2 text-[11px] rounded-md border border-gray-300 focus:border-amber-500 shadow-sm"
                  value={rfidSearchTerm}
                  onChange={(e) => setRfidSearchTerm(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleRfidSearch()}
                />
                <button
                  onClick={handleRfidSearch}
                  className="h-7 px-2 text-[11px] rounded-md bg-amber-100 hover:bg-amber-200 border-amber-300 text-amber-700 shadow-sm font-medium"
                  title="Search by RFID"
                >
                  🔍 RFID
                </button>
              </div>
            )}
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="h-7 px-2 text-[11px] rounded-md border border-gray-300 focus:border-amber-500 shadow-sm bg-white"
            >
              <option value="All">All Status</option>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
            <button
              onClick={() => {
                setSearchTerm("");
                setRfidSearchTerm("");
                setPageNumber(1);
                fetchVehicles();
              }}
              className="h-7 px-3 text-[11px] rounded-md border-gray-300 hover:border-amber-500 hover:text-amber-600 shadow-sm font-medium bg-white"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="px-3 py-2 bg-gradient-to-r from-gray-50 to-amber-50/30 border-b border-amber-200 shadow-sm max-h-[50vh] overflow-y-auto">
        {/* Info Banner */}
        {/* <div className="mb-2 p-2 bg-blue-50 border border-blue-200 rounded text-[10px] text-blue-800">
          <strong>ℹ️ Required:</strong> Select an Owner and Axle Configuration from the dropdowns. 
          If you don't see the options you need, add them in the Owners and Axle Configurations sections first.
          <br/>
          <strong>📝 Note:</strong> RFID codes can only be assigned when <em>updating</em> a vehicle, not during initial creation.
        </div> */}
        
        <form onSubmit={handleSubmit} className="space-y-3">
          {/* Basic Information */}
          <div>
            <div className="flex justify-between items-center mb-2">
              <h3 className="text-[10px] font-bold text-amber-900 uppercase">Basic Information</h3>
              <button
                type="button"
                onClick={() => setShowAdvanced(!showAdvanced)}
                className="text-[10px] text-amber-600 hover:text-amber-800 font-semibold"
              >
                {showAdvanced ? "Hide" : "Show"} Advanced Fields
              </button>
            </div>
            
            <div className="grid grid-cols-4 gap-2">
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Registration Number *
                </label>
                <input
                  name="registrationNumber"
                  value={form.registrationNumber}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  required
                  placeholder="e.g., KXX 123Y"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Type *
                </label>
                <input
                  name="type"
                  value={form.type}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  required
                  placeholder="e.g., Truck, Van"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Make
                </label>
                <input
                  name="make"
                  value={form.make}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  placeholder="e.g., Isuzu"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Model
                </label>
                <input
                  name="model"
                  value={form.model}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  placeholder="e.g., FRR"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Year
                </label>
                <input
                  type="number"
                  name="yearOfManufacture"
                  value={form.yearOfManufacture}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  placeholder="e.g., 2020"
                  min="1900"
                  max="2100"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Color
                </label>
                <input
                  name="color"
                  value={form.color}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  placeholder="e.g., White"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Status
                </label>
                <select
                  name="status"
                  value={form.status}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                >
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Body Type
                </label>
                <input
                  name="bodyType"
                  value={form.bodyType}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  placeholder="e.g., Flatbed"
                />
              </div>

              {/* Required fields moved to basic form */}
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Owner *
                </label>
                <select
                  name="ownerId"
                  value={form.ownerId}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  required
                >
                  <option value="">-- Select Owner --</option>
                  {Array.isArray(owners) && owners.map((owner) => (
                    <option key={owner.id} value={owner.id}>
                      {owner.name || owner.ownerName || `Owner ${owner.id.substring(0, 8)}`}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Axle Configuration *
                </label>
                <select
                  name="axleConfigurationId"
                  value={form.axleConfigurationId}
                  onChange={handleChange}
                  className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                  required
                >
                  <option value="">-- Select Axle Config --</option>
                  {Array.isArray(axleConfigs) && axleConfigs.map((config) => (
                    <option key={config.id} value={config.id}>
                      {config.code || config.description || config.name || `Config ${config.id?.substring(0, 6)}`}
                    </option>
                  ))}
                </select>
              </div>

              {rfidLicensed && (
                <div>
                  <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                    RFID Code {editingVehicle && <span className="text-[9px] text-blue-600">(Update only)</span>}
                  </label>
                  <input
                    name="rfiDcode"
                    value={form.rfiDcode}
                    onChange={handleChange}
                    className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200 disabled:bg-gray-100 disabled:cursor-not-allowed"
                    placeholder={editingVehicle ? "RFID/NFC Code" : "Set after creation"}
                    disabled={!editingVehicle}
                    title={editingVehicle ? "Edit RFID code" : "RFID can only be set when updating a vehicle"}
                  />
                </div>
              )}
            </div>
          </div>

          {/* Advanced Fields - Collapsible */}
          {showAdvanced && (
            <>
              {/* Vehicle Specifications */}
              <div>
                <h3 className="text-[10px] font-bold text-amber-900 uppercase mb-2">Specifications</h3>
                <div className="grid grid-cols-4 gap-2">
                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Chassis Number
                    </label>
                    <input
                      name="chassisNumber"
                      value={form.chassisNumber}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Chassis #"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Engine Number
                    </label>
                    <input
                      name="engineNumber"
                      value={form.engineNumber}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Engine #"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Vehicle Class
                    </label>
                    <input
                      name="vehicleClass"
                      value={form.vehicleClass}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Class"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Seating Capacity
                    </label>
                    <input
                      type="number"
                      name="seatingCapacity"
                      value={form.seatingCapacity}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Seats"
                      min="0"
                    />
                  </div>
                </div>
              </div>

              {/* Weight Information */}
              <div>
                <h3 className="text-[10px] font-bold text-amber-900 uppercase mb-2">Weight Information (kg)</h3>
                <div className="grid grid-cols-4 gap-2">
                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Gross Weight
                    </label>
                    <input
                      type="number"
                      name="grossWeight"
                      value={form.grossWeight}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="kg"
                      min="0"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Tare Weight
                    </label>
                    <input
                      type="number"
                      name="tareWeight"
                      value={form.tareWeight}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="kg"
                      min="0"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Net Capacity
                    </label>
                    <input
                      type="number"
                      name="netWeightCapacity"
                      value={form.netWeightCapacity}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="kg"
                      min="0"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Fuel Tank (L)
                    </label>
                    <input
                      type="number"
                      name="fuelTankCapacity"
                      value={form.fuelTankCapacity}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Liters"
                      min="0"
                    />
                  </div>
                </div>
              </div>

              {/* Insurance & Compliance */}
              <div>
                <h3 className="text-[10px] font-bold text-amber-900 uppercase mb-2">Insurance & Compliance</h3>
                <div className="grid grid-cols-4 gap-2">
                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Insurance Policy #
                    </label>
                    <input
                      name="insurancePolicyNumber"
                      value={form.insurancePolicyNumber}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Policy Number"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Insurance Expiry
                    </label>
                    <input
                      type="date"
                      name="insuranceExpiryDate"
                      value={form.insuranceExpiryDate}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Road Worthiness #
                    </label>
                    <input
                      name="roadWorthinessNumber"
                      value={form.roadWorthinessNumber}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Certificate #"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Worthiness Expiry
                    </label>
                    <input
                      type="date"
                      name="roadWorthinessExpiryDate"
                      value={form.roadWorthinessExpiryDate}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                    />
                  </div>
                </div>
              </div>

              {/* Relationships */}
              <div>
                <h3 className="text-[10px] font-bold text-amber-900 uppercase mb-2">Optional Relationships</h3>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Supplier ID
                    </label>
                    <input
                      name="supplierId"
                      value={form.supplierId}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Supplier ID"
                    />
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Transporter ID
                    </label>
                    <input
                      name="transporterId"
                      value={form.transporterId}
                      onChange={handleChange}
                      className="w-full h-7 text-[11px] rounded border-amber-300 px-2 focus:border-amber-500 focus:ring-1 focus:ring-amber-200"
                      placeholder="Transporter ID"
                    />
                  </div>
                </div>
              </div>
            </>
          )}

          {/* Submit Buttons */}
          <div className="flex gap-2 justify-end pt-2 border-t border-amber-200">
            {editingVehicle && (
              <button
                type="button"
                onClick={resetForm}
                className="h-7 px-3 text-[11px] font-semibold bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition-all flex items-center gap-1"
              >
                <X className="w-3 h-3" />
                Cancel
              </button>
            )}
            <button
              type="submit"
              disabled={loading}
              className="h-7 px-3 text-[11px] font-semibold bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded shadow transition-all flex items-center gap-1 disabled:opacity-50"
            >
              <Plus className="w-3 h-3" />
              {editingVehicle ? "Update" : "Add"} Vehicle
            </button>
          </div>
        </form>
      </div>

      {/* Table Section */}
      <div className="flex-1 overflow-auto bg-white">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">Loading vehicles...</p>
          </div>
        ) : vehicles.length === 0 ? (
          <div className="flex items-center justify-center h-full">
            <p className="text-gray-500 text-sm">No vehicles found.</p>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-orange-50 border-b-2 border-amber-200">
              <tr>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">#</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Reg. Number</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Type</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Make/Model</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Owner</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Axle Config</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">RFID</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-left uppercase tracking-wide">Status</th>
                <th className="px-3 py-2 text-[9px] font-bold text-amber-900 text-center uppercase tracking-wide">Actions</th>
              </tr>
            </thead>
            <tbody>
              {vehicles.filter(v => statusFilter === "All" || v.status?.toLowerCase() === statusFilter.toLowerCase()).map((v, index) => (
                <tr
                  key={v.id}
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-3 py-2 text-[10px] text-gray-500 font-semibold">
                    {(pageNumber - 1) * pageSize + index + 1}
                  </td>
                  <td className="px-3 py-2">
                    <div className="inline-block bg-gray-900 text-white px-2 py-0.5 rounded text-[10px] font-bold tracking-wider">
                      {v.registrationNumber}
                    </div>
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-700 font-medium">{v.type}</td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">
                    {v.make && v.model ? `${v.make} ${v.model}` : v.make || v.model || "-"}
                  </td>
                  <td className="px-3 py-2 text-[10px] text-gray-600">
                    {v.ownerName || "-"}
                  </td>
                  <td className="px-3 py-2">
                    {v.axleConfigurationName ? (
                      <div className="inline-block bg-blue-100 text-blue-800 px-2 py-0.5 rounded text-[10px] font-semibold border border-blue-300">
                        {v.axleConfigurationName}
                      </div>
                    ) : (
                      <span className="text-gray-400">-</span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    {v.nfCcode || v.rfiDcode ? (
                      <div className="inline-block bg-purple-100 text-purple-800 px-2 py-0.5 rounded text-[9px] font-mono border border-purple-300">
                        {v.nfCcode || v.rfiDcode}
                      </div>
                    ) : (
                      <span className="text-gray-400 text-[9px]">Not set</span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    <span
                      className={`px-2 py-0.5 rounded-full text-[9px] font-semibold uppercase ${
                        v.status?.toLowerCase() === "active"
                          ? "bg-green-100 text-green-700 border border-green-300"
                          : "bg-red-100 text-red-700 border border-red-300"
                      }`}
                    >
                      {v.status?.toLowerCase() === "active" ? "✓ Active" : "✕ Inactive"}
                    </span>
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
                      <button
                        onClick={() => handleToggleStatus(v)}
                        className={`p-1 rounded border text-[9px] font-semibold transition-all ${
                          v.status?.toLowerCase() === "active"
                            ? "text-green-700 border-green-300 hover:bg-green-50"
                            : "text-red-700 border-red-300 hover:bg-red-50"
                        }`}
                        title={v.status?.toLowerCase() === "active" ? "Set Inactive" : "Set Active"}
                      >
                        {v.status?.toLowerCase() === "active" ? "✓" : "✕"}
                      </button>
                      <button
                        onClick={() => handleEdit(v)}
                        className="p-1 rounded text-amber-600 hover:bg-amber-50 border border-amber-300 hover:border-amber-500 transition-all"
                        title="Edit"
                      >
                        <Pencil className="w-3 h-3" />
                      </button>
                      <button
                        onClick={() => handleDelete(v.id)}
                        className="p-1 rounded text-red-600 hover:bg-red-50 border border-red-300 hover:border-red-500 transition-all"
                        title="Delete"
                      >
                        <Trash2 className="w-3 h-3" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Footer with Pagination */}
      <div className="px-3 py-2 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
        <span className="text-[10px] text-gray-600 font-medium">
          Page <span className="font-semibold text-amber-600">{pageNumber}</span> of{" "}
          <span className="font-semibold text-amber-600">{totalPages}</span>
          {" • "}
          <span className="font-semibold text-amber-600">{vehicles.length}</span> vehicles on this page
        </span>
        <div className="flex gap-2">
          <button
            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
            disabled={pageNumber === 1}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Previous
          </button>
          <button
            onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
            disabled={pageNumber === totalPages}
            className="h-6 px-2 text-[10px] font-semibold border border-gray-300 rounded disabled:opacity-40 disabled:cursor-not-allowed hover:bg-amber-50 hover:border-amber-500 transition-all"
          >
            Next
          </button>
        </div>
      </div>

      <style>{`
        .compact-table {
          font-size: 10px;
        }
        .compact-table thead tr th {
          padding: 6px 12px;
          font-weight: 700;
          font-size: 9px;
          line-height: 1.2;
        }
        .compact-table tbody tr td {
          padding: 6px 12px;
          line-height: 1.3;
        }
      `}</style>
    </div>
  );
}