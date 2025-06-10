import { TestBed } from '@angular/core/testing';

import { FitnessService } from './fitness.service';

describe('FitnessService', () => {
  let service: FitnessService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FitnessService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
