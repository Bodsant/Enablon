import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AccessReviewPageComponent } from './access-review-page.component';

describe('AccessReviewPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccessReviewPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the access reviews heading', () => {
    const fixture = TestBed.createComponent(AccessReviewPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/access-reviews').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Access reviews');
  });

  it('lists access reviews', () => {
    const fixture = TestBed.createComponent(AccessReviewPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/access-reviews').forEach(r => r.flush([
      { id: '1', memberName: 'j.doe', roleName: 'HSE Manager', requestedAt: '2026-09-05', status: 'Pending', reviewedByName: 'a.admin' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('j.doe');
    expect(fixture.nativeElement.textContent).toContain('HSE Manager');
    expect(fixture.nativeElement.textContent).toContain('Pending');
  });

  it('shows empty state when no reviews are loaded', () => {
    const fixture = TestBed.createComponent(AccessReviewPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/access-reviews').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No access reviews loaded.');
  });
});