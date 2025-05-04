import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { AthleteListComponent } from './athlete-list.component';
import { AthleteService } from '../../../services/athlete.service';

describe('AthleteListComponent', () => {
  let component: AthleteListComponent;
  let fixture: ComponentFixture<AthleteListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AthleteListComponent, HttpClientTestingModule],
      providers: [AthleteService],
    }).compileComponents();

    fixture = TestBed.createComponent(AthleteListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
