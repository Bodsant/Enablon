import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { IntegrationPageComponent } from './integration-page.component';

describe('IntegrationPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IntegrationPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the integration management heading', () => {
    const fixture = TestBed.createComponent(IntegrationPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/integration/interfaces').flush([]);
    httpMock.expectOne('/api/v1/integration/runs').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Integration management');
  });

  it('lists loaded interfaces', () => {
    const fixture = TestBed.createComponent(IntegrationPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/integration/interfaces').flush([
      { id: 'i1', name: 'SAP HR Sync', interfaceType: 'Enterprise', protocol: 'REST', status: 'Active' },
    ]);
    httpMock.expectOne('/api/v1/integration/runs').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('SAP HR Sync');
  });
});
