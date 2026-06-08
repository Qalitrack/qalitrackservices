// src/services/syncService.js
import { dequeueById } from '../store/offlineQueueSlice';
import { addTransaction, addSecondWeight, deactivateTransactionApi } from '../store/weighingSlice';

export function initSync(store) {
  const flush = async () => {
    const state = store.getState();
    const queue = state.offlineQueue.items || [];
    if (!navigator.onLine || queue.length === 0) return;

    for (const item of queue) {
      try {
        if (item.type === 'CREATE') {
          await store.dispatch(addTransaction(item.payload)).unwrap();
        } else if (item.type === 'COMPLETE') {
          await store.dispatch(addSecondWeight(item.payload)).unwrap();
        } else if (item.type === 'DELETE') {
          await store.dispatch(deactivateTransactionApi(item.payload.id)).unwrap();
        }
        store.dispatch(dequeueById(item.id));
      } catch (e) {
        // Stop on first error to retry later (simple backoff)
        break;
      }
    }
  };

  // Try at start and whenever we go online
  window.addEventListener('online', flush);
  flush();

  // Optional periodic retry
  setInterval(flush, 15000);
}
