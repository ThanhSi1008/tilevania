const mongoose = require('mongoose');
require('dotenv').config();

const connectDB = async () => {
  try {
    const mongoUri = process.env.MONGODB_URI;
    
    if (!mongoUri) {
      throw new Error('MONGODB_URI is not defined in .env file');
    }

    const conn = await mongoose.connect(mongoUri);

    console.log(`\n✅ MongoDB connected successfully`);
    console.log(`📍 Connected to: ${conn.connection.host}`);
    console.log(`🗄️  Database: ${conn.connection.name}\n`);

    return conn;
  } catch (error) {
    console.error('\n❌ MongoDB connection failed');
    console.error(`Error: ${error.message}\n`);
    process.exit(1);
  }
};

// Connect to database
connectDB();

// Listen to connection events
mongoose.connection.on('connected', () => {
  console.log('Mongoose connected to MongoDB');
});

mongoose.connection.on('disconnected', () => {
  console.log('Mongoose disconnected from MongoDB');
});

mongoose.connection.on('error', (error) => {
  console.error('Mongoose connection error:', error);
});

module.exports = mongoose.connection;
