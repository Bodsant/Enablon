import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AssetEmergencyPageComponent } from './asset-emergency-page.component';

describe('AssetEmergencyPageComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetEmergencyPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('renders the asset and emergency management heading', () => {
    const fixture = TestBed.createComponent(AssetEmergencyPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/assets').flush([]);
    httpMock.expectOne('/api/v1/emergency/plans').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('emergency management');
  });

  it('lists loaded assets', () => {
    const fixture = TestBed.createComponent(AssetEmergencyPageComponent);
    fixture.detectChanges();
    httpMock.expectOne('/api/v1/assets').flush([
      { id: 'a1', name: 'Forklift FL-01', assetCode: 'AS-1001', assetType: 'Material handling', location: 'Warehouse', status: 'Operational' },
    ]);
    httpMock.expectOne('/api/v1/emergency/plans').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Forklift FL-01');
    expect(fixture.nativeElement.textContent).toContain('AS-1001');
  });
});
