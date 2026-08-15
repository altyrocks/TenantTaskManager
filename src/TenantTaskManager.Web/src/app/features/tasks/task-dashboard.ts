import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { TaskItem, TaskService } from './task.service';

@Component({
  selector: 'app-task-dashboard',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './task-dashboard.html',
  styleUrl: './task-dashboard.css'
})
export class TaskDashboard implements OnInit {
  tasks: TaskItem[] = [];
  isLoading = true;
  isCreating = false;
  errorMessage = '';
  createErrorMessage = '';

  readonly createForm;

  constructor(
    formBuilder: FormBuilder,
    private readonly taskService: TaskService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.createForm = formBuilder.nonNullable.group({
      title: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]]
    });
  }

  ngOnInit(): void {
    this.loadTasks();
  }

  createTask(): void {
    const title = this.createForm.controls.title.value.trim();

    if (!title || this.createForm.invalid || this.isCreating) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.isCreating = true;
    this.createErrorMessage = '';

    this.taskService.createTask(title).subscribe({
      next: () => {
        this.isCreating = false;
        this.createForm.reset();
        this.loadTasks();
      },
      error: () => {
        this.isCreating = false;
        this.createErrorMessage = 'Unable to create the task.';
      }
    });
  }

  private loadTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';

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
