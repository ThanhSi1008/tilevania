const mongoose = require('mongoose');

const gameSessionSchema = new mongoose.Schema(
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
    startTime: {
      type: Date,
      required: true,
      default: Date.now,
    },
    endTime: {
      type: Date,
      default: null,
    },
    duration: {
      type: Number,
      default: 0, // in seconds
      min: 0,
    },
    finalScore: {
      type: Number,
      default: 0,
      min: 0,
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
    livesRemaining: {
      type: Number,
      default: 3,
      min: 0,
    },
    sessionStatus: {
      type: String,
      enum: ['ACTIVE', 'COMPLETED', 'ABANDONED', 'FAILED'],
      default: 'ACTIVE',
    },
    isCompleted: {
      type: Boolean,
      default: false,
    },
  },
  {
    timestamps: true,
  }
);

// Indexes
gameSessionSchema.index({ userId: 1 });
gameSessionSchema.index({ levelId: 1 });
gameSessionSchema.index({ sessionStatus: 1 });

module.exports = mongoose.model('GameSession', gameSessionSchema);
