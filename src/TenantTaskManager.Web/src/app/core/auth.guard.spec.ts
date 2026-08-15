import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from './auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  const authService = {
    isAuthenticated: vi.fn()
  };
  const loginUrlTree = {} as UrlTree;
  const router = {
    createUrlTree: vi.fn(() => loginUrlTree)
  };

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });
  });

  it('allows authenticated users to open protected routes', () => {
    authService.isAuthenticated.mockReturnValue(true);

    const result = runGuard();

    expect(result).toBe(true);
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('redirects unauthenticated users to login', () => {
    authService.isAuthenticated.mockReturnValue(false);

    const result = runGuard();

    expect(result).toBe(loginUrlTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() => authGuard(
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot));
  }
});
