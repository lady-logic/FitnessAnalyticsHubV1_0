import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AthleteDetailComponent } from './athlete-detail.component';
import { AthleteService } from '../../../services/athlete.service';

describe('AthleteDetailComponent', () => {
  let component: AthleteDetailComponent;
  let fixture: ComponentFixture<AthleteDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AthleteDetailComponent,
        HttpClientTestingModule,
        RouterTestingModule,
      ],
      providers: [AthleteService],
    }).compileComponents();

    fixture = TestBed.createComponent(AthleteDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
