const jwt = require('../utils/jwt');

describe('JWT Utilities', () => {
  const testUserId = '507f1f77bcf86cd799439011';
  const testPayload = { userId: testUserId, username: 'testuser' };

  describe('generateToken', () => {
    it('should generate a valid JWT token', () => {
      const token = jwt.generateToken(testPayload);
      expect(token).toBeDefined();
      expect(typeof token).toBe('string');
      expect(token.split('.')).toHaveLength(3); // JWT has 3 parts
    });

    it('should generate different tokens for different payloads', () => {
      const token1 = jwt.generateToken(testPayload);
      const token2 = jwt.generateToken({ userId: 'other-id' });
      expect(token1).not.toBe(token2);
    });
  });

  describe('verifyToken', () => {
    it('should verify a valid token', () => {
      const token = jwt.generateToken(testPayload);
      const decoded = jwt.verifyToken(token);
      expect(decoded).toBeDefined();
      expect(decoded.userId).toBe(testUserId);
    });

    it('should throw error for invalid token', () => {
      expect(() => jwt.verifyToken('invalid.token.here')).toThrow();
    });

    it('should throw error for expired or malformed token', () => {
      expect(() => jwt.verifyToken('')).toThrow();
    });
  });

  describe('decodeToken', () => {
    it('should decode token without verification', () => {
      const token = jwt.generateToken(testPayload);
      const decoded = jwt.decodeToken(token);
      expect(decoded).toBeDefined();
      expect(decoded.userId).toBe(testUserId);
    });

    it('should return null for invalid token', () => {
      const decoded = jwt.decodeToken('invalid');
      expect(decoded).toBeNull();
    });
  });

  describe('Token expiry', () => {
    it('generated token should have exp claim', () => {
      const token = jwt.generateToken(testPayload);
      const decoded = jwt.decodeToken(token);
      expect(decoded.exp).toBeDefined();
    });
  });
});
