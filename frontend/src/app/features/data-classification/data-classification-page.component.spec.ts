import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { DataClassificationPageComponent } from './data-classification-page.component';

describe('DataClassificationPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataClassificationPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the data classification heading', () => {
    const fixture = TestBed.createComponent(DataClassificationPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/data-classifications').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Data classification');
  });

  it('lists classifications', () => {
    const fixture = TestBed.createComponent(DataClassificationPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/data-classifications').forEach(r => r.flush([
      { id: '1', code: 'C2', name: 'Internal', rank: 2, isRestricted: true, description: 'Internal use only' },
    ]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Internal');
    expect(fixture.nativeElement.textContent).toContain('restricted');
  });

  it('shows empty state when no classifications are loaded', () => {
    const fixture = TestBed.createComponent(DataClassificationPageComponent);
    fixture.detectChanges();
    httpMock.match('/api/v1/data-classifications').forEach(r => r.flush([]));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No classifications loaded.');
  });
});