const mongoose = require('mongoose');

const leaderboardSchema = new mongoose.Schema(
  {
    userId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
    },
    rank: {
      type: Number,
      required: true,
      min: 1,
    },
    totalScore: {
      type: Number,
      required: true,
      min: 0,
    },
    username: {
      type: String,
      required: true,
    },
    profileImage: {
      type: String,
      default: null,
    },
    period: {
      type: String,
      enum: ['ALLTIME', 'WEEKLY', 'DAILY'],
      required: true,
    },
    calculatedAt: {
      type: Date,
      default: Date.now,
    },
  },
  {
    timestamps: true,
  }
);

// Indexes for faster queries
leaderboardSchema.index({ period: 1, rank: 1 });
leaderboardSchema.index({ userId: 1, period: 1 }, { unique: true });
leaderboardSchema.index({ totalScore: -1, period: 1 });

module.exports = mongoose.model('Leaderboard', leaderboardSchema);
