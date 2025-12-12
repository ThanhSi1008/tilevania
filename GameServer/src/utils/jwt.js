const jwt = require('jsonwebtoken');
require('dotenv').config({ path: '/Users/xis108/Desktop/ntap/GameServer/.env' });

const JWT_SECRET = process.env.JWT_SECRET || 'your_super_secret_jwt_key_change_in_production_12345';
const TOKEN_EXPIRY = '7d';

// Generate JWT token
const generateToken = (userId, expiresIn = TOKEN_EXPIRY) => {
  try {
    const token = jwt.sign(
      { userId, iat: Math.floor(Date.now() / 1000) },
      JWT_SECRET,
      { expiresIn }
    );
    return token;
  } catch (error) {
    console.error('Error generating token:', error);
    throw error;
  }
};

// Verify JWT token
const verifyToken = (token) => {
  try {
    const decoded = jwt.verify(token, JWT_SECRET);
    return decoded;
  } catch (error) {
    console.error('Token verification failed:', error.message);
    return null;
  }
};

// Decode token without verification (just to inspect)
const decodeToken = (token) => {
  try {
    const decoded = jwt.decode(token);
    return decoded;
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
};

module.exports = {
  generateToken,
  verifyToken,
  decodeToken,
};
