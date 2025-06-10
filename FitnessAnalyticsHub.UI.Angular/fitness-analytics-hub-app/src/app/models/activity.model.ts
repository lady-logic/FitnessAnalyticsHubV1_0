export interface ActivityDto {
  id: number;
  name: string;
  sportType: string;
  distance: number;
  startDate: string;
  athleteFullName: string;
  duration?: number;
  averageHeartRate?: number;
  maxHeartRate?: number;
  calories?: number;
}

export interface ActivityStatistics {
  totalActivities: number;
  totalDistance: number;
  totalDuration: string;
  activitiesByType: { [key: string]: number };
  activitiesByMonth: { [key: number]: number };
  averageDistance?: number;
  longestDistance?: number;
  mostCommonSport?: string;
}

export interface WorkoutData {
  date: string;
  activityType: string;
  distance: number;
  duration?: number;
  heartRate?: number;
  calories?: number;
}

export interface AIAnalysis {
  analysis: string;
  keyInsights: string[];
  recommendations?: string[];
  performanceScore?: number;
  trends?: {
    direction: 'up' | 'down' | 'stable';
    description: string;
  };
}
