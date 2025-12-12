const { verifyToken } = require('../utils/jwt');

// Middleware to authenticate JWT token
const authenticateToken = (req, res, next) => {
  try {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

    if (!token) {
      return res.status(401).json({
        error: 'Access Denied',
        message: 'No token provided',
      });
    }

    const decoded = verifyToken(token);
    if (!decoded) {
      return res.status(403).json({
        error: 'Invalid Token',
        message: 'Token is invalid or expired',
      });
    }

    // Provide both req.userId and req.user for compatibility
    req.userId = decoded.userId;
    req.user = { id: decoded.userId };
    next();
  } catch (error) {
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Middleware to authorize user (check if user is accessing their own data)
const authorizeUser = (req, res, next) => {
  try {
    const userIdFromParam = req.params.userId;
    const userIdFromToken = req.userId;

    if (userIdFromParam !== userIdFromToken) {
      return res.status(403).json({
        error: 'Forbidden',
        message: 'You do not have permission to access this resource',
      });
    }

    next();
  } catch (error) {
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  authenticateToken,
  authorizeUser,
};
