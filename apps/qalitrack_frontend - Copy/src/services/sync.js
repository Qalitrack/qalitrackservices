import api from './api';
import { store } from '../store';
import { clearOfflineQueue } from '../store/weighingSlice';

export async function syncData(){
  const state = store.getState();
  const queue = state.weighing.offlineQueue || [];
  if (!queue.length) return;
  try{
    await api.post('/sync', queue);
    store.dispatch(clearOfflineQueue());
  }catch(e){
    console.warn('Sync failed, will retry later.');
  }
}

window.addEventListener('online', () => { syncData(); });
