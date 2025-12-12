const request = require('supertest');
const app = require('../app');
const { connect, disconnect, clearDatabase } = require('./db.helper');
const { User, GameProfile, Achievement, PlayerAchievement } = require('../models');

describe('Achievement API', () => {
  let user;
  let gameProfile;
  let token;
  let achievements;

  beforeAll(async () => {
    await connect();
  });

  afterEach(async () => {
    await clearDatabase();
  });

  afterAll(async () => {
    await disconnect();
  });

  beforeEach(async () => {
    // Register and login user
    const registerRes = await request(app)
      .post('/api/auth/register')
      .send({
        username: 'testuser',
        email: 'test@example.com',
        password: 'password123',
      });

    user = registerRes.body.user;
    token = registerRes.body.token;
    gameProfile = await GameProfile.findOne({ userId: user.id });

    // Create achievements
    achievements = await Achievement.create([
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
        name: 'Score Master',
        description: 'Achieve 1000 points',
        condition: 'SCORE_MASTER_1000',
        points: 50,
        rarity: 'RARE',
      },
    ]);
  });

  describe('GET /api/achievements', () => {
    it('should return all achievements', async () => {
      const res = await request(app)
        .get('/api/achievements');

      expect(res.status).toBe(200);
      expect(Array.isArray(res.body)).toBe(true);
      expect(res.body.length).toBeGreaterThanOrEqual(3);
    });

    it('should return achievement details', async () => {
      const res = await request(app)
        .get('/api/achievements');

      const firstAchievement = res.body[0];
      expect(firstAchievement).toHaveProperty('name');
      expect(firstAchievement).toHaveProperty('description');
      expect(firstAchievement).toHaveProperty('points');
      expect(firstAchievement).toHaveProperty('rarity');
    });
  });

  describe('POST /api/achievements/:userId/unlock/:achievementId', () => {
    it('should unlock achievement for user', async () => {
      const res = await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});

      expect(res.status).toBe(200);
      expect(res.body).toHaveProperty('message');
    });

    it('should create PlayerAchievement record', async () => {
      await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});

      const playerAchievement = await PlayerAchievement.findOne({
        userId: user.id,
        achievementId: achievements[0]._id,
      });

      expect(playerAchievement).toBeDefined();
      expect(playerAchievement.unlockedAt).toBeDefined();
    });

    it('should add achievement points to user score', async () => {
      const initialScore = gameProfile.totalScore;

      await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});

      const updatedProfile = await GameProfile.findById(gameProfile._id);
      expect(updatedProfile.totalScore).toBe(initialScore + achievements[0].points);
    });

    it('should prevent duplicate achievement unlocks', async () => {
      // First unlock
      await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});

      // Second unlock attempt
      const res = await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});

      expect(res.status).toBe(400);
    });
  });

  describe('GET /api/achievements/:userId/unlocked', () => {
    beforeEach(async () => {
      // Unlock first achievement
      await request(app)
        .post(`/api/achievements/${user.id}/unlock/${achievements[0]._id}`)
        .set('Authorization', `Bearer ${token}`)
        .send({});
    });

    it('should return user unlocked achievements', async () => {
      const res = await request(app)
        .get(`/api/achievements/${user.id}/unlocked`)
        .set('Authorization', `Bearer ${token}`);

      expect(res.status).toBe(200);
      expect(Array.isArray(res.body)).toBe(true);
      expect(res.body.length).toBe(1);
    });

    it('should return correct achievement details', async () => {
      const res = await request(app)
        .get(`/api/achievements/${user.id}/unlocked`)
        .set('Authorization', `Bearer ${token}`);

      const unlockedAchievement = res.body[0];
      expect(unlockedAchievement.name).toBe('First Step');
      expect(unlockedAchievement.points).toBe(10);
    });
  });
});
