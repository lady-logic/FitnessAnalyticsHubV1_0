import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AthleteEditComponent } from './athlete-edit.component';
import { AthleteService } from '../../../services/athlete.service';

describe('AthleteEditComponent', () => {
  let component: AthleteEditComponent;
  let fixture: ComponentFixture<AthleteEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AthleteEditComponent,
        HttpClientTestingModule,
        RouterTestingModule,
      ],
      providers: [AthleteService],
    }).compileComponents();

    fixture = TestBed.createComponent(AthleteEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
