const { LevelProgress, GameProfile } = require('../models');

// Get level progress
const getLevelProgress = async (req, res) => {
  try {
    const { userId, levelId } = req.params;

    const levelProgress = await LevelProgress.findOne({ userId, levelId })
      .populate('userId', 'username email')
      .populate('levelId', 'levelName difficulty');

    if (!levelProgress) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Level progress not found',
      });
    }

    return res.status(200).json({ levelProgress });
  } catch (error) {
    console.error('Get level progress error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get all level progress for a user
const getUserLevelProgress = async (req, res) => {
  try {
    const { userId } = req.params;

    const levelProgresses = await LevelProgress.find({ userId })
      .populate('levelId', 'levelName levelNumber difficulty')
      .sort({ 'levelId.levelNumber': 1 });

    return res.status(200).json({
      count: levelProgresses.length,
      levelProgresses,
    });
  } catch (error) {
    console.error('Get user level progress error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update level progress
const updateLevelProgress = async (req, res) => {
  try {
    const { userId, levelId } = req.params;
    const { coinsCollected, enemiesDefeated, deathCount, bestScore, bestTime } = req.body;

    // Check if progress exists, if not create it
    let levelProgress = await LevelProgress.findOne({ userId, levelId });

    if (!levelProgress) {
      levelProgress = new LevelProgress({
        userId,
        levelId,
      });
    }

    // Update fields
    if (coinsCollected !== undefined) {
      levelProgress.coinsCollected = Math.max(levelProgress.coinsCollected, coinsCollected);
    }

    if (enemiesDefeated !== undefined) {
      levelProgress.enemiesDefeated = Math.max(levelProgress.enemiesDefeated, enemiesDefeated);
    }

    if (deathCount !== undefined) {
      levelProgress.deathCount += deathCount;
    }

    if (bestScore !== undefined) {
      levelProgress.bestScore = Math.max(levelProgress.bestScore, bestScore);
    }

    if (bestTime !== undefined && (levelProgress.bestTime === null || bestTime < levelProgress.bestTime)) {
      levelProgress.bestTime = bestTime;
    }

    levelProgress.playCount += 1;
    levelProgress.lastPlayedAt = new Date();

    await levelProgress.save();

    return res.status(200).json({
      message: 'Level progress updated successfully',
      levelProgress,
    });
  } catch (error) {
    console.error('Update level progress error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Complete level
const completeLevel = async (req, res) => {
  try {
    const { userId, levelId } = req.params;
    const { score, coins, enemies, time } = req.body;

    let levelProgress = await LevelProgress.findOne({ userId, levelId });

    if (!levelProgress) {
      levelProgress = new LevelProgress({
        userId,
        levelId,
      });
    }

    levelProgress.isCompleted = true;
    levelProgress.completedAt = new Date();
    levelProgress.playCount += 1;
    levelProgress.lastPlayedAt = new Date();

    if (score !== undefined) {
      levelProgress.bestScore = Math.max(levelProgress.bestScore, score);
    }

    if (coins !== undefined) {
      levelProgress.coinsCollected = Math.max(levelProgress.coinsCollected, coins);
    }

    if (enemies !== undefined) {
      levelProgress.enemiesDefeated = Math.max(levelProgress.enemiesDefeated, enemies);
    }

    if (time !== undefined && (levelProgress.bestTime === null || time < levelProgress.bestTime)) {
      levelProgress.bestTime = time;
    }

    await levelProgress.save();

    return res.status(200).json({
      message: 'Level completed successfully',
      levelProgress,
    });
  } catch (error) {
    console.error('Complete level error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  getLevelProgress,
  getUserLevelProgress,
  updateLevelProgress,
  completeLevel,
};
