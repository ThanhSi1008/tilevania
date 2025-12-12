const mongoose = require('mongoose');

const levelSchema = new mongoose.Schema(
  {
    levelNumber: {
      type: Number,
      required: [true, 'Level number is required'],
      unique: true,
      min: 1,
    },
    levelName: {
      type: String,
      required: [true, 'Level name is required'],
    },
    description: {
      type: String,
      default: '',
    },
    difficulty: {
      type: String,
      enum: ['EASY', 'NORMAL', 'HARD'],
      default: 'NORMAL',
    },
    maxCoins: {
      type: Number,
      required: true,
      min: 0,
    },
    maxEnemies: {
      type: Number,
      required: true,
      min: 0,
    },
    sceneName: {
      type: String,
      required: [true, 'Scene name is required'],
    },
    isUnlocked: {
      type: Boolean,
      default: false,
    },
    requiredScoreToUnlock: {
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
levelSchema.index({ levelNumber: 1 });

module.exports = mongoose.model('Level', levelSchema);
