import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { SessionMgmtPageComponent } from './session-mgmt-page.component';

describe('SessionMgmtPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionMgmtPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the active sessions heading', () => {
    const fixture = TestBed.createComponent(SessionMgmtPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/sessions').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Active sessions');
  });

  it('lists sessions', () => {
    const fixture = TestBed.createComponent(SessionMgmtPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/sessions').forEach(r => r.flush([
      { id: '1', userEmail: 'j.doe@acme.com', ipAddress: '10.0.0.9', userAgent: 'Chrome/120', createdAt: '2026-09-12T09:00:00Z', expiresAt: null, isCurrent: true },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('j.doe@acme.com');
    expect(fixture.nativeElement.textContent).toContain('10.0.0.9');
    expect(fixture.nativeElement.textContent).toContain('current');
  });

  it('shows empty state when no sessions are loaded', () => {
    const fixture = TestBed.createComponent(SessionMgmtPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/sessions').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No sessions loaded.');
  });
});