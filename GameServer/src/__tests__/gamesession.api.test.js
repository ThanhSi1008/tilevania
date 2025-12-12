const request = require('supertest');
const app = require('../app');
const { connect, disconnect, clearDatabase } = require('./db.helper');
const { User, GameProfile, Level, GameSession, LevelProgress } = require('../models');

describe('Game Session API', () => {
  let user;
  let gameProfile;
  let level;
  let token;

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

    // Create a level
    level = await Level.create({
      levelNumber: 1,
      name: 'Castle Entrance',
      difficulty: 'EASY',
      maxCoins: 50,
      maxEnemies: 10,
    });
  });

  describe('POST /api/sessions', () => {
    it('should start a game session', async () => {
      const res = await request(app)
        .post('/api/sessions')
        .set('Authorization', `Bearer ${token}`)
        .send({
          userId: user.id,
          levelId: level._id,
        });

      expect(res.status).toBe(201);
      expect(res.body).toHaveProperty('sessionId');
      expect(res.body.sessionStatus).toBe('ACTIVE');
    });

    it('should create session with correct user and level', async () => {
      const res = await request(app)
        .post('/api/sessions')
        .set('Authorization', `Bearer ${token}`)
        .send({
          userId: user.id,
          levelId: level._id,
        });

      const session = await GameSession.findById(res.body.sessionId);
      expect(session.userId.toString()).toBe(user.id);
      expect(session.levelId.toString()).toBe(level._id.toString());
    });
  });

  describe('PUT /api/sessions/:sessionId', () => {
    let sessionId;

    beforeEach(async () => {
      const res = await request(app)
        .post('/api/sessions')
        .set('Authorization', `Bearer ${token}`)
        .send({
          userId: user.id,
          levelId: level._id,
        });
      sessionId = res.body.sessionId;
    });

    it('should update session with score and coins', async () => {
      const res = await request(app)
        .put(`/api/sessions/${sessionId}`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          currentScore: 250,
          coinsCollected: 30,
          enemiesDefeated: 5,
        });

      expect(res.status).toBe(200);
      expect(res.body.currentScore).toBe(250);
      expect(res.body.coinsCollected).toBe(30);
    });
  });

  describe('POST /api/sessions/:sessionId/end', () => {
    let sessionId;

    beforeEach(async () => {
      const startRes = await request(app)
        .post('/api/sessions')
        .set('Authorization', `Bearer ${token}`)
        .send({
          userId: user.id,
          levelId: level._id,
        });
      sessionId = startRes.body.sessionId;

      // Update session with stats
      await request(app)
        .put(`/api/sessions/${sessionId}`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          currentScore: 500,
          coinsCollected: 45,
          enemiesDefeated: 8,
        });
    });

    it('should end session and mark as COMPLETED', async () => {
      const res = await request(app)
        .post(`/api/sessions/${sessionId}/end`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          sessionStatus: 'COMPLETED',
        });

      expect(res.status).toBe(200);
      expect(res.body.sessionStatus).toBe('COMPLETED');
    });

    it('should update game profile stats after session ends', async () => {
      await request(app)
        .post(`/api/sessions/${sessionId}/end`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          sessionStatus: 'COMPLETED',
        });

      const updatedProfile = await GameProfile.findById(gameProfile._id);
      expect(updatedProfile.totalScore).toBeGreaterThan(0);
      expect(updatedProfile.totalCoinsCollected).toBeGreaterThan(0);
    });

    it('should update level progress after session ends', async () => {
      await request(app)
        .post(`/api/sessions/${sessionId}/end`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          sessionStatus: 'COMPLETED',
        });

      const progress = await LevelProgress.findOne({
        userId: user.id,
        levelId: level._id,
      });

      expect(progress).toBeDefined();
      expect(progress.playCount).toBeGreaterThan(0);
    });
  });

  describe('GET /api/sessions/:userId/history', () => {
    it('should return session history for user', async () => {
      // Create a session first
      const sessionRes = await request(app)
        .post('/api/sessions')
        .set('Authorization', `Bearer ${token}`)
        .send({
          userId: user.id,
          levelId: level._id,
        });

      // End the session
      await request(app)
        .post(`/api/sessions/${sessionRes.body.sessionId}/end`)
        .set('Authorization', `Bearer ${token}`)
        .send({
          sessionStatus: 'COMPLETED',
        });

      // Get history
      const res = await request(app)
        .get(`/api/sessions/${user.id}/history`)
        .set('Authorization', `Bearer ${token}`);

      expect(res.status).toBe(200);
      expect(Array.isArray(res.body)).toBe(true);
      expect(res.body.length).toBeGreaterThan(0);
    });
  });
});
