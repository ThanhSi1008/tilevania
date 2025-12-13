const { Level } = require('../models');

// Get all levels
const getAllLevels = async (req, res) => {
  try {
    const levels = await Level.find().sort({ levelNumber: 1 });

    return res.status(200).json({
      count: levels.length,
      levels,
    });
  } catch (error) {
    console.error('Get all levels error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Get level by ID
const getLevelById = async (req, res) => {
  try {
    const { levelId } = req.params;

    const level = await Level.findById(levelId);

    if (!level) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Level not found',
      });
    }

    return res.status(200).json({ level });
  } catch (error) {
    console.error('Get level by ID error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Create level (admin only)
const createLevel = async (req, res) => {
  try {
    const { 
      levelNumber, 
      levelName, 
      description, 
      difficulty, 
      maxCoins, 
      maxEnemies, 
      sceneName,
      isUnlocked,
      requiredScoreToUnlock
    } = req.body;

    // Validate required fields
    if (!levelNumber || !levelName || !sceneName || maxCoins === undefined || maxEnemies === undefined) {
      return res.status(400).json({
        error: 'Validation Failed',
        message: 'Missing required fields',
      });
    }

    // Check if level already exists
    const existingLevel = await Level.findOne({ levelNumber });
    if (existingLevel) {
      return res.status(409).json({
        error: 'Level Already Exists',
        message: 'Level with this number already exists',
      });
    }

    // Create new level
    const newLevel = new Level({
      levelNumber,
      levelName,
      description: description || '',
      difficulty: difficulty || 'NORMAL',
      maxCoins,
      maxEnemies,
      sceneName,
      isUnlocked: isUnlocked !== undefined ? isUnlocked : false,
      requiredScoreToUnlock: requiredScoreToUnlock !== undefined ? requiredScoreToUnlock : 0,
    });

    await newLevel.save();

    return res.status(201).json({
      message: 'Level created successfully',
      level: newLevel,
    });
  } catch (error) {
    console.error('Create level error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

// Update level (admin only)
const updateLevel = async (req, res) => {
  try {
    const { levelId } = req.params;
    const updateData = req.body;

    const updatedLevel = await Level.findByIdAndUpdate(
      levelId,
      updateData,
      { new: true, runValidators: true }
    );

    if (!updatedLevel) {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Level not found',
      });
    }

    return res.status(200).json({
      message: 'Level updated successfully',
      level: updatedLevel,
    });
  } catch (error) {
    console.error('Update level error:', error);
    return res.status(500).json({
      error: 'Internal Server Error',
      message: error.message,
    });
  }
};

module.exports = {
  getAllLevels,
  getLevelById,
  createLevel,
  updateLevel,
};
