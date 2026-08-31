import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { LegalPageComponent } from './legal-page.component';

describe('LegalPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LegalPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the legal management heading', () => {
    const fixture = TestBed.createComponent(LegalPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/legal/sources').flush([]);
    httpMock.expectOne('/api/v1/legal/obligations').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Legal management');
  });

  it('lists loaded legal sources', () => {
    const fixture = TestBed.createComponent(LegalPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/legal/sources').flush([
      { id: 's1', title: 'Occupational Safety Act', jurisdiction: 'Vietnam', sourceType: 'Regulation', effectiveDate: '2025-01-01', status: 'Active' },
    ]);
    httpMock.expectOne('/api/v1/legal/obligations').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Occupational Safety Act');
  });
});
