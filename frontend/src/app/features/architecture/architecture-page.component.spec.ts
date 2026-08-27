import { TestBed } from '@angular/core/testing';
import { ArchitecturePageComponent } from './architecture-page.component';
describe('ArchitecturePageComponent', () => { it('states the non-business scaffold boundary', async () => { await TestBed.configureTestingModule({imports:[ArchitecturePageComponent]}).compileComponents(); const fixture=TestBed.createComponent(ArchitecturePageComponent); fixture.detectChanges(); expect(fixture.nativeElement.textContent).toContain('No EHS business workflow'); }); });
