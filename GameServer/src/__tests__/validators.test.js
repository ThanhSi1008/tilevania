const { validate, registerSchema, loginSchema, updateUserSchema } = require('../utils/validators');

describe('Input Validators', () => {
  describe('registerSchema', () => {
    it('should validate correct registration data', () => {
      const validData = {
        username: 'testuser',
        email: 'test@example.com',
        password: 'password123',
      };
      const result = validate(validData, registerSchema);
      expect(result.error).toBeUndefined();
      expect(result.value).toEqual(validData);
    });

    it('should reject missing email', () => {
      const invalidData = {
        username: 'testuser',
        password: 'password123',
      };
      const result = validate(invalidData, registerSchema);
      expect(result.error).toBeDefined();
    });

    it('should reject invalid email format', () => {
      const invalidData = {
        username: 'testuser',
        email: 'not-an-email',
        password: 'password123',
      };
      const result = validate(invalidData, registerSchema);
      expect(result.error).toBeDefined();
    });

    it('should reject short passwords', () => {
      const invalidData = {
        username: 'testuser',
        email: 'test@example.com',
        password: '123', // Too short
      };
      const result = validate(invalidData, registerSchema);
      expect(result.error).toBeDefined();
    });

    it('should reject missing username', () => {
      const invalidData = {
        email: 'test@example.com',
        password: 'password123',
      };
      const result = validate(invalidData, registerSchema);
      expect(result.error).toBeDefined();
    });
  });

  describe('loginSchema', () => {
    it('should validate correct login data', () => {
      const validData = {
        email: 'test@example.com',
        password: 'password123',
      };
      const result = validate(validData, loginSchema);
      expect(result.error).toBeUndefined();
    });

    it('should reject missing email', () => {
      const invalidData = {
        password: 'password123',
      };
      const result = validate(invalidData, loginSchema);
      expect(result.error).toBeDefined();
    });

    it('should reject invalid email', () => {
      const invalidData = {
        email: 'invalid-email',
        password: 'password123',
      };
      const result = validate(invalidData, loginSchema);
      expect(result.error).toBeDefined();
    });

    it('should reject missing password', () => {
      const invalidData = {
        email: 'test@example.com',
      };
      const result = validate(invalidData, loginSchema);
      expect(result.error).toBeDefined();
    });
  });

  describe('updateUserSchema', () => {
    it('should validate correct update data', () => {
      const validData = {
        username: 'newusername',
        profileImage: 'https://example.com/image.jpg',
      };
      const result = validate(validData, updateUserSchema);
      expect(result.error).toBeUndefined();
    });

    it('should allow partial updates', () => {
      const validData = {
        username: 'newusername',
      };
      const result = validate(validData, updateUserSchema);
      expect(result.error).toBeUndefined();
    });

    it('should allow empty object', () => {
      const validData = {};
      const result = validate(validData, updateUserSchema);
      expect(result.error).toBeUndefined();
    });

    it('should reject invalid profile image URL', () => {
      const invalidData = {
        profileImage: 'not-a-url',
      };
      const result = validate(invalidData, updateUserSchema);
      expect(result.error).toBeDefined();
    });
  });
});
