const { User } = require('../models');
const { validate, updateUserSchema } = require('../utils/validators');

// Get user profile
const getUserProfile = async (req, res) => {
  try {
    const { userId } = req.params;

    const user = await User.findById(userId);

    if (!user) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'User not found',
      });
    }

    return res.status(200).json({
      user: {
        id: user._id,
        username: user.username,
        email: user.email,
        profileImage: user.profileImage,
        isActive: user.isActive,
        lastLoginAt: user.lastLoginAt,
        createdAt: user.createdAt,
        updatedAt: user.updatedAt,
      },
    });
  } catch (error) {
    console.error('Get user profile error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get current authenticated user profile ( /api/users/me )
const getMe = async (req, res) => {
  try {
    const userId = req.user?.id;

    if (!userId) {
      return res.status(401).json({
        error: 'Unauthorized',
        message: 'Missing user in request context',
      });
    }

    const user = await User.findById(userId);

    if (!user) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'User not found',
      });
    }

    return res.status(200).json({
      user: {
        id: user._id,
        username: user.username,
        email: user.email,
        profileImage: user.profileImage,
        isActive: user.isActive,
        lastLoginAt: user.lastLoginAt,
        createdAt: user.createdAt,
        updatedAt: user.updatedAt,
      },
    });
  } catch (error) {
    console.error('Get current user error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update user profile
const updateUserProfile = async (req, res) => {
  try {
    const { userId } = req.params;

    // Validate input
    const validation = validate(req.body, updateUserSchema);
    if (!validation.valid) {
      return res.status(400).json({
        error: 'Validation Failed',
        errors: validation.errors,
      });
    }

    const updateData = validation.data;

    // Check if username is already taken (if updating username)
    if (updateData.username) {
      const existingUser = await User.findOne({
        username: updateData.username,
        _id: { $ne: userId },
      });

      if (existingUser) {
        return res.status(409).json({
          error: 'Username Already Exists',
          message: 'Username already in use',
        });
      }
    }

    // Check if email is already taken (if updating email)
    if (updateData.email) {
      const existingUser = await User.findOne({
        email: updateData.email,
        _id: { $ne: userId },
      });

      if (existingUser) {
        return res.status(409).json({
          error: 'Email Already Exists',
          message: 'Email already in use',
        });
      }
    }

    // Update user
    const updatedUser = await User.findByIdAndUpdate(
      userId,
      updateData,
      { new: true, runValidators: true }
    );

    if (!updatedUser) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'User not found',
      });
    }

    return res.status(200).json({
      message: 'User profile updated successfully',
      user: {
        id: updatedUser._id,
        username: updatedUser.username,
        email: updatedUser.email,
        profileImage: updatedUser.profileImage,
        isActive: updatedUser.isActive,
        updatedAt: updatedUser.updatedAt,
      },
    });
  } catch (error) {
    console.error('Update user profile error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Delete user account
const deleteUserAccount = async (req, res) => {
  try {
    const { userId } = req.params;

    const user = await User.findByIdAndDelete(userId);

    if (!user) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'User not found',
      });
    }

    // TODO: Also delete related data (GameProfile, LevelProgress, GameSession, etc.)

    return res.status(200).json({
      message: 'User account deleted successfully',
    });
  } catch (error) {
    console.error('Delete user account error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  getUserProfile,
  getMe,
  updateUserProfile,
  deleteUserAccount,
};
