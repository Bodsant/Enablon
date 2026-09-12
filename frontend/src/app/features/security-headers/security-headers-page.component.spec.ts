import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { SecurityHeadersPageComponent } from './security-headers-page.component';

describe('SecurityHeadersPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SecurityHeadersPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the security headers heading', () => {
    const fixture = TestBed.createComponent(SecurityHeadersPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/architecture/info').forEach(r => r.flush({ application: 'ehsms-api', version: '1.0.0', environment: 'dev' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Security headers');
  });

  it('documents the applied security headers', () => {
    const fixture = TestBed.createComponent(SecurityHeadersPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/architecture/info').forEach(r => r.flush({ application: 'ehsms-api', version: '1.0.0', environment: 'dev' }));
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('nosniff');
    expect(text).toContain('DENY');
    expect(text).toContain('no-referrer');
    expect(text).toContain("default-src 'none'");
  });

  it('shows application info', () => {
    const fixture = TestBed.createComponent(SecurityHeadersPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/architecture/info').forEach(r => r.flush({ application: 'ehsms-api', version: '2.4.1', environment: 'staging' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('ehsms-api');
    expect(fixture.nativeElement.textContent).toContain('2.4.1');
    expect(fixture.nativeElement.textContent).toContain('staging');
  });

  it('contains the production readiness eyebrow', () => {
    const fixture = TestBed.createComponent(SecurityHeadersPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/architecture/info').forEach(r => r.flush({ application: 'ehsms-api', version: '1.0.0', environment: 'dev' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('PRODUCTION READINESS');
  });
});