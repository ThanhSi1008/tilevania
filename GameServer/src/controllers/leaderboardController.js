const { Leaderboard, User } = require('../models');

// Get global leaderboard
const getGlobalLeaderboard = async (req, res) => {
  try {
    const { limit = 50 } = req.query;

    const leaderboard = await Leaderboard.find({ period: 'ALLTIME' })
      .sort({ rank: 1 })
      .limit(parseInt(limit));

    return res.status(200).json({
      count: leaderboard.length,
      leaderboard,
    });
  } catch (error) {
    console.error('Get global leaderboard error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get weekly leaderboard
const getWeeklyLeaderboard = async (req, res) => {
  try {
    const { limit = 50 } = req.query;

    const leaderboard = await Leaderboard.find({ period: 'WEEKLY' })
      .sort({ rank: 1 })
      .limit(parseInt(limit));

    return res.status(200).json({
      count: leaderboard.length,
      leaderboard,
    });
  } catch (error) {
    console.error('Get weekly leaderboard error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get daily leaderboard
const getDailyLeaderboard = async (req, res) => {
  try {
    const { limit = 50 } = req.query;

    const leaderboard = await Leaderboard.find({ period: 'DAILY' })
      .sort({ rank: 1 })
      .limit(parseInt(limit));

    return res.status(200).json({
      count: leaderboard.length,
      leaderboard,
    });
  } catch (error) {
    console.error('Get daily leaderboard error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get player rank by period
const getPlayerRank = async (req, res) => {
  try {
    const { userId } = req.params;
    const { period = 'ALLTIME' } = req.query;

    const playerRank = await Leaderboard.findOne({ userId, period });

    if (!playerRank) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Player not found in leaderboard for this period',
      });
    }

    return res.status(200).json({
      rank: playerRank,
    });
  } catch (error) {
    console.error('Get player rank error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Calculate and update leaderboards (scheduled task)
const calculateLeaderboards = async () => {
  try {
    console.log('Calculating leaderboards...');

    // Define period dates
    const now = new Date();
    const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    const dayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);

    // Delete old leaderboards
    await Leaderboard.deleteMany({});

    // Get all users with their profiles
    const { GameProfile } = require('../models');
    const gameProfiles = await GameProfile.find().populate('userId');

    // Calculate ALLTIME leaderboard
    const alltimeScores = gameProfiles
      .map(gp => ({
        userId: gp.userId._id,
        username: gp.userId.username,
        profileImage: gp.userId.profileImage,
        totalScore: gp.totalScore,
        period: 'ALLTIME',
      }))
      .sort((a, b) => b.totalScore - a.totalScore);

    // Save ALLTIME leaderboard with ranks
    let rank = 1;
    for (const entry of alltimeScores) {
      await Leaderboard.create({
        ...entry,
        rank: rank++,
        calculatedAt: now,
      });
    }

    // Calculate WEEKLY leaderboard (simplified - based on total score for now)
    // In real app, you'd track session dates
    const weeklyScores = alltimeScores
      .map(entry => ({ ...entry, period: 'WEEKLY' }))
      .sort((a, b) => b.totalScore - a.totalScore);

    rank = 1;
    for (const entry of weeklyScores) {
      await Leaderboard.create({
        ...entry,
        rank: rank++,
        calculatedAt: now,
      });
    }

    // Calculate DAILY leaderboard (simplified)
    const dailyScores = alltimeScores
      .map(entry => ({ ...entry, period: 'DAILY' }))
      .sort((a, b) => b.totalScore - a.totalScore);

    rank = 1;
    for (const entry of dailyScores) {
      await Leaderboard.create({
        ...entry,
        rank: rank++,
        calculatedAt: now,
      });
    }

    console.log('✅ Leaderboards calculated successfully');
  } catch (error) {
    console.error('Calculate leaderboards error:', error);
  }
};

module.exports = {
  getGlobalLeaderboard,
  getWeeklyLeaderboard,
  getDailyLeaderboard,
  getPlayerRank,
  calculateLeaderboards,
};
