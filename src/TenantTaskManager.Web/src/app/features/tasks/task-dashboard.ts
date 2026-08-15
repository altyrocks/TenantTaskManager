import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { TaskItem, TaskService } from './task.service';

@Component({
  selector: 'app-task-dashboard',
  imports: [DatePipe],
  templateUrl: './task-dashboard.html',
  styleUrl: './task-dashboard.css'
})
export class TaskDashboard implements OnInit {
  tasks: TaskItem[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private readonly taskService: TaskService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.taskService.getTasks().subscribe({
      next: tasks => {
        this.tasks = tasks;
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage = error.status === 401
          ? 'Your session has expired. Please sign in again.'
          : 'Unable to load tasks.';
      }
    });
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
