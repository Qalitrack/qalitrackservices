// server.js
import express from 'express';
import cors from 'cors';

const app = express();
app.use(cors());
app.use(express.json());

let data = []; // in-memory store

app.get('/weighing', (req, res) => {
  res.json(data);
});

app.post('/weighing', (req, res) => {
  const tx = {
    id: Date.now().toString(),
    plate: req.body.plate,
    orderId: req.body.orderId,
    w1: req.body.w1,
    w2: null,
    startTime: Date.now(),
    ttat: null,
    date: new Date().toISOString()
  };
  data.push(tx);
  res.status(201).json(tx);
});

app.patch('/weighing/:id/complete', (req, res) => {
  const { id } = req.params;
  const tx = data.find(t => t.id === id);
  if (!tx) return res.status(404).send('Not found');
  tx.w2 = req.body.w2;
  tx.ttat = Math.floor((Date.now() - tx.startTime) / 1000);
  res.json(tx);
});

app.delete('/weighing/:id', (req, res) => {
  const { id } = req.params;
  data = data.filter(t => t.id !== id);
  res.status(204).send();
});

app.listen(4000, () => console.log('API running on http://localhost:4000'));
