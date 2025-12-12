const swaggerJsdoc = require('swagger-jsdoc');

const options = {
  definition: {
    openapi: '3.0.0',
    info: {
      title: 'Tilevania Game Server API',
      version: '1.0.0',
      description: 'RESTful API for Tilevania 2D platformer game server',
      contact: {
        name: 'Game Development Team',
        email: 'support@tilevania.com',
      },
    },
    servers: [
      {
        url: 'http://localhost:3000',
        description: 'Development server',
      },
      {
        url: 'https://api.tilevania.com',
        description: 'Production server',
      },
    ],
    components: {
      securitySchemes: {
        BearerAuth: {
          type: 'http',
          scheme: 'bearer',
          bearerFormat: 'JWT',
          description: 'Enter JWT token',
        },
      },
      schemas: {
        User: {
          type: 'object',
          properties: {
            id: { type: 'string', description: 'User ID (MongoDB ObjectId)' },
            username: { type: 'string', description: 'Unique username' },
            email: { type: 'string', description: 'Unique email address' },
            profileImage: { type: 'string', description: 'Profile image URL' },
            createdAt: { type: 'string', format: 'date-time' },
            lastLoginAt: { type: 'string', format: 'date-time' },
          },
        },
        GameProfile: {
          type: 'object',
          properties: {
            userId: { type: 'string' },
            totalScore: { type: 'integer' },
            highestScoreAchieved: { type: 'integer' },
            totalCoinsCollected: { type: 'integer' },
            totalEnemiesDefeated: { type: 'integer' },
            totalDeaths: { type: 'integer' },
            totalPlayTime: { type: 'number' },
            currentLives: { type: 'integer' },
          },
        },
        GameSession: {
          type: 'object',
          properties: {
            sessionId: { type: 'string' },
            userId: { type: 'string' },
            levelId: { type: 'string' },
            currentScore: { type: 'integer' },
            coinsCollected: { type: 'integer' },
            enemiesDefeated: { type: 'integer' },
            sessionStatus: {
              type: 'string',
              enum: ['ACTIVE', 'COMPLETED', 'ABANDONED', 'FAILED'],
            },
            startTime: { type: 'string', format: 'date-time' },
            endTime: { type: 'string', format: 'date-time' },
            duration: { type: 'number', description: 'Duration in seconds' },
          },
        },
        Achievement: {
          type: 'object',
          properties: {
            id: { type: 'string' },
            name: { type: 'string' },
            description: { type: 'string' },
            condition: { type: 'string' },
            points: { type: 'integer' },
            rarity: {
              type: 'string',
              enum: ['COMMON', 'UNCOMMON', 'RARE', 'EPIC', 'LEGENDARY'],
            },
          },
        },
        ErrorResponse: {
          type: 'object',
          properties: {
            error: { type: 'string' },
            message: { type: 'string' },
          },
        },
      },
    },
  },
  apis: ['./src/routes/*.js'],
};

const swaggerSpec = swaggerJsdoc(options);

module.exports = swaggerSpec;
