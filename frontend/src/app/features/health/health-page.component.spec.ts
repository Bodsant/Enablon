import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { HealthPageComponent } from './health-page.component';

describe('HealthPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the health management heading', () => {
    const fixture = TestBed.createComponent(HealthPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/health/profiles').flush([]);
    httpMock.expectOne('/api/v1/health/fitness-statuses').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Health management');
  });

  it('lists loaded health profiles', () => {
    const fixture = TestBed.createComponent(HealthPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/health/profiles').flush([
      { id: 'p1', personName: 'Alice Chen', bloodType: 'O+', allergies: 'None', conditions: 'None', status: 'Fit' },
    ]);
    httpMock.expectOne('/api/v1/health/fitness-statuses').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Alice Chen');
    expect(fixture.nativeElement.textContent).toContain('O+');
  });
});
