const express = require('express');
const router = express.Router();
const levelProgressController = require('../controllers/levelProgressController');
const { authenticateToken, authorizeUser } = require('../middleware/auth');

// GET /api/levelProgress/:userId - Get all user level progress
router.get('/:userId', authenticateToken, authorizeUser, levelProgressController.getUserLevelProgress);

// GET /api/levelProgress/:userId/:levelId - Get specific level progress
router.get('/:userId/:levelId', authenticateToken, authorizeUser, levelProgressController.getLevelProgress);

// PUT /api/levelProgress/:userId/:levelId - Update level progress
router.put('/:userId/:levelId', authenticateToken, authorizeUser, levelProgressController.updateLevelProgress);

// POST /api/levelProgress/:userId/:levelId/complete - Complete level
router.post('/:userId/:levelId/complete', authenticateToken, authorizeUser, levelProgressController.completeLevel);

module.exports = router;
