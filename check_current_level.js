const mongoose = require('mongoose');

// MongoDB connection string
const MONGODB_URI = "mongodb+srv://xis:xis@tilevania.mdipeqi.mongodb.net/tilevania?retryWrites=true&w=majority&appName=tilevania";

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
    const db = mongoose.connection.db;
    
    // Get all game profiles with currentLevel
    const gameProfiles = await db.collection('gameprofiles').find({}).toArray();
    
    console.log(`\n📊 Found ${gameProfiles.length} game profiles\n`);
    
    for (const profile of gameProfiles) {
      console.log(`User ID: ${profile.userId}`);
      console.log(`  Current Level ID: ${profile.currentLevel || 'null'}`);
      console.log(`  Current Lives: ${profile.currentLives}`);
      console.log(`  Total Deaths: ${profile.totalDeaths}`);
      
      // Try to get level info if currentLevel exists
      if (profile.currentLevel) {
        const level = await db.collection('levels').findOne({ _id: profile.currentLevel });
        if (level) {
          console.log(`  Current Level Name: ${level.levelName} (Level ${level.levelNumber})`);
        } else {
          console.log(`  ⚠️ Level not found in levels collection`);
        }
      }
      console.log('');
    }
    
    // Get all levels to show mapping
    const levels = await db.collection('levels').find({}).toArray();
    console.log(`\n📋 Available Levels:\n`);
    for (const level of levels) {
      console.log(`  ${level._id} -> ${level.levelName} (Level ${level.levelNumber})`);
    }
    
    mongoose.connection.close();
    console.log('\n✅ Check completed');
  } catch (error) {
    console.error('❌ Error checking current levels:', error);
    mongoose.connection.close();
    process.exit(1);
  }
}

