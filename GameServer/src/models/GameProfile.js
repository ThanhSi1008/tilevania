const mongoose = require('mongoose');

const gameProfileSchema = new mongoose.Schema(
  {
    userId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
      unique: true,
    },
    totalScore: {
      type: Number,
      default: 0,
      min: 0,
    },
    totalCoinsCollected: {
      type: Number,
      default: 0,
      min: 0,
    },
    totalEnemiesDefeated: {
      type: Number,
      default: 0,
      min: 0,
    },
    totalDeaths: {
      type: Number,
      default: 0,
      min: 0,
    },
    totalPlayTime: {
      type: Number,
      default: 0,
      min: 0, // in seconds
    },
    currentLives: {
      type: Number,
      default: 3,
      min: 0,
    },
    highestScoreAchieved: {
      type: Number,
      default: 0,
      min: 0,
    },
    lastSessionScore: {
      type: Number,
      default: 0,
      min: 0,
    },
  },
  {
    timestamps: true,
  }
);

// Indexes
gameProfileSchema.index({ userId: 1 });

module.exports = mongoose.model('GameProfile', gameProfileSchema);
