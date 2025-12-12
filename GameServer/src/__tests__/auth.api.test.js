const request = require('supertest');
const app = require('../app');
const { connect, disconnect, clearDatabase } = require('./db.helper');
const { User, GameProfile } = require('../models');

describe('Authentication API', () => {
  beforeAll(async () => {
    await connect();
  });

  afterEach(async () => {
    await clearDatabase();
  });

  afterAll(async () => {
    await disconnect();
  });

  describe('POST /api/auth/register', () => {
    it('should register a new user successfully', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user1',
          email: 'user1@example.com',
          password: 'password123',
        });

      expect(res.status).toBe(201);
      expect(res.body).toHaveProperty('token');
      expect(res.body).toHaveProperty('user');
      expect(res.body.user.username).toBe('user1');
      expect(res.body.user.email).toBe('user1@example.com');
    });

    it('should hash the password', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user2',
          email: 'user2@example.com',
          password: 'password123',
        });

      expect(res.status).toBe(201);

      const user = await User.findById(res.body.user.id);
      expect(user.passwordHash).not.toBe('password123');
    });

    it('should create a GameProfile automatically', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user3',
          email: 'user3@example.com',
          password: 'password123',
        });

      const profile = await GameProfile.findOne({ userId: res.body.user.id });
      expect(profile).toBeDefined();
      expect(profile.totalScore).toBe(0);
      expect(profile.totalCoinsCollected).toBe(0);
    });

    it('should reject duplicate email', async () => {
      await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user4',
          email: 'dup@example.com',
          password: 'password123',
        });

      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user5',
          email: 'dup@example.com',
          password: 'password123',
        });

      expect(res.status).toBe(409);
    });

    it('should reject invalid email', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user6',
          email: 'not-an-email',
          password: 'password123',
        });

      expect(res.status).toBe(400);
    });

    it('should reject short password', async () => {
      const res = await request(app)
        .post('/api/auth/register')
        .send({
          username: 'user7',
          email: 'test7@example.com',
          password: 'short',
        });

      expect(res.status).toBe(400);
    });
  });

  describe('POST /api/auth/login', () => {
    beforeEach(async () => {
      await request(app)
        .post('/api/auth/register')
        .send({
          username: 'loginuser',
          email: 'login@example.com',
          password: 'password123',
        });
    });

    it('should login successfully with correct credentials', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'login@example.com',
          password: 'password123',
        });

      expect(res.status).toBe(200);
      expect(res.body).toHaveProperty('token');
      expect(res.body).toHaveProperty('user');
    });

    it('should return JWT token on login', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'login@example.com',
          password: 'password123',
        });

      expect(res.body.token).toBeDefined();
      expect(typeof res.body.token).toBe('string');
      expect(res.body.token.split('.')).toHaveLength(3);
    });

    it('should reject incorrect password', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'login@example.com',
          password: 'wrongpassword',
        });

      expect(res.status).toBe(401);
    });

    it('should reject non-existent email', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'nonexistent@example.com',
          password: 'password123',
        });

      expect(res.status).toBe(401);
    });

    it('should update lastLoginAt on login', async () => {
      const res = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'login@example.com',
          password: 'password123',
        });

      const user = await User.findById(res.body.user.id);
      expect(user.lastLoginAt).toBeDefined();
    });
  });
});
