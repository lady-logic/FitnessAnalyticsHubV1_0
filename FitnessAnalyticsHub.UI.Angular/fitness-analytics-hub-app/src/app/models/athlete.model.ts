export interface Athlete {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: Date;
  weight: number;
  height: number;
  // Füge hier weitere Eigenschaften hinzu, die in deinem AthleteDto enthalten sind
}

export interface CreateAthleteDto {
  name: string;
  email: string;
  dateOfBirth: Date;
  weight: number;
  height: number;
  // Weitere Eigenschaften entsprechend deinem CreateAthleteDto
}

export interface UpdateAthleteDto {
  id: number;
  name: string;
  email: string;
  dateOfBirth: Date;
  weight: number;
  height: number;
  // Weitere Eigenschaften entsprechend deinem UpdateAthleteDto
}
