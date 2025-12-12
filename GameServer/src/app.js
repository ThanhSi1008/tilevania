const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const swaggerUi = require('swagger-ui-express');
const db = require('./config/database');
const swaggerSpec = require('./config/swagger');

const app = express();

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Swagger UI
app.use('/api/docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec, { swaggerOptions: { defaultModelsExpandDepth: 1 } }));
app.get('/api/swagger.json', (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  res.send(swaggerSpec);
});

// Routes
app.get('/api/health', (req, res) => {
  res.status(200).json({
    status: 'OK',
    message: 'Tilevania Server is running',
    timestamp: new Date().toISOString(),
  });
});

// Auth routes
app.use('/api/auth', require('./routes/auth'));

// User routes
app.use('/api/users', require('./routes/users'));

// Level routes
app.use('/api/levels', require('./routes/levels'));

// Game profile routes
app.use('/api/gameProfile', require('./routes/gameProfile'));

// Level progress routes
app.use('/api/levelProgress', require('./routes/levelProgress'));

// Game session routes
app.use('/api/sessions', require('./routes/sessions'));

// Achievement routes
app.use('/api/achievements', require('./routes/achievements'));

// Leaderboard routes
app.use('/api/leaderboard', require('./routes/leaderboard'));

// Error handling middleware
app.use((err, req, res, next) => {
  console.error('Error:', err);
  res.status(500).json({
    error: 'Internal Server Error',
    message: err.message,
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: `Route ${req.method} ${req.path} not found`,
  });
});

module.exports = app;
