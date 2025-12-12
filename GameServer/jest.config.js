module.exports = {
  testEnvironment: 'node',
  coverageDirectory: 'coverage',
  collectCoverageFrom: [
    'src/**/*.js',
    '!src/**/index.js',
    '!src/config/**',
  ],
  testMatch: ['**/__tests__/**/*.js', '**/*.test.js'],
  coverageThreshold: {
    global: {
      branches: 50,
      functions: 50,
      lines: 70,
      statements: 70,
    },
  },
  setupFilesAfterEnv: ['<rootDir>/src/__tests__/db.setup.js'],
  testTimeout: 10000,
  verbose: true,
};
