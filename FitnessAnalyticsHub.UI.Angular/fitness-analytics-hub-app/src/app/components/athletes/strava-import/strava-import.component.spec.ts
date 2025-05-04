import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { StravaImportComponent } from './strava-import.component';
import { AthleteService } from '../../../services/athlete.service';

describe('StravaImportComponent', () => {
  let component: StravaImportComponent;
  let fixture: ComponentFixture<StravaImportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StravaImportComponent, HttpClientTestingModule],
      providers: [AthleteService],
    }).compileComponents();

    fixture = TestBed.createComponent(StravaImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
