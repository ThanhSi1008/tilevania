const { GameSession, GameProfile, LevelProgress } = require('../models');

// Start game session
const startSession = async (req, res) => {
  try {
    const { userId, levelId } = req.body;

    if (!userId || !levelId) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'userId and levelId are required',
      });
    }

    // Create new session
    const newSession = new GameSession({
      userId,
      levelId,
      startTime: new Date(),
      sessionStatus: 'ACTIVE',
    });

    await newSession.save();

    return res.status(201).json({
      message: 'Game session started',
      session: newSession,
    });
  } catch (error) {
    console.error('Start session error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update session (during gameplay)
const updateSession = async (req, res) => {
  try {
    const { sessionId } = req.params;
    const { finalScore, coinsCollected, enemiesDefeated, deathCount, livesRemaining } = req.body;

    const session = await GameSession.findById(sessionId);

    if (!session) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Session not found',
      });
    }

    if (finalScore !== undefined) {
      session.finalScore = finalScore;
    }

    if (coinsCollected !== undefined) {
      session.coinsCollected = coinsCollected;
    }

    if (enemiesDefeated !== undefined) {
      session.enemiesDefeated = enemiesDefeated;
    }

    if (deathCount !== undefined) {
      session.deathCount = deathCount;
    }

    if (livesRemaining !== undefined) {
      session.livesRemaining = livesRemaining;
    }

    await session.save();

    return res.status(200).json({
      message: 'Session updated',
      session,
    });
  } catch (error) {
    console.error('Update session error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// End session
const endSession = async (req, res) => {
  try {
    const { sessionId } = req.params;
    const { status, finalScore, coinsCollected, enemiesDefeated, deathCount, livesRemaining } = req.body;

    const session = await GameSession.findById(sessionId);

    if (!session) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Session not found',
      });
    }

    // Calculate duration
    const endTime = new Date();
    session.endTime = endTime;
    session.duration = Math.round((endTime - session.startTime) / 1000); // in seconds

    // Update session data
    session.sessionStatus = status || 'COMPLETED';
    session.isCompleted = ['COMPLETED', 'ABANDONED', 'FAILED'].includes(session.sessionStatus);

    if (finalScore !== undefined) session.finalScore = finalScore;
    if (coinsCollected !== undefined) session.coinsCollected = coinsCollected;
    if (enemiesDefeated !== undefined) session.enemiesDefeated = enemiesDefeated;
    if (deathCount !== undefined) session.deathCount = deathCount;
    if (livesRemaining !== undefined) session.livesRemaining = livesRemaining;

    await session.save();

    // Update game profile
    const gameProfile = await GameProfile.findOne({ userId: session.userId });
    if (gameProfile) {
      gameProfile.totalScore += session.finalScore;
      gameProfile.totalCoinsCollected += session.coinsCollected;
      gameProfile.totalEnemiesDefeated += session.enemiesDefeated;
      gameProfile.totalDeaths += session.deathCount;
      gameProfile.totalPlayTime += session.duration;
      gameProfile.currentLives = session.livesRemaining;
      gameProfile.lastSessionScore = session.finalScore;

      if (session.finalScore > gameProfile.highestScoreAchieved) {
        gameProfile.highestScoreAchieved = session.finalScore;
      }

      await gameProfile.save();
    }

    // Update level progress if level was completed
    if (session.sessionStatus === 'COMPLETED') {
      let levelProgress = await LevelProgress.findOne({
        userId: session.userId,
        levelId: session.levelId,
      });

      if (!levelProgress) {
        levelProgress = new LevelProgress({
          userId: session.userId,
          levelId: session.levelId,
        });
      }

      levelProgress.isCompleted = true;
      levelProgress.completedAt = new Date();
      levelProgress.coinsCollected = Math.max(levelProgress.coinsCollected, session.coinsCollected);
      levelProgress.enemiesDefeated = Math.max(levelProgress.enemiesDefeated, session.enemiesDefeated);
      levelProgress.bestScore = Math.max(levelProgress.bestScore, session.finalScore);
      levelProgress.bestTime = session.duration;
      levelProgress.playCount += 1;
      levelProgress.lastPlayedAt = new Date();

      await levelProgress.save();
    }

    return res.status(200).json({
      message: 'Game session ended',
      session,
    });
  } catch (error) {
    console.error('End session error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get session history
const getSessionHistory = async (req, res) => {
  try {
    const { userId } = req.params;
    const { limit = 20, offset = 0 } = req.query;

    const sessions = await GameSession.find({ userId })
      .populate('levelId', 'levelName levelNumber')
      .sort({ startTime: -1 })
      .limit(parseInt(limit))
      .skip(parseInt(offset));

    const total = await GameSession.countDocuments({ userId });

    return res.status(200).json({
      total,
      count: sessions.length,
      sessions,
    });
  } catch (error) {
    console.error('Get session history error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  startSession,
  updateSession,
  endSession,
  getSessionHistory,
};
