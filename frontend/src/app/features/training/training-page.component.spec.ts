import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TrainingPageComponent } from './training-page.component';

describe('TrainingPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainingPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the training management heading', () => {
    const fixture = TestBed.createComponent(TrainingPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/training-sessions').flush([]);
    httpMock.expectOne('/api/v1/training-sessions/1/participants').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Training management');
  });

  it('lists loaded sessions', () => {
    const fixture = TestBed.createComponent(TrainingPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/training-sessions').flush([
      { id: 't1', title: 'Confined space training', description: 'Entry and rescue', sessionDate: '2026-09-01', location: 'Site A', instructorName: 'J. Smith', status: 'Scheduled' },
    ]);
    httpMock.expectOne('/api/v1/training-sessions/1/participants').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Confined space training');
  });
});
