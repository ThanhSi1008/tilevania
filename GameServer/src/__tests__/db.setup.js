// Test setup file
process.env.NODE_ENV = 'test';
process.env.MONGODB_URI = 'mongodb+srv://xis:xis@tilevania.mdipeqi.mongodb.net/tilevania?retryWrites=true&w=majority&appName=tilevania';
process.env.JWT_SECRET = 'test-secret-key-do-not-use-in-production';
process.env.PORT = 3001;

// Set longer timeout for MongoDB operations
jest.setTimeout(10000);
