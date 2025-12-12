const express = require('express');
const router = express.Router();
const sessionController = require('../controllers/sessionController');
const { authenticateToken, authorizeUser } = require('../middleware/auth');

// POST /api/sessions - Start new session
router.post('/', authenticateToken, sessionController.startSession);

// PUT /api/sessions/:sessionId - Update session
router.put('/:sessionId', authenticateToken, sessionController.updateSession);

// POST /api/sessions/:sessionId/end - End session
router.post('/:sessionId/end', authenticateToken, sessionController.endSession);

// GET /api/sessions/:userId/history - Get session history
router.get('/:userId/history', authenticateToken, authorizeUser, sessionController.getSessionHistory);

module.exports = router;
