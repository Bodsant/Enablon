import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AppComponent } from './app.component';
import { routes } from './app.routes';

describe('application routing and shell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter(routes)]
    }).compileComponents();
  });

  async function renderAt(url: string) {
    const router = TestBed.inject(Router);
    const fixture = TestBed.createComponent(AppComponent);
    await router.navigateByUrl(url);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, router };
  }

  it('redirects the root route to architecture', async () => {
    const { router } = await renderAt('/');
    expect(router.url).toBe('/architecture');
  });

  it('renders the architecture route inside the application shell', async () => {
    const { fixture, router } = await renderAt('/architecture');
    expect(router.url).toBe('/architecture');
    expect(fixture.nativeElement.querySelector('app-shell')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('ENABLON EHSMS');
    expect(fixture.nativeElement.textContent).toContain('No EHS business workflow');
  });

  it('redirects an unknown route to architecture', async () => {
    const { router } = await renderAt('/not-a-route');
    expect(router.url).toBe('/architecture');
  });
});
