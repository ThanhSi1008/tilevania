const express = require('express');
const router = express.Router();
const levelController = require('../controllers/levelController');

// GET /api/levels - Get all levels
router.get('/', levelController.getAllLevels);

// GET /api/levels/:levelId - Get level by ID
router.get('/:levelId', levelController.getLevelById);

// POST /api/levels - Create level (admin)
router.post('/', levelController.createLevel);

// PUT /api/levels/:levelId - Update level (admin)
router.put('/:levelId', levelController.updateLevel);

module.exports = router;
