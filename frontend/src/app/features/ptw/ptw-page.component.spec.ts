import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { PtwPageComponent } from './ptw-page.component';

describe('PtwPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PtwPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the permit to work heading', () => {
    const fixture = TestBed.createComponent(PtwPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/work-requests').flush([]);
    httpMock.expectOne('/api/v1/permits').flush([]);
    httpMock.expectOne('/api/v1/isolation-plans').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Permit to work');
  });

  it('lists work requests and isolation plans', () => {
    const fixture = TestBed.createComponent(PtwPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/work-requests').flush([
      { id: 'w1', recordNumber: 'WR-0001', workDescription: 'Replace gasket', workType: 'Maintenance' },
    ]);
    httpMock.expectOne('/api/v1/permits').flush([
      { id: 'p1', recordNumber: 'PTW-0001', validFrom: null, validUntil: null },
    ]);
    httpMock.expectOne('/api/v1/isolation-plans').flush([
      { id: 'i1', recordNumber: 'LOTO-0001', status: 'Active' },
    ]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('WR-0001');
    expect(fixture.nativeElement.textContent).toContain('Replace gasket');
    expect(fixture.nativeElement.textContent).toContain('LOTO-0001');
  });
});
