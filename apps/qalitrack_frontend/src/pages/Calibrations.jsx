import { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { addCalibration, updateCalibration, deleteCalibration } from '../store/calibrationSlice';

export default function Calibrations() {
  const dispatch = useDispatch();
  const calibrations = useSelector(state => state.calibration?.records || []);

  const [form, setForm] = useState({ id: null, equipment: '', date: '', status: '' });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.equipment || !form.date || !form.status) {
      alert('All fields are required');
      return;
    }

    if (form.id) {
      dispatch(updateCalibration(form));
    } else {
      dispatch(addCalibration({ ...form, id: Date.now() }));
    }

    setForm({ id: null, equipment: '', date: '', status: '' });
  };

  const handleEdit = (record) => {
    setForm(record);
  };

  const handleDelete = (id) => {
    dispatch(deleteCalibration(id));
  };

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Calibration Records</h2>

      {/* Form */}
      <form onSubmit={handleSubmit} className="mb-4 flex flex-wrap gap-2">
        <input
          type="text"
          placeholder="Equipment"
          value={form.equipment}
          onChange={(e) => setForm({ ...form, equipment: e.target.value })}
          className="border rounded px-3 py-2"
        />
        <input
          type="date"
          value={form.date}
          onChange={(e) => setForm({ ...form, date: e.target.value })}
          className="border rounded px-3 py-2"
        />
        <select
          value={form.status}
          onChange={(e) => setForm({ ...form, status: e.target.value })}
          className="border rounded px-3 py-2"
        >
          <option value="">Select Status</option>
          <option value="Pending">Pending</option>
          <option value="Completed">Completed</option>
          <option value="Overdue">Overdue</option>
        </select>
        <button
          type="submit"
          className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
        >
          {form.id ? 'Update' : 'Add'}
        </button>
      </form>

      {/* Records Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100 text-left">
              <th className="px-4 py-2 border">Equipment</th>
              <th className="px-4 py-2 border">Date</th>
              <th className="px-4 py-2 border">Status</th>
              <th className="px-4 py-2 border">Actions</th>
            </tr>
          </thead>
          <tbody>
            {calibrations.map((record) => (
              <tr key={record.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 border">{record.equipment}</td>
                <td className="px-4 py-2 border">{record.date}</td>
                <td className="px-4 py-2 border">{record.status}</td>
                <td className="px-4 py-2 border space-x-2">
                  <button
                    className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                    onClick={() => handleEdit(record)}
                  >
                    Edit
                  </button>
                  <button
                    className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600"
                    onClick={() => handleDelete(record.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
            {calibrations.length === 0 && (
              <tr>
                <td colSpan="4" className="px-4 py-3 text-center text-gray-500">
                  No calibration records found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
