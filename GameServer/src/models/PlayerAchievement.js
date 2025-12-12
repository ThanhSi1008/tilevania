const mongoose = require('mongoose');

const playerAchievementSchema = new mongoose.Schema(
  {
    userId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
    },
    achievementId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'Achievement',
      required: true,
    },
    unlockedAt: {
      type: Date,
      default: Date.now,
    },
    progress: {
      type: Number,
      default: 0,
      min: 0,
      max: 100,
    },
  },
  {
    timestamps: true,
  }
);

// Unique index - user can't unlock same achievement twice
playerAchievementSchema.index({ userId: 1, achievementId: 1 }, { unique: true });
playerAchievementSchema.index({ userId: 1 });

module.exports = mongoose.model('PlayerAchievement', playerAchievementSchema);
