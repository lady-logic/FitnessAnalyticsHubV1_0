import { Environment } from './environment.interface';

export const environment: Environment = {
  production: false,
  apiUrl: 'http://localhost:5000', // Athleten, Activities, etc.
  aiApiUrl: 'https://localhost:5001', // AI-Services
};
