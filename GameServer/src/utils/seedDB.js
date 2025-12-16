const mongoose = require('mongoose');
require('dotenv').config({ path: '/Users/xis108/Desktop/ntap/GameServer/.env' });

const { Level, Achievement, User, GameProfile } = require('../models');
const { calculateLeaderboards } = require('../controllers/leaderboardController');

// Simple helper to create users sequentially (so password pre-save hook runs)
const createUserIfMissing = async (user) => {
  const existing = await User.findOne({ email: user.email });
  if (existing) return existing;
  const created = await User.create(user);
  console.log(`   - Created user ${created.username} (${created.email})`);
  return created;
};

const createGameProfileIfMissing = async (userId, profile) => {
  const existing = await GameProfile.findOne({ userId });
  if (existing) return existing;
  const created = await GameProfile.create({ userId, ...profile });
  console.log(`   - Created game profile for userId=${userId} score=${profile.totalScore}`);
  return created;
};

const seedDatabase = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('Connected to MongoDB for seeding...\n');

    // Seed levels
    const existingLevels = await Level.countDocuments();
    if (existingLevels === 0) {
      const defaultLevels = [
        {
          levelNumber: 1,
          levelName: 'Level 1',
          description: 'Start your adventure at the castle entrance. Collect coins and defeat enemies!',
          difficulty: 'EASY',
          maxCoins: 50,
          maxEnemies: 10,
          sceneName: 'Level 1',
          isUnlocked: true,
        },
        {
          levelNumber: 2,
          levelName: 'Level 2',
          description: 'Navigate through the dark forest. Be careful of the shadows!',
          difficulty: 'NORMAL',
          maxCoins: 75,
          maxEnemies: 15,
          sceneName: 'Level 2',
          isUnlocked: false,
          requiredScoreToUnlock: 100,
        },
        {
          levelNumber: 3,
          levelName: 'Level 3',
          description: 'Face the ultimate challenge in the dragon lair!',
          difficulty: 'HARD',
          maxCoins: 100,
          maxEnemies: 20,
          sceneName: 'Level 3',
          isUnlocked: false,
          requiredScoreToUnlock: 300,
        },
      ];

      const insertedLevels = await Level.insertMany(defaultLevels);
      console.log(`✅ Seeded ${insertedLevels.length} levels:`);
      insertedLevels.forEach((level) => {
        console.log(`   - Level ${level.levelNumber}: ${level.levelName}`);
      });
    } else {
      console.log('⏭️  Levels already seeded, skipping...\n');
    }

    // Seed achievements
    const existingAchievements = await Achievement.countDocuments();
    if (existingAchievements === 0) {
      const defaultAchievements = [
        {
          name: 'First Step',
          description: 'Defeat your first enemy',
          condition: 'FIRST_KILL',
          points: 10,
          rarity: 'COMMON',
        },
        {
          name: 'Coin Collector',
          description: 'Collect 100 coins',
          condition: 'COIN_COLLECTOR_100',
          points: 25,
          rarity: 'COMMON',
        },
        {
          name: 'Fortune Hunter',
          description: 'Collect 500 coins',
          condition: 'COIN_COLLECTOR_500',
          points: 50,
          rarity: 'RARE',
        },
        {
          name: 'Monster Slayer',
          description: 'Defeat 100 enemies',
          condition: 'KILLER_100',
          points: 75,
          rarity: 'RARE',
        },
        {
          name: 'Score Master',
          description: 'Reach 1000 points',
          condition: 'SCORE_MASTER_1000',
          points: 50,
          rarity: 'RARE',
        },
        {
          name: 'Score Legend',
          description: 'Reach 5000 points',
          condition: 'SCORE_MASTER_5000',
          points: 150,
          rarity: 'EPIC',
        },
        {
          name: 'Dedicated Player',
          description: 'Play for 1 hour',
          condition: 'PLAYTIME_HOUR',
          points: 40,
          rarity: 'COMMON',
        },
        {
          name: 'Obsessed Gamer',
          description: 'Play for 24 hours',
          condition: 'PLAYTIME_DAY',
          points: 200,
          rarity: 'LEGENDARY',
        },
      ];

      const insertedAchievements = await Achievement.insertMany(defaultAchievements);
      console.log(`\n✅ Seeded ${insertedAchievements.length} achievements:`);
      insertedAchievements.forEach((achievement) => {
        console.log(`   - ${achievement.name} (${achievement.rarity}): ${achievement.description}`);
      });
    } else {
      console.log('⏭️  Achievements already seeded, skipping...\n');
    }

    // Seed demo users & profiles for leaderboard
    const defaultUsers = [
      { username: 'alice', email: 'alice@example.com', passwordHash: 'Password123!' , profileImage: null },
      { username: 'bob',   email: 'bob@example.com',   passwordHash: 'Password123!' , profileImage: null },
      { username: 'carol', email: 'carol@example.com', passwordHash: 'Password123!' , profileImage: null },
      { username: 'dave',  email: 'dave@example.com',  passwordHash: 'Password123!' , profileImage: null },
    ];

    const seededUsers = [];
    for (const u of defaultUsers) {
      const created = await createUserIfMissing(u);
      seededUsers.push(created);
    }

    // Attach game profiles with scores for leaderboard
    const demoProfiles = [
      { email: 'alice@example.com', totalScore: 5200, totalCoinsCollected: 800, totalEnemiesDefeated: 120 },
      { email: 'bob@example.com',   totalScore: 3400, totalCoinsCollected: 500, totalEnemiesDefeated: 90 },
      { email: 'carol@example.com', totalScore: 2100, totalCoinsCollected: 300, totalEnemiesDefeated: 60 },
      { email: 'dave@example.com',  totalScore: 900,  totalCoinsCollected: 120, totalEnemiesDefeated: 20 },
    ];

    for (const profile of demoProfiles) {
      const user = await User.findOne({ email: profile.email });
      if (!user) {
        console.warn(`⚠️  Skipping profile seed, user not found: ${profile.email}`);
        continue;
      }

      await createGameProfileIfMissing(user._id, {
        totalScore: profile.totalScore,
        totalCoinsCollected: profile.totalCoinsCollected,
        totalEnemiesDefeated: profile.totalEnemiesDefeated,
        totalDeaths: 0,
        totalPlayTime: 0,
        currentLives: 3,
        highestScoreAchieved: profile.totalScore,
        lastSessionScore: profile.totalScore,
      });
    }

    // Recalculate leaderboards from GameProfiles
    await calculateLeaderboards();
    console.log('\n✅ Leaderboards seeded from demo profiles.');

    console.log('\n✅ Database seeding completed!');
    process.exit(0);
  } catch (error) {
    console.error('❌ Seeding error:', error);
    process.exit(1);
  }
};

seedDatabase();
