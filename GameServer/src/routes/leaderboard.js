const express = require('express');
const router = express.Router();
const leaderboardController = require('../controllers/leaderboardController');
const { authenticateToken } = require('../middleware/auth');

// GET /api/leaderboard - Get global leaderboard
router.get('/', leaderboardController.getGlobalLeaderboard);

// GET /api/leaderboard/weekly - Get weekly leaderboard
router.get('/weekly', leaderboardController.getWeeklyLeaderboard);

// GET /api/leaderboard/daily - Get daily leaderboard
router.get('/daily', leaderboardController.getDailyLeaderboard);

// GET /api/leaderboard/rank/:userId - Get player rank
router.get('/rank/:userId', authenticateToken, leaderboardController.getPlayerRank);

module.exports = router;
