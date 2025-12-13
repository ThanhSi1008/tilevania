// Use the same database config as the server
require('dotenv').config();
const mongoose = require('mongoose');
const { GameProfile } = require('./src/models');
const { Level } = require('./src/models');

// MongoDB connection string from .env or use provided one
const MONGODB_URI = process.env.MONGODB_URI || "mongodb+srv://xis:xis@tilevania.mdipeqi.mongodb.net/tilevania?retryWrites=true&w=majority&appName=tilevania";

// Connect to MongoDB
mongoose.connect(MONGODB_URI, {
  useNewUrlParser: true,
  useUnifiedTopology: true,
})
.then(() => {
  console.log('✅ Connected to MongoDB');
  checkCurrentLevels();
})
.catch((error) => {
  console.error('❌ MongoDB connection error:', error);
  process.exit(1);
});

async function checkCurrentLevels() {
  try {
    // Get all game profiles with currentLevel populated
    const gameProfiles = await GameProfile.find({})
      .populate('currentLevel', '_id levelName levelNumber')
      .populate('userId', 'username email');
    
    console.log(`\n📊 Found ${gameProfiles.length} game profiles\n`);
    
    for (const profile of gameProfiles) {
      console.log(`User: ${profile.userId?.username || profile.userId} (${profile.userId?._id || profile.userId})`);
      if (profile.currentLevel) {
        console.log(`  Current Level: ${profile.currentLevel.levelName} (Level ${profile.currentLevel.levelNumber})`);
        console.log(`  Current Level ID: ${profile.currentLevel._id}`);
      } else {
        console.log(`  Current Level: null (will start from Level 1)`);
      }
      console.log(`  Current Lives: ${profile.currentLives}`);
      console.log(`  Total Deaths: ${profile.totalDeaths}`);
      console.log(`  Total Score: ${profile.totalScore}`);
      console.log('');
    }
    
    // Get all levels to show mapping
    const levels = await Level.find({}).sort({ levelNumber: 1 });
    console.log(`\n📋 Available Levels:\n`);
    for (const level of levels) {
      console.log(`  Level ${level.levelNumber}: ${level.levelName} (ID: ${level._id})`);
    }
    
    mongoose.connection.close();
    console.log('\n✅ Check completed');
  } catch (error) {
    console.error('❌ Error checking current levels:', error);
    mongoose.connection.close();
    process.exit(1);
  }
}

