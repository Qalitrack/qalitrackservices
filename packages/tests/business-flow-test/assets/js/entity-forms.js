/**
 * QaliTrack Entity Forms Handler
 * Handles form operations and table management for individual entities
 */

// Configuration for API endpoints
const API_CONFIG = {
    masterDataUrl: 'http://localhost:5004',
    dataManagerUrl: 'http://localhost:5005'
};

// Helper function to make API calls
async function apiCall(method, endpoint, data = null, showResponse = true) {
    const url = API_CONFIG.masterDataUrl + endpoint;
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        }
    };

    if (data) {
        options.body = JSON.stringify(data);
    }

    try {
        const response = await fetch(url, options);
        const responseData = await response.text();
        
        let result;
        try {
            result = JSON.parse(responseData);
        } catch (e) {
            result = responseData;
        }

        if (showResponse) {
            console.log(`${method} ${endpoint}:`, response.status, result);
        }

        return { ok: response.ok, status: response.status, data: result };
    } catch (error) {
        console.error(`Error with ${method} ${endpoint}:`, error);
        return { ok: false, error: error.message };
    }
}

// Location Types Functions
async function createSingleLocationType() {
    const data = {
        name: document.getElementById('locationTypeName').value,
        code: document.getElementById('locationTypeCode').value,
        description: document.getElementById('locationTypeDescription').value || null
    };
    
    if (!data.name || !data.code) {
        alert('Name and Code are required');
        return;
    }
    
    const result = await apiCall('POST', '/location-types', data);
    if (result.ok) {
        clearLocationTypeForm();
        loadLocationTypes();
        updateResponseArea('response-location-types', 'Location Type created successfully', true);
    } else {
        updateResponseArea('response-location-types', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

async function updateSingleLocationType() {
    const id = document.getElementById('locationTypeId').value;
    if (!id) {
        alert('Please enter an ID for update');
        return;
    }
    
    const data = {
        name: document.getElementById('locationTypeName').value,
        code: document.getElementById('locationTypeCode').value,
        description: document.getElementById('locationTypeDescription').value || null
    };
    
    const result = await apiCall('PUT', `/location-types/${id}`, data);
    if (result.ok) {
        clearLocationTypeForm();
        loadLocationTypes();
        updateResponseArea('response-location-types', 'Location Type updated successfully', true);
    } else {
        updateResponseArea('response-location-types', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

function clearLocationTypeForm() {
    document.getElementById('locationTypeId').value = '';
    document.getElementById('locationTypeName').value = '';
    document.getElementById('locationTypeCode').value = '';
    document.getElementById('locationTypeDescription').value = '';
}

async function loadLocationTypes() {
    const result = await apiCall('GET', '/location-types');
    if (result.ok && result.data) {
        populateLocationTypesTable(result.data);
        updateResponseArea('response-location-types', `Loaded ${result.data.length} location types`, true);
    } else {
        updateResponseArea('response-location-types', `Error loading data: ${result.error || result.data}`, false);
    }
}

function populateLocationTypesTable(data) {
    const tbody = document.getElementById('locationTypesTableBody');
    
    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="padding: 20px; text-align: center; color: #6c757d;">No location types found</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 10px;">${item.id}</td>
            <td style="padding: 10px;">${item.name}</td>
            <td style="padding: 10px;">${item.code}</td>
            <td style="padding: 10px;">${item.description || ''}</td>
            <td style="padding: 10px;">
                <button class="btn btn-load" onclick="editLocationType('${item.id}', '${item.name}', '${item.code}', '${item.description || ''}')" style="margin-right: 5px; padding: 3px 8px; font-size: 11px;">✏️ Edit</button>
                <button class="btn btn-danger" onclick="deleteLocationType('${item.id}')" style="padding: 3px 8px; font-size: 11px; background: #dc3545; color: white;">🗑️ Delete</button>
            </td>
        </tr>
    `).join('');
}

function editLocationType(id, name, code, description) {
    document.getElementById('locationTypeId').value = id;
    document.getElementById('locationTypeName').value = name;
    document.getElementById('locationTypeCode').value = code;
    document.getElementById('locationTypeDescription').value = description;
}

async function deleteLocationType(id) {
    if (confirm('Are you sure you want to delete this location type?')) {
        const result = await apiCall('DELETE', `/location-types/${id}`);
        if (result.ok) {
            loadLocationTypes();
            updateResponseArea('response-location-types', 'Location Type deleted successfully', true);
        } else {
            updateResponseArea('response-location-types', `Error deleting: ${JSON.stringify(result.data)}`, false);
        }
    }
}

// Zone Functions
async function createSingleZone() {
    const data = {
        name: document.getElementById('zoneName').value,
        code: document.getElementById('zoneCode').value,
        description: document.getElementById('zoneDescription').value || null
    };
    
    if (!data.name || !data.code) {
        alert('Name and Code are required');
        return;
    }
    
    const result = await apiCall('POST', '/zones', data);
    if (result.ok) {
        clearZoneForm();
        loadZones();
        updateResponseArea('response-zones', 'Zone created successfully', true);
    } else {
        updateResponseArea('response-zones', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

async function updateSingleZone() {
    const id = document.getElementById('zoneId').value;
    if (!id) {
        alert('Please enter an ID for update');
        return;
    }
    
    const data = {
        name: document.getElementById('zoneName').value,
        code: document.getElementById('zoneCode').value,
        description: document.getElementById('zoneDescription').value || null
    };
    
    const result = await apiCall('PUT', `/zones/${id}`, data);
    if (result.ok) {
        clearZoneForm();
        loadZones();
        updateResponseArea('response-zones', 'Zone updated successfully', true);
    } else {
        updateResponseArea('response-zones', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

function clearZoneForm() {
    document.getElementById('zoneId').value = '';
    document.getElementById('zoneName').value = '';
    document.getElementById('zoneCode').value = '';
    document.getElementById('zoneDescription').value = '';
}

async function loadZones() {
    const result = await apiCall('GET', '/zones');
    if (result.ok && result.data) {
        populateZonesTable(result.data);
        updateResponseArea('response-zones', `Loaded ${result.data.length} zones`, true);
    } else {
        updateResponseArea('response-zones', `Error loading data: ${result.error || result.data}`, false);
    }
}

function populateZonesTable(data) {
    const tbody = document.getElementById('zonesTableBody');
    
    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="padding: 20px; text-align: center; color: #6c757d;">No zones found</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 10px;">${item.id}</td>
            <td style="padding: 10px;">${item.name}</td>
            <td style="padding: 10px;">${item.code}</td>
            <td style="padding: 10px;">${item.description || ''}</td>
            <td style="padding: 10px;">
                <button class="btn btn-load" onclick="editZone('${item.id}', '${item.name}', '${item.code}', '${item.description || ''}')" style="margin-right: 5px; padding: 3px 8px; font-size: 11px;">✏️ Edit</button>
                <button class="btn btn-danger" onclick="deleteZone('${item.id}')" style="padding: 3px 8px; font-size: 11px; background: #dc3545; color: white;">🗑️ Delete</button>
            </td>
        </tr>
    `).join('');
}

function editZone(id, name, code, description) {
    document.getElementById('zoneId').value = id;
    document.getElementById('zoneName').value = name;
    document.getElementById('zoneCode').value = code;
    document.getElementById('zoneDescription').value = description;
}

async function deleteZone(id) {
    if (confirm('Are you sure you want to delete this zone?')) {
        const result = await apiCall('DELETE', `/zones/${id}`);
        if (result.ok) {
            loadZones();
            updateResponseArea('response-zones', 'Zone deleted successfully', true);
        } else {
            updateResponseArea('response-zones', `Error deleting: ${JSON.stringify(result.data)}`, false);
        }
    }
}

// Business Entity Functions
async function createSingleBusinessEntity() {
    const data = {
        name: document.getElementById('businessEntityName').value,
        code: document.getElementById('businessEntityCode').value,
        type: document.getElementById('businessEntityType').value || null,
        contactInfo: document.getElementById('businessEntityContact').value || null
    };
    
    if (!data.name || !data.code) {
        alert('Name and Code are required');
        return;
    }
    
    const result = await apiCall('POST', '/business-entities', data);
    if (result.ok) {
        clearBusinessEntityForm();
        loadBusinessEntities();
        updateResponseArea('response-business-entities', 'Business Entity created successfully', true);
    } else {
        updateResponseArea('response-business-entities', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

async function updateSingleBusinessEntity() {
    const id = document.getElementById('businessEntityId').value;
    if (!id) {
        alert('Please enter an ID for update');
        return;
    }
    
    const data = {
        name: document.getElementById('businessEntityName').value,
        code: document.getElementById('businessEntityCode').value,
        type: document.getElementById('businessEntityType').value || null,
        contactInfo: document.getElementById('businessEntityContact').value || null
    };
    
    const result = await apiCall('PUT', `/business-entities/${id}`, data);
    if (result.ok) {
        clearBusinessEntityForm();
        loadBusinessEntities();
        updateResponseArea('response-business-entities', 'Business Entity updated successfully', true);
    } else {
        updateResponseArea('response-business-entities', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

function clearBusinessEntityForm() {
    document.getElementById('businessEntityId').value = '';
    document.getElementById('businessEntityName').value = '';
    document.getElementById('businessEntityCode').value = '';
    document.getElementById('businessEntityType').value = '';
    document.getElementById('businessEntityContact').value = '';
}

async function loadBusinessEntities() {
    const result = await apiCall('GET', '/business-entities');
    if (result.ok && result.data) {
        populateBusinessEntitiesTable(result.data);
        updateResponseArea('response-business-entities', `Loaded ${result.data.length} business entities`, true);
    } else {
        updateResponseArea('response-business-entities', `Error loading data: ${result.error || result.data}`, false);
    }
}

function populateBusinessEntitiesTable(data) {
    const tbody = document.getElementById('businessEntitiesTableBody');
    
    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="padding: 20px; text-align: center; color: #6c757d;">No business entities found</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 10px;">${item.id}</td>
            <td style="padding: 10px;">${item.name}</td>
            <td style="padding: 10px;">${item.code}</td>
            <td style="padding: 10px;">${item.type || ''}</td>
            <td style="padding: 10px;">${item.contactInfo || ''}</td>
            <td style="padding: 10px;">
                <button class="btn btn-load" onclick="editBusinessEntity('${item.id}', '${item.name}', '${item.code}', '${item.type || ''}', '${item.contactInfo || ''}')" style="margin-right: 5px; padding: 3px 8px; font-size: 11px;">✏️ Edit</button>
                <button class="btn btn-danger" onclick="deleteBusinessEntity('${item.id}')" style="padding: 3px 8px; font-size: 11px; background: #dc3545; color: white;">🗑️ Delete</button>
            </td>
        </tr>
    `).join('');
}

function editBusinessEntity(id, name, code, type, contactInfo) {
    document.getElementById('businessEntityId').value = id;
    document.getElementById('businessEntityName').value = name;
    document.getElementById('businessEntityCode').value = code;
    document.getElementById('businessEntityType').value = type;
    document.getElementById('businessEntityContact').value = contactInfo;
}

async function deleteBusinessEntity(id) {
    if (confirm('Are you sure you want to delete this business entity?')) {
        const result = await apiCall('DELETE', `/business-entities/${id}`);
        if (result.ok) {
            loadBusinessEntities();
            updateResponseArea('response-business-entities', 'Business Entity deleted successfully', true);
        } else {
            updateResponseArea('response-business-entities', `Error deleting: ${JSON.stringify(result.data)}`, false);
        }
    }
}

// Vehicle Functions
async function createSingleVehicle() {
    const data = {
        registrationNumber: document.getElementById('vehicleRegistration').value,
        make: document.getElementById('vehicleMake').value,
        model: document.getElementById('vehicleModel').value,
        capacity: parseFloat(document.getElementById('vehicleCapacity').value) || null
    };
    
    if (!data.registrationNumber || !data.make || !data.model) {
        alert('Registration Number, Make, and Model are required');
        return;
    }
    
    const result = await apiCall('POST', '/vehicles', data);
    if (result.ok) {
        clearVehicleForm();
        loadVehicles();
        updateResponseArea('response-vehicles', 'Vehicle created successfully', true);
    } else {
        updateResponseArea('response-vehicles', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

async function updateSingleVehicle() {
    const id = document.getElementById('vehicleId').value;
    if (!id) {
        alert('Please enter an ID for update');
        return;
    }
    
    const data = {
        registrationNumber: document.getElementById('vehicleRegistration').value,
        make: document.getElementById('vehicleMake').value,
        model: document.getElementById('vehicleModel').value,
        capacity: parseFloat(document.getElementById('vehicleCapacity').value) || null
    };
    
    const result = await apiCall('PUT', `/vehicles/${id}`, data);
    if (result.ok) {
        clearVehicleForm();
        loadVehicles();
        updateResponseArea('response-vehicles', 'Vehicle updated successfully', true);
    } else {
        updateResponseArea('response-vehicles', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

function clearVehicleForm() {
    document.getElementById('vehicleId').value = '';
    document.getElementById('vehicleRegistration').value = '';
    document.getElementById('vehicleMake').value = '';
    document.getElementById('vehicleModel').value = '';
    document.getElementById('vehicleCapacity').value = '';
}

async function loadVehicles() {
    const result = await apiCall('GET', '/vehicles');
    if (result.ok && result.data) {
        populateVehiclesTable(result.data);
        updateResponseArea('response-vehicles', `Loaded ${result.data.length} vehicles`, true);
    } else {
        updateResponseArea('response-vehicles', `Error loading data: ${result.error || result.data}`, false);
    }
}

function populateVehiclesTable(data) {
    const tbody = document.getElementById('vehiclesTableBody');
    
    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="padding: 20px; text-align: center; color: #6c757d;">No vehicles found</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 10px;">${item.id}</td>
            <td style="padding: 10px;">${item.registrationNumber}</td>
            <td style="padding: 10px;">${item.make}</td>
            <td style="padding: 10px;">${item.model}</td>
            <td style="padding: 10px;">${item.capacity || ''}</td>
            <td style="padding: 10px;">
                <button class="btn btn-load" onclick="editVehicle('${item.id}', '${item.registrationNumber}', '${item.make}', '${item.model}', '${item.capacity || ''}')" style="margin-right: 5px; padding: 3px 8px; font-size: 11px;">✏️ Edit</button>
                <button class="btn btn-danger" onclick="deleteVehicle('${item.id}')" style="padding: 3px 8px; font-size: 11px; background: #dc3545; color: white;">🗑️ Delete</button>
            </td>
        </tr>
    `).join('');
}

function editVehicle(id, registration, make, model, capacity) {
    document.getElementById('vehicleId').value = id;
    document.getElementById('vehicleRegistration').value = registration;
    document.getElementById('vehicleMake').value = make;
    document.getElementById('vehicleModel').value = model;
    document.getElementById('vehicleCapacity').value = capacity;
}

async function deleteVehicle(id) {
    if (confirm('Are you sure you want to delete this vehicle?')) {
        const result = await apiCall('DELETE', `/vehicles/${id}`);
        if (result.ok) {
            loadVehicles();
            updateResponseArea('response-vehicles', 'Vehicle deleted successfully', true);
        } else {
            updateResponseArea('response-vehicles', `Error deleting: ${JSON.stringify(result.data)}`, false);
        }
    }
}

// Driver Functions
async function createSingleDriver() {
    const data = {
        name: document.getElementById('driverName').value,
        licenseNumber: document.getElementById('driverLicense').value,
        phoneNumber: document.getElementById('driverPhone').value || null,
        email: document.getElementById('driverEmail').value || null
    };
    
    if (!data.name || !data.licenseNumber) {
        alert('Name and License Number are required');
        return;
    }
    
    const result = await apiCall('POST', '/drivers', data);
    if (result.ok) {
        clearDriverForm();
        loadDrivers();
        updateResponseArea('response-drivers', 'Driver created successfully', true);
    } else {
        updateResponseArea('response-drivers', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

async function updateSingleDriver() {
    const id = document.getElementById('driverId').value;
    if (!id) {
        alert('Please enter an ID for update');
        return;
    }
    
    const data = {
        name: document.getElementById('driverName').value,
        licenseNumber: document.getElementById('driverLicense').value,
        phoneNumber: document.getElementById('driverPhone').value || null,
        email: document.getElementById('driverEmail').value || null
    };
    
    const result = await apiCall('PUT', `/drivers/${id}`, data);
    if (result.ok) {
        clearDriverForm();
        loadDrivers();
        updateResponseArea('response-drivers', 'Driver updated successfully', true);
    } else {
        updateResponseArea('response-drivers', `Error: ${JSON.stringify(result.data)}`, false);
    }
}

function clearDriverForm() {
    document.getElementById('driverId').value = '';
    document.getElementById('driverName').value = '';
    document.getElementById('driverLicense').value = '';
    document.getElementById('driverPhone').value = '';
    document.getElementById('driverEmail').value = '';
}

async function loadDrivers() {
    const result = await apiCall('GET', '/drivers');
    if (result.ok && result.data) {
        populateDriversTable(result.data);
        updateResponseArea('response-drivers', `Loaded ${result.data.length} drivers`, true);
    } else {
        updateResponseArea('response-drivers', `Error loading data: ${result.error || result.data}`, false);
    }
}

function populateDriversTable(data) {
    const tbody = document.getElementById('driversTableBody');
    
    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="padding: 20px; text-align: center; color: #6c757d;">No drivers found</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 10px;">${item.id}</td>
            <td style="padding: 10px;">${item.name}</td>
            <td style="padding: 10px;">${item.licenseNumber}</td>
            <td style="padding: 10px;">${item.phoneNumber || ''}</td>
            <td style="padding: 10px;">${item.email || ''}</td>
            <td style="padding: 10px;">
                <button class="btn btn-load" onclick="editDriver('${item.id}', '${item.name}', '${item.licenseNumber}', '${item.phoneNumber || ''}', '${item.email || ''}')" style="margin-right: 5px; padding: 3px 8px; font-size: 11px;">✏️ Edit</button>
                <button class="btn btn-danger" onclick="deleteDriver('${item.id}')" style="padding: 3px 8px; font-size: 11px; background: #dc3545; color: white;">🗑️ Delete</button>
            </td>
        </tr>
    `).join('');
}

function editDriver(id, name, license, phone, email) {
    document.getElementById('driverId').value = id;
    document.getElementById('driverName').value = name;
    document.getElementById('driverLicense').value = license;
    document.getElementById('driverPhone').value = phone;
    document.getElementById('driverEmail').value = email;
}

async function deleteDriver(id) {
    if (confirm('Are you sure you want to delete this driver?')) {
        const result = await apiCall('DELETE', `/drivers/${id}`);
        if (result.ok) {
            loadDrivers();
            updateResponseArea('response-drivers', 'Driver deleted successfully', true);
        } else {
            updateResponseArea('response-drivers', `Error deleting: ${JSON.stringify(result.data)}`, false);
        }
    }
}

// Utility function to update response areas
function updateResponseArea(elementId, message, isSuccess) {
    const element = document.getElementById(elementId);
    if (element) {
        const color = isSuccess ? 'green' : 'red';
        const timestamp = new Date().toLocaleTimeString();
        element.innerHTML = `
            <div style="color: ${color}; font-size: 12px;">
                [${timestamp}] ${message}
            </div>
        `;
    }
}