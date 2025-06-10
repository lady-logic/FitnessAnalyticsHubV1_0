import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FitnessDashboardComponent } from './fitness-dashboard.component';

describe('FitnessDashboardComponent', () => {
  let component: FitnessDashboardComponent;
  let fixture: ComponentFixture<FitnessDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FitnessDashboardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FitnessDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
