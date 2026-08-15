import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  it('stores the access token after login', () => {
    service.login('user@example.com', 'Password123!').subscribe();

    const request = httpTesting.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'user@example.com',
      password: 'Password123!'
    });

    request.flush({
      accessToken: 'test-token',
      expiresAtUtc: '2026-08-16T00:00:00Z'
    });

    expect(service.getAccessToken()).toBe('test-token');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('clears the access token on logout', () => {
    sessionStorage.setItem('tenant-task-manager-token', 'test-token');

    service.logout();

    expect(service.getAccessToken()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });
});
