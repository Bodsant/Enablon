import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { RbacAdminPageComponent } from './rbac-admin-page.component';

describe('RbacAdminPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RbacAdminPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the RBAC administration heading', () => {
    const fixture = TestBed.createComponent(RbacAdminPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/roles').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/permissions').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('RBAC administration');
  });

  it('lists roles', () => {
    const fixture = TestBed.createComponent(RbacAdminPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/roles').forEach(r => r.flush([
      { id: '1', code: 'HSE_MGR', name: 'HSE Manager', scopeType: 'Site', isSystem: true },
    ]));
    httpMock.match('/api/v1/identities/permissions').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('HSE_MGR');
    expect(fixture.nativeElement.textContent).toContain('HSE Manager');
    expect(fixture.nativeElement.textContent).toContain('Site');
  });

  it('lists permissions', () => {
    const fixture = TestBed.createComponent(RbacAdminPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/roles').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/permissions').forEach(r => r.flush([
      { id: '1', code: 'RECORD_APPROVE', module: 'records', action: 'approve', description: 'Approve records' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('RECORD_APPROVE');
    expect(fixture.nativeElement.textContent).toContain('records');
    expect(fixture.nativeElement.textContent).toContain('approve');
  });

  it('shows empty state when no roles or permissions are loaded', () => {
    const fixture = TestBed.createComponent(RbacAdminPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/identities/roles').forEach(r => r.flush([]));
    httpMock.match('/api/v1/identities/permissions').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No roles loaded.');
    expect(fixture.nativeElement.textContent).toContain('No permissions loaded.');
  });
});