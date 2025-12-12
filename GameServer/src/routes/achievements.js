const express = require('express');
const router = express.Router();
const achievementController = require('../controllers/achievementController');
const { authenticateToken, authorizeUser } = require('../middleware/auth');

// GET /api/achievements - Get all achievements
router.get('/', achievementController.getAllAchievements);

// GET /api/achievements/:userId - Get player achievements
router.get('/:userId/unlocked', authenticateToken, authorizeUser, achievementController.getPlayerAchievements);

// POST /api/achievements/:userId/:achievementId - Unlock achievement
router.post(
  '/:userId/unlock/:achievementId',
  authenticateToken,
  authorizeUser,
  achievementController.unlockAchievement
);

module.exports = router;
