const { Achievement, PlayerAchievement, GameProfile } = require('../models');

// Get all achievements
const getAllAchievements = async (req, res) => {
  try {
    const achievements = await Achievement.find().sort({ rarity: -1, createdAt: 1 });

    return res.status(200).json({
      count: achievements.length,
      achievements,
    });
  } catch (error) {
    console.error('Get all achievements error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get user achievements
const getPlayerAchievements = async (req, res) => {
  try {
    const { userId } = req.params;

    const playerAchievements = await PlayerAchievement.find({ userId })
      .populate('achievementId')
      .sort({ unlockedAt: -1 });

    return res.status(200).json({
      count: playerAchievements.length,
      achievements: playerAchievements,
    });
  } catch (error) {
    console.error('Get player achievements error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Unlock achievement
const unlockAchievement = async (req, res) => {
  try {
    const { userId, achievementId } = req.params;

    // Check if achievement exists
    const achievement = await Achievement.findById(achievementId);
    if (!achievement) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Achievement not found',
      });
    }

    // Check if already unlocked
    const existingUnlock = await PlayerAchievement.findOne({ userId, achievementId });
    if (existingUnlock) {
      return res.status(200).json({
        message: 'Achievement already unlocked',
        achievement: existingUnlock,
      });
    }

    // Create new player achievement
    const playerAchievement = new PlayerAchievement({
      userId,
      achievementId,
      unlockedAt: new Date(),
      progress: 100,
    });

    await playerAchievement.save();

    // Update user's game profile with achievement points
    const gameProfile = await GameProfile.findOne({ userId });
    if (gameProfile) {
      gameProfile.totalScore += achievement.points;
      await gameProfile.save();
    }

    return res.status(201).json({
      message: 'Achievement unlocked successfully',
      achievement: await playerAchievement.populate('achievementId'),
    });
  } catch (error) {
    console.error('Unlock achievement error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Auto-check and unlock achievements
const checkAchievements = async (userId) => {
  try {
    const gameProfile = await GameProfile.findOne({ userId });
    if (!gameProfile) return;

    // List of achievements to check
    const achievements = await Achievement.find();
    const unlockedAchievements = await PlayerAchievement.find({ userId });
    const unlockedIds = new Set(unlockedAchievements.map(a => a.achievementId.toString()));

    for (const achievement of achievements) {
      // Skip if already unlocked
      if (unlockedIds.has(achievement._id.toString())) continue;

      let shouldUnlock = false;

      // Check conditions
      switch (achievement.condition) {
        case 'FIRST_KILL':
          shouldUnlock = gameProfile.totalEnemiesDefeated >= 1;
          break;
        case 'COIN_COLLECTOR_100':
          shouldUnlock = gameProfile.totalCoinsCollected >= 100;
          break;
        case 'COIN_COLLECTOR_500':
          shouldUnlock = gameProfile.totalCoinsCollected >= 500;
          break;
        case 'SCORE_MASTER_1000':
          shouldUnlock = gameProfile.totalScore >= 1000;
          break;
        case 'SCORE_MASTER_5000':
          shouldUnlock = gameProfile.totalScore >= 5000;
          break;
        case 'KILLER_100':
          shouldUnlock = gameProfile.totalEnemiesDefeated >= 100;
          break;
        case 'PLAYTIME_HOUR':
          shouldUnlock = gameProfile.totalPlayTime >= 3600; // 1 hour
          break;
        case 'PLAYTIME_DAY':
          shouldUnlock = gameProfile.totalPlayTime >= 86400; // 1 day
          break;
        // Add more conditions as needed
        default:
          break;
      }

      if (shouldUnlock) {
        const playerAchievement = new PlayerAchievement({
          userId,
          achievementId: achievement._id,
          unlockedAt: new Date(),
          progress: 100,
        });

        try {
          await playerAchievement.save();
          gameProfile.totalScore += achievement.points;
        } catch (err) {
          // Achievement might already be unlocked
        }
      }
    }

    // Save updated game profile
    await gameProfile.save();
  } catch (error) {
    console.error('Check achievements error:', error);
  }
};

module.exports = {
  getAllAchievements,
  getPlayerAchievements,
  unlockAchievement,
  checkAchievements,
};
