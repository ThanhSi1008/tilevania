const { GameProfile } = require('../models');

// Get game profile
const getGameProfile = async (req, res) => {
  try {
    const { userId } = req.params;

    const gameProfile = await GameProfile.findOne({ userId }).populate('userId', 'username email');

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    return res.status(200).json({ gameProfile });
  } catch (error) {
    console.error('Get game profile error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update game profile
const updateGameProfile = async (req, res) => {
  try {
    const { userId } = req.params;
    const updateData = req.body;

    const gameProfile = await GameProfile.findOneAndUpdate(
      { userId },
      updateData,
      { new: true, runValidators: true }
    );

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    return res.status(200).json({
      message: 'Game profile updated successfully',
      gameProfile,
    });
  } catch (error) {
    console.error('Update game profile error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Add score
const addScore = async (req, res) => {
  try {
    const { userId } = req.params;
    const { points } = req.body;

    if (typeof points !== 'number' || points < 0) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Points must be a positive number',
      });
    }

    const gameProfile = await GameProfile.findOne({ userId });

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    gameProfile.totalScore += points;
    gameProfile.lastSessionScore = points;

    // Update highest score if needed
    if (gameProfile.totalScore > gameProfile.highestScoreAchieved) {
      gameProfile.highestScoreAchieved = gameProfile.totalScore;
    }

    await gameProfile.save();

    return res.status(200).json({
      message: 'Score added successfully',
      gameProfile,
    });
  } catch (error) {
    console.error('Add score error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Add coins
const addCoins = async (req, res) => {
  try {
    const { userId } = req.params;
    const { coins } = req.body;

    if (typeof coins !== 'number' || coins < 0) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Coins must be a positive number',
      });
    }

    const gameProfile = await GameProfile.findOne({ userId });

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    gameProfile.totalCoinsCollected += coins;
    await gameProfile.save();

    return res.status(200).json({
      message: 'Coins added successfully',
      gameProfile,
    });
  } catch (error) {
    console.error('Add coins error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Increment death count
const incrementDeathCount = async (req, res) => {
  try {
    const { userId } = req.params;

    const gameProfile = await GameProfile.findOne({ userId });

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    gameProfile.totalDeaths += 1;
    gameProfile.currentLives = Math.max(0, gameProfile.currentLives - 1);
    await gameProfile.save();

    return res.status(200).json({
      message: 'Death count incremented',
      gameProfile,
    });
  } catch (error) {
    console.error('Increment death count error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update current lives
const updateCurrentLives = async (req, res) => {
  try {
    const { userId } = req.params;
    const { lives } = req.body;

    if (typeof lives !== 'number' || lives < 0) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Lives must be a non-negative number',
      });
    }

    const gameProfile = await GameProfile.findOne({ userId });

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    gameProfile.currentLives = lives;
    await gameProfile.save();

    return res.status(200).json({
      message: 'Current lives updated',
      gameProfile,
    });
  } catch (error) {
    console.error('Update current lives error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Add play time
const addPlayTime = async (req, res) => {
  try {
    const { userId } = req.params;
    const { seconds } = req.body;

    if (typeof seconds !== 'number' || seconds < 0) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Seconds must be a non-negative number',
      });
    }

    const gameProfile = await GameProfile.findOne({ userId });

    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    gameProfile.totalPlayTime += seconds;
    await gameProfile.save();

    return res.status(200).json({
      message: 'Play time updated',
      gameProfile,
    });
  } catch (error) {
    console.error('Add play time error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  getGameProfile,
  updateGameProfile,
  addScore,
  addCoins,
  incrementDeathCount,
  updateCurrentLives,
  addPlayTime,
};
