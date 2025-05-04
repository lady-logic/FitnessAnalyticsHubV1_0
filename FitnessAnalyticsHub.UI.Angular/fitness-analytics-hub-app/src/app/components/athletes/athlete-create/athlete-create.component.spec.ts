import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AthleteCreateComponent } from './athlete-create.component';
import { AthleteService } from '../../../services/athlete.service';

describe('AthleteCreateComponent', () => {
  let component: AthleteCreateComponent;
  let fixture: ComponentFixture<AthleteCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AthleteCreateComponent,
        HttpClientTestingModule,
        RouterTestingModule,
      ],
      providers: [AthleteService],
    }).compileComponents();

    fixture = TestBed.createComponent(AthleteCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
