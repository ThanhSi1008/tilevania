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

    // Get all achievements
    const allAchievements = await Achievement.find().sort({ rarity: -1, createdAt: 1 });
    
    // Get player's achievement progress
    const playerAchievements = await PlayerAchievement.find({ userId })
      .populate('achievementId');
    
    // Create a map for quick lookup
    const progressMap = new Map();
    playerAchievements.forEach(pa => {
      if (pa.achievementId && pa.achievementId._id) {
        progressMap.set(pa.achievementId._id.toString(), pa);
      }
    });

    // Get game profile to calculate progress for achievements without records
    const gameProfile = await GameProfile.findOne({ userId });
    if (!gameProfile) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Game profile not found',
      });
    }

    // Build response with all achievements and their progress
    const achievementsWithProgress = allAchievements.map(achievement => {
      const existing = progressMap.get(achievement._id.toString());
      
      // If PlayerAchievement exists, use it
      if (existing) {
        return existing;
      }
      
      // If no PlayerAchievement exists, calculate progress on-the-fly
      const { currentProgress } = calculateProgress(achievement.condition, gameProfile);
      
      return {
        _id: null, // No PlayerAchievement record yet
        unlockedAt: null,
        progress: currentProgress,
        achievementId: achievement,
      };
    });

    return res.status(200).json({
      count: achievementsWithProgress.length,
      achievements: achievementsWithProgress,
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

// Calculate progress for an achievement based on condition
const calculateProgress = (condition, gameProfile) => {
  let currentProgress = 0;
  let shouldUnlock = false;

  switch (condition) {
    case 'FIRST_KILL':
      currentProgress = gameProfile.totalEnemiesDefeated >= 1 ? 100 : 0;
      shouldUnlock = currentProgress >= 100;
      break;
    case 'COIN_COLLECTOR_100':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 100) * 100));
      shouldUnlock = gameProfile.totalCoinsCollected >= 100;
      break;
    case 'COIN_COLLECTOR_500':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalCoinsCollected / 500) * 100));
      shouldUnlock = gameProfile.totalCoinsCollected >= 500;
      break;
    case 'SCORE_MASTER_1000':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalScore / 1000) * 100));
      shouldUnlock = gameProfile.totalScore >= 1000;
      break;
    case 'SCORE_MASTER_5000':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalScore / 5000) * 100));
      shouldUnlock = gameProfile.totalScore >= 5000;
      break;
    case 'KILLER_100':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalEnemiesDefeated / 100) * 100));
      shouldUnlock = gameProfile.totalEnemiesDefeated >= 100;
      break;
    case 'PLAYTIME_HOUR':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 3600) * 100));
      shouldUnlock = gameProfile.totalPlayTime >= 3600;
      break;
    case 'PLAYTIME_DAY':
      currentProgress = Math.min(100, Math.floor((gameProfile.totalPlayTime / 86400) * 100));
      shouldUnlock = gameProfile.totalPlayTime >= 86400;
      break;
    default:
      currentProgress = 0;
      shouldUnlock = false;
      break;
  }

  return { currentProgress, shouldUnlock };
};

// Auto-check and unlock achievements
const checkAchievements = async (userId) => {
  try {
    const gameProfile = await GameProfile.findOne({ userId });
    if (!gameProfile) return;

    // List of achievements to check
    const achievements = await Achievement.find();
    const unlockedAchievements = await PlayerAchievement.find({ userId });
    const unlockedMap = new Map();
    unlockedAchievements.forEach(a => {
      unlockedMap.set(a.achievementId.toString(), a);
    });

    for (const achievement of achievements) {
      const achievementIdStr = achievement._id.toString();
      const existing = unlockedMap.get(achievementIdStr);
      const isUnlocked = existing && existing.progress >= 100;

      // Calculate current progress (0-100)
      const { currentProgress, shouldUnlock } = calculateProgress(achievement.condition, gameProfile);

      // Ensure progress is 100 if unlocked
      const finalProgress = shouldUnlock ? 100 : currentProgress;

      // Update or create PlayerAchievement
      if (existing) {
        // Update existing
        existing.progress = finalProgress;
        if (shouldUnlock && !isUnlocked) {
          existing.unlockedAt = new Date();
          gameProfile.totalScore += achievement.points;
        }
        await existing.save();
      } else {
        // Create new PlayerAchievement (even if not unlocked)
        const playerAchievement = new PlayerAchievement({
          userId,
          achievementId: achievement._id,
          progress: finalProgress,
          unlockedAt: shouldUnlock ? new Date() : null,
        });
        await playerAchievement.save();

        if (shouldUnlock) {
          gameProfile.totalScore += achievement.points;
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
