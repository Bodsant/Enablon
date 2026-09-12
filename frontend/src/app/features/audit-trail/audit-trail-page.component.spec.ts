import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuditTrailPageComponent } from './audit-trail-page.component';

describe('AuditTrailPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuditTrailPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the audit trail heading', () => {
    const fixture = TestBed.createComponent(AuditTrailPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/audit-logs').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Audit trail');
  });

  it('lists audit log entries', () => {
    const fixture = TestBed.createComponent(AuditTrailPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/audit-logs').forEach(r => r.flush([
      { id: '1', actionCode: 'RECORD_VIEW', recordTitle: 'Incident IR-001', moduleCode: 'incident', occurredAt: '2026-09-01T08:00:00Z', tenantMemberName: 'j.doe', ipAddress: '10.0.0.9' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('RECORD_VIEW');
    expect(fixture.nativeElement.textContent).toContain('Incident IR-001');
    expect(fixture.nativeElement.textContent).toContain('j.doe');
  });

  it('shows empty state when no audit events are loaded', () => {
    const fixture = TestBed.createComponent(AuditTrailPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/audit-logs').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No audit events loaded.');
  });
});