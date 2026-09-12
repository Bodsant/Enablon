import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { RetentionPageComponent } from './retention-page.component';

describe('RetentionPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RetentionPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the retention policies heading', () => {
    const fixture = TestBed.createComponent(RetentionPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/retention-policies').forEach(r => r.flush([]));
    httpMock.match('/api/v1/retention-policies/purge-candidates').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Retention policies');
  });

  it('lists retention policies', () => {
    const fixture = TestBed.createComponent(RetentionPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/retention-policies').forEach(r => r.flush([
      { id: '1', name: 'Incident records', moduleCode: 'incident', recordType: 'IncidentRecord', retentionDays: 3650, status: 'Active' },
    ]));
    httpMock.match('/api/v1/retention-policies/purge-candidates').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Incident records');
    expect(fixture.nativeElement.textContent).toContain('3650 days');
  });

  it('lists purge candidates', () => {
    const fixture = TestBed.createComponent(RetentionPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/retention-policies').forEach(r => r.flush([]));
    httpMock.match('/api/v1/retention-policies/purge-candidates').forEach(r => r.flush([
      { id: '1', moduleCode: 'ptw', recordType: 'WorkRequest', title: 'WR-0001', scheduledPurgeDate: '2026-10-01' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('WR-0001');
    expect(fixture.nativeElement.textContent).toContain('2026-10-01');
  });

  it('shows empty state when no policies or candidates are loaded', () => {
    const fixture = TestBed.createComponent(RetentionPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/retention-policies').forEach(r => r.flush([]));
    httpMock.match('/api/v1/retention-policies/purge-candidates').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No retention policies loaded.');
    expect(fixture.nativeElement.textContent).toContain('No purge candidates loaded.');
  });
});