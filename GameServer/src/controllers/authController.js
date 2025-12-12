const { User, GameProfile } = require('../models');
const { generateToken } = require('../utils/jwt');
const { validate, registerSchema, loginSchema } = require('../utils/validators');

// Register - tạo tài khoản mới
const register = async (req, res) => {
  try {
    // Validate input
    const validation = validate(req.body, registerSchema);
    if (!validation.valid) {
      return res.status(400).json({
        error: 'Validation Failed',
        errors: validation.errors,
      });
    }

    const { username, email, password } = validation.data;

    // Check if user already exists
    const existingUser = await User.findOne({
      $or: [{ email }, { username }],
    });

    if (existingUser) {
      return res.status(409).json({
        error: 'User Already Exists',
        message: existingUser.email === email 
          ? 'Email already in use' 
          : 'Username already in use',
      });
    }

    // Create new user
    const newUser = new User({
      username,
      email,
      passwordHash: password,
    });

    await newUser.save();

    // Create GameProfile for new user
    const gameProfile = new GameProfile({
      userId: newUser._id,
    });

    await gameProfile.save();

    // Generate token
    const token = generateToken(newUser._id.toString());

    return res.status(201).json({
      message: 'User registered successfully',
      user: {
        id: newUser._id,
        username: newUser.username,
        email: newUser.email,
        createdAt: newUser.createdAt,
      },
      token,
    });
  } catch (error) {
    console.error('Register error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Login - đăng nhập
const login = async (req, res) => {
  try {
    // Validate input
    const validation = validate(req.body, loginSchema);
    if (!validation.valid) {
      return res.status(400).json({
        error: 'Validation Failed',
        errors: validation.errors,
      });
    }

    const { email, password } = validation.data;

    // Find user by email
    const user = await User.findOne({ email }).select('+passwordHash');

    if (!user) {
      return res.status(401).json({
        error: 'Authentication Failed',
        message: 'Invalid email or password',
      });
    }

    // Compare password
    const isPasswordValid = await user.comparePassword(password);

    if (!isPasswordValid) {
      return res.status(401).json({
        error: 'Authentication Failed',
        message: 'Invalid email or password',
      });
    }

    // Update lastLoginAt
    user.lastLoginAt = new Date();
    await user.save();

    // Generate token
    const token = generateToken(user._id.toString());

    return res.status(200).json({
      message: 'Login successful',
      user: {
        id: user._id,
        username: user.username,
        email: user.email,
        lastLoginAt: user.lastLoginAt,
      },
      token,
    });
  } catch (error) {
    console.error('Login error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  register,
  login,
};
