// @ts-check
const angular = require('angular-eslint');

module.exports = [
  {
    ignores: ['dist/**', '.angular/**', 'coverage/**']
  },
  ...angular.configs.tsRecommended,
  ...angular.configs.templateRecommended,
  {
    files: ['**/*.ts'],
    rules: {
      '@angular-eslint/prefer-standalone': 'error'
    }
  }
];
