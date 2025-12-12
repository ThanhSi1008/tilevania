const mongoose = require('mongoose');

const achievementSchema = new mongoose.Schema(
  {
    name: {
      type: String,
      required: [true, 'Achievement name is required'],
      unique: true,
    },
    description: {
      type: String,
      default: '',
    },
    icon: {
      type: String,
      default: null, // URL to icon
    },
    condition: {
      type: String,
      required: [true, 'Condition is required'],
      // Examples: "FIRST_KILL", "COIN_COLLECTOR_100", "NO_DEATH_LEVEL", etc.
    },
    points: {
      type: Number,
      default: 10,
      min: 0,
    },
    rarity: {
      type: String,
      enum: ['COMMON', 'RARE', 'EPIC', 'LEGENDARY'],
      default: 'COMMON',
    },
  },
  {
    timestamps: true,
  }
);

// Indexes
achievementSchema.index({ condition: 1 });

module.exports = mongoose.model('Achievement', achievementSchema);
