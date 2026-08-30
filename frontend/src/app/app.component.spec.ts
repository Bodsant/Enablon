import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AppComponent } from './app.component';
import { routes } from './app.routes';

describe('application routing and shell', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter(routes)],
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

  it('redirects the root route to login when unauthenticated', async () => {
    const { router } = await renderAt('/');
    expect(router.url).toBe('/login');
  });

  it('redirects a protected route to login when unauthenticated', async () => {
    const { router } = await renderAt('/dashboard');
    expect(router.url).toBe('/login');
  });

  it('renders the login page at /login', async () => {
    const { fixture, router } = await renderAt('/login');
    expect(router.url).toBe('/login');
    expect(fixture.nativeElement.textContent).toContain('ENABLON EHSMS');
    expect(fixture.nativeElement.textContent).toContain('Sign in');
  });
});