import { useEffect, useState } from "react";
import { Pencil, Trash2, Truck, Plus, Search, X, Users } from "lucide-react";
import { useLicenseFeature } from "../../hooks/useLicenseFeature";
import { LicenseFeatures } from "../../utils/LicenseFeatures";
import { message, Modal, Select, Switch } from "antd";
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
import TablePagination from "../TablePagination";
import { getSuppliers } from "../../api/MasterData/Suppliers";
import { getTransporters } from "../../api/MasterData/Transporters";
import { getDrivers, assignDriverToVehicle, unassignDriverFromVehicle } from "../../api/MasterData/Drivers";

// Axle config codes follow the truck-industry "NxM" convention (e.g. "8x4" =
// 8 wheels total, 4 driven) — the leading number is the wheel count. Spelling
// that out next to the code lets non-technical staff pick the right one
// without having to already know what "8x4" means.
const getWheelCount = (code) => {
  const match = code?.match(/^(\d+)/);
  return match ? parseInt(match[1], 10) : null;
};

export default function Vehicles() {
  const rfidLicensed = useLicenseFeature(LicenseFeatures.RFID);
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [owners, setOwners] = useState([]);
  const [axleConfigs, setAxleConfigs] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [transporters, setTransporters] = useState([]);
  const [driversList, setDriversList] = useState([]);
  const [assignModalVehicle, setAssignModalVehicle] = useState(null);
  const [driverToAssign, setDriverToAssign] = useState("");
  const [assignBusy, setAssignBusy] = useState(false);
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

  const fetchSuppliers = async () => {
    try {
      const data = await getSuppliers(1, 200, "");
      const items = Array.isArray(data) ? data : data?.items || [];
      setSuppliers(items);
    } catch (error) {
      setSuppliers([]);
    }
  };

  const fetchTransporters = async () => {
    try {
      const data = await getTransporters({ pageNumber: 1, pageSize: 200 });
      setTransporters(Array.isArray(data?.items) ? data.items : []);
    } catch (error) {
      setTransporters([]);
    }
  };

  const fetchDriversList = async () => {
    try {
      const data = await getDrivers({ pageNumber: 1, pageSize: 500 });
      const items = data?.data?.items || data?.items || [];
      setDriversList(Array.isArray(items) ? items : []);
    } catch (error) {
      setDriversList([]);
    }
  };

  useEffect(() => {
    fetchVehicles();
  }, [pageNumber, searchTerm]);

  useEffect(() => {
    fetchOwners();
    fetchAxleConfigs();
    fetchSuppliers();
    fetchTransporters();
    fetchDriversList();
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

  const handleDelete = (id) => {
    Modal.confirm({
      title: "Are you sure you want to delete this vehicle?",
      okText: "Delete",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteVehicle(id);
          await fetchVehicles();
        } catch (err) {
          message.error(err.message || "Failed to delete vehicle");
        }
      },
    });
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
      {/* Compact Header — navy app-bar (Navy-theme experiment, see Transaction.jsx) */}
      <div className="px-3 py-2" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-md cs-icon-box flex items-center justify-center shadow-sm">
              <Truck className="w-4 h-4" style={{ color: "var(--cs-icon-accent)" }} />
            </div>
            <div>
              <span className="text-[11px] font-bold block leading-tight" style={{ color: "var(--cs-appbar-text)" }}>
                Vehicles
              </span>
              <span className="text-[9px] font-medium" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
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
                className="qt-filter-field w-40 h-7 pl-8 pr-3 text-[11px] rounded-md border border-gray-300 shadow-sm"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            {rfidLicensed && (
              <div className="relative flex gap-1">
                <input
                  type="text"
                  placeholder="RFID Code..."
                  className="qt-filter-field w-32 h-7 px-2 text-[11px] rounded-md border border-gray-300 shadow-sm"
                  value={rfidSearchTerm}
                  onChange={(e) => setRfidSearchTerm(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleRfidSearch()}
                />
                <button
                  onClick={handleRfidSearch}
                  className="h-7 px-2 text-[11px] rounded-md cs-ghost-btn shadow-sm font-medium"
                  title="Search by RFID"
                >
                  🔍 RFID
                </button>
              </div>
            )}
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="qt-filter-field h-7 px-2 text-[11px] rounded-md cs-ghost-btn shadow-sm"
            >
              <option value="All" className="text-black">All Status</option>
              <option value="Active" className="text-black">Active</option>
              <option value="Inactive" className="text-black">Inactive</option>
            </select>
            <button
              onClick={() => {
                setSearchTerm("");
                setRfidSearchTerm("");
                setPageNumber(1);
                fetchVehicles();
              }}
              className="h-7 px-3 text-[11px] rounded-md cs-solid-chip-btn shadow-sm font-medium"
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
                  Registration Number <span style={{ color: "var(--cs-required)" }}>*</span>
                </label>
                <input
                  name="registrationNumber"
                  value={form.registrationNumber}
                  onChange={handleChange}
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                  required
                  placeholder="e.g., KXX 123Y"
                />
              </div>

              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Type <span style={{ color: "var(--cs-required)" }}>*</span>
                </label>
                <input
                  name="type"
                  value={form.type}
                  onChange={handleChange}
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                  placeholder="e.g., Flatbed"
                />
              </div>

              {/* Required fields moved to basic form */}
              <div>
                <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                  Owner <span style={{ color: "var(--cs-required)" }}>*</span>
                </label>
                <select
                  name="ownerId"
                  value={form.ownerId}
                  onChange={handleChange}
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                  Axle Configuration <span style={{ color: "var(--cs-required)" }}>*</span>
                </label>
                <select
                  name="axleConfigurationId"
                  value={form.axleConfigurationId}
                  onChange={handleChange}
                  className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                  required
                >
                  <option value="">-- Select Axle Config --</option>
                  {Array.isArray(axleConfigs) && axleConfigs
                    .filter((config) => config.isActive !== false || config.id === form.axleConfigurationId)
                    .map((config) => {
                    const label = config.code || config.description || config.name || `Config ${config.id?.substring(0, 6)}`;
                    const wheels = getWheelCount(config.code);
                    return (
                      <option key={config.id} value={config.id}>
                        {label}{wheels ? ` — ${wheels} wheels` : ""}{config.isActive === false ? " (inactive)" : ""}
                      </option>
                    );
                  })}
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
                    className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2 disabled:bg-gray-100 disabled:cursor-not-allowed"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
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
                      Supplier
                    </label>
                    <select
                      name="supplierId"
                      value={form.supplierId}
                      onChange={handleChange}
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                    >
                      <option value="">-- None --</option>
                      {Array.isArray(suppliers) && suppliers.map((s) => (
                        <option key={s.id} value={s.id}>
                          {s.name || `Supplier ${s.id.substring(0, 8)}`}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="text-[10px] font-semibold text-gray-700 mb-1 block">
                      Transporter
                    </label>
                    <select
                      name="transporterId"
                      value={form.transporterId}
                      onChange={handleChange}
                      className="qt-filter-field w-full h-7 text-[11px] rounded border border-gray-300 px-2"
                    >
                      <option value="">-- None --</option>
                      {Array.isArray(transporters) && transporters.map((t) => (
                        <option key={t.id} value={t.id}>
                          {t.name || `Transporter ${t.id.substring(0, 8)}`}
                        </option>
                      ))}
                    </select>
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
              className="h-7 px-3 text-[11px] font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow transition-all flex items-center gap-1 disabled:opacity-50"
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
            <div className="text-center">
              <Truck className="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p className="text-gray-500 text-sm">No vehicles found.</p>
              <p className="text-gray-400 text-xs mt-1">Add a vehicle using the form above</p>
            </div>
          </div>
        ) : (
          <table className="w-full compact-table">
            <thead className="sticky top-0 bg-gradient-to-b from-amber-50 to-amber-50 border-b-2 border-amber-200">
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
                  className={`border-b border-gray-100 hover:bg-gradient-to-r hover:from-amber-50 hover:to-amber-50 transition-all ${
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
                        {getWheelCount(v.axleConfigurationName) ? ` (${getWheelCount(v.axleConfigurationName)} wheels)` : ""}
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
                  <td className="px-3 py-2 text-center">
                    <Switch
                      checked={v.status?.toLowerCase() === "active"}
                      onChange={() => handleToggleStatus(v)}
                      size="small"
                    />
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1 justify-center">
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

        {/* Footer with Pagination — inside the scroll area so it sits immediately after the table instead of pinned to the bottom of the page */}
        <TablePagination
          page={pageNumber}
          totalPages={totalPages}
          onPageChange={setPageNumber}
          itemCount={vehicles.length}
          itemLabel="vehicles on this page"
        />
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