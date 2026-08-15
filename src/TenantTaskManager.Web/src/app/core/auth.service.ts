import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';

interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'tenant-task-manager-token';

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string) {
    return this.http
      .post<LoginResponse>('/api/auth/login', { email, password })
      .pipe(tap(response => sessionStorage.setItem(this.tokenKey, response.accessToken)));
  }
}
