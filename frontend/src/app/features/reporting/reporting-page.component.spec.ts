import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ReportingPageComponent } from './reporting-page.component';

describe('ReportingPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReportingPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the reporting and KPI management heading', () => {
    const fixture = TestBed.createComponent(ReportingPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/kpis').flush([]);
    httpMock.expectOne('/api/v1/kpis/versions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Reporting');
  });

  it('lists loaded KPIs', () => {
    const fixture = TestBed.createComponent(ReportingPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/kpis').flush([
      { id: 'k1', name: 'Lost Time Injury Rate', code: 'LTIR', description: 'Rate per million hours', formula: 'LTI*1e6/hours', unit: 'ratio', status: 'Active' },
    ]);
    httpMock.expectOne('/api/v1/kpis/versions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Lost Time Injury Rate');
    expect(fixture.nativeElement.textContent).toContain('LTIR');
  });
});
