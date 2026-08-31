import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { IncidentPageComponent } from './incident-page.component';

describe('IncidentPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IncidentPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the incident management heading', () => {
    const fixture = TestBed.createComponent(IncidentPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/incidents').flush([]);
    httpMock.expectOne('/api/v1/capa/actions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Incident management');
  });

  it('lists loaded incidents and CAPA actions', () => {
    const fixture = TestBed.createComponent(IncidentPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/incidents').flush([
      { id: 'i1', recordNumber: 'INC-0001', description: 'Chemical splash', classificationStatus: 'NearMiss' },
    ]);
    httpMock.expectOne('/api/v1/capa/actions').flush([
      { id: 'a1', actionType: 'Corrective', description: 'PPE training', priority: 'High' },
    ]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('INC-0001');
    expect(fixture.nativeElement.textContent).toContain('PPE training');
  });
});
