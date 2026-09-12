import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AccessScopePageComponent } from './access-scope-page.component';

describe('AccessScopePageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccessScopePageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the access scopes heading', () => {
    const fixture = TestBed.createComponent(AccessScopePageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/scopes').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/temporary-grants').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Access scopes');
  });

  it('lists access scopes', () => {
    const fixture = TestBed.createComponent(AccessScopePageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/scopes').forEach(r => r.flush([
      { id: '1', memberName: 'j.doe', scopeType: 'Site', siteName: 'Plant A', companyName: 'Acme', validFrom: '2026-01-01', validUntil: null, status: 'Active' },
    ]));
    httpMock.match('/api/v1/identities/temporary-grants').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('j.doe');
    expect(fixture.nativeElement.textContent).toContain('Plant A');
    expect(fixture.nativeElement.textContent).toContain('Active');
  });

  it('lists temporary grants', () => {
    const fixture = TestBed.createComponent(AccessScopePageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/scopes').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/temporary-grants').forEach(r => r.flush([
      { id: '1', memberName: 'j.doe', roleName: 'Site Manager', validFrom: '2026-09-01', validUntil: '2026-09-30', reason: 'Cover for leave', status: 'Active' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Site Manager');
    expect(fixture.nativeElement.textContent).toContain('Cover for leave');
  });

  it('shows empty state when no scopes or grants are loaded', () => {
    const fixture = TestBed.createComponent(AccessScopePageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/scopes').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/temporary-grants').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No scopes loaded.');
    expect(fixture.nativeElement.textContent).toContain('No temporary grants loaded.');
  });
});