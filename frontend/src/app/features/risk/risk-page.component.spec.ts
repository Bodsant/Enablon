import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { RiskPageComponent } from './risk-page.component';

describe('RiskPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RiskPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the risk management heading', () => {
    const fixture = TestBed.createComponent(RiskPageComponent);
    fixture.detectChanges();

    httpMock.expectOne('/api/v1/risk/hazards').flush([]);
    httpMock.expectOne('/api/v1/risk/registers').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Risk management');
  });

  it('lists loaded hazards', () => {
    const fixture = TestBed.createComponent(RiskPageComponent);
    fixture.detectChanges();

    httpMock.expectOne('/api/v1/risk/hazards').flush([
      { id: 'h1', code: 'HAZ-001', name: 'Confined space entry', description: 'Oxygen deficiency' },
    ]);
    httpMock.expectOne('/api/v1/risk/registers').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('HAZ-001');
    expect(fixture.nativeElement.textContent).toContain('Confined space entry');
  });
});
