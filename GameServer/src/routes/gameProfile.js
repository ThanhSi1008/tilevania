const express = require('express');
const router = express.Router();
const gameProfileController = require('../controllers/gameProfileController');
const { authenticateToken, authorizeUser } = require('../middleware/auth');

// GET /api/gameProfile/:userId - Get game profile
router.get('/:userId', authenticateToken, gameProfileController.getGameProfile);

// PUT /api/gameProfile/:userId - Update game profile
router.put('/:userId', authenticateToken, authorizeUser, gameProfileController.updateGameProfile);

// POST /api/gameProfile/:userId/score - Add score
router.post('/:userId/score', authenticateToken, authorizeUser, gameProfileController.addScore);

// POST /api/gameProfile/:userId/coins - Add coins
router.post('/:userId/coins', authenticateToken, authorizeUser, gameProfileController.addCoins);

// POST /api/gameProfile/:userId/death - Increment death count
router.post('/:userId/death', authenticateToken, authorizeUser, gameProfileController.incrementDeathCount);

// PUT /api/gameProfile/:userId/lives - Update current lives
router.put('/:userId/lives', authenticateToken, authorizeUser, gameProfileController.updateCurrentLives);

// POST /api/gameProfile/:userId/playtime - Add play time
router.post('/:userId/playtime', authenticateToken, authorizeUser, gameProfileController.addPlayTime);

module.exports = router;
