const express = require('express');
const router = express.Router();
const userController = require('../controllers/userController');
const { authenticateToken, authorizeUser } = require('../middleware/auth');

// GET /api/users/me - Get current user profile
router.get('/me', authenticateToken, userController.getMe);

// GET /api/users/:userId - Get user profile by id
router.get('/:userId', authenticateToken, userController.getUserProfile);

// PUT /api/users/:userId - Update user profile
router.put(
  '/:userId',
  authenticateToken,
  authorizeUser,
  userController.updateUserProfile
);

// DELETE /api/users/:userId - Delete user account
router.delete(
  '/:userId',
  authenticateToken,
  authorizeUser,
  userController.deleteUserAccount
);

module.exports = router;
