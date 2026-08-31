import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { InspectionPageComponent } from './inspection-page.component';

describe('InspectionPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InspectionPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the inspection & audit heading', () => {
    const fixture = TestBed.createComponent(InspectionPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/audits').flush([]);
    httpMock.expectOne('/api/v1/inspections').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Inspection & audit');
  });

  it('lists audits and shows compliance percentage', () => {
    const fixture = TestBed.createComponent(InspectionPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/audits').flush([
      { id: 'a1', recordNumber: 'AUD-001', auditType: 'Internal', scopeText: 'Chemical storage' },
    ]);
    httpMock.expectOne('/api/v1/inspections').flush([
      { id: 'i1', recordNumber: 'INSP-001', compliancePercentage: 92 },
    ]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('AUD-001');
    expect(fixture.nativeElement.textContent).toContain('Chemical storage');
    expect(fixture.nativeElement.textContent).toContain('92% compliant');
  });
});
