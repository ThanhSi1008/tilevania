const mongoose = require('mongoose');

const levelProgressSchema = new mongoose.Schema(
  {
    userId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
    },
    levelId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'Level',
      required: true,
    },
    isCompleted: {
      type: Boolean,
      default: false,
    },
    coinsCollected: {
      type: Number,
      default: 0,
      min: 0,
    },
    enemiesDefeated: {
      type: Number,
      default: 0,
      min: 0,
    },
    deathCount: {
      type: Number,
      default: 0,
      min: 0,
    },
    bestScore: {
      type: Number,
      default: 0,
      min: 0,
    },
    bestTime: {
      type: Number,
      default: null,
      min: 0, // in seconds
    },
    playCount: {
      type: Number,
      default: 0,
      min: 0,
    },
    lastPlayedAt: {
      type: Date,
      default: null,
    },
    completedAt: {
      type: Date,
      default: null,
    },
  },
  {
    timestamps: true,
  }
);

// Indexes for faster queries
levelProgressSchema.index({ userId: 1, levelId: 1 }, { unique: true });
levelProgressSchema.index({ userId: 1 });
levelProgressSchema.index({ levelId: 1 });

module.exports = mongoose.model('LevelProgress', levelProgressSchema);
