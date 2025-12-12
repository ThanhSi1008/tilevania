const mongoose = require('mongoose');
require('dotenv').config({ path: '/Users/xis108/Desktop/ntap/GameServer/.env' });

const { calculateLeaderboards } = require('../controllers/leaderboardController');

const calculateAndSave = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('Connected to MongoDB...\n');

    await calculateLeaderboards();

    console.log('\n✅ Leaderboard calculated!');
    process.exit(0);
  } catch (error) {
    console.error('❌ Error:', error);
    process.exit(1);
  }
};

calculateAndSave();
