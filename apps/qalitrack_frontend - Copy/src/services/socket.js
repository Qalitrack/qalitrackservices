import { io } from 'socket.io-client';

// In real use: const socket = io('https://backend-api', { autoConnect: true });
export const socket = {
  connect: () => {
    if (!socket.interval) {
      socket.interval = setInterval(() => {
        const weight = (Math.random() * 50).toFixed(2);
        window.dispatchEvent(new CustomEvent('weightUpdate', { detail: parseFloat(weight) }));
      }, 1000);
    }
  },
  disconnect: () => {
    clearInterval(socket.interval);
    socket.interval = null;
  }
};
