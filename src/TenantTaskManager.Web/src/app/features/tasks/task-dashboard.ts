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
  taskActionErrorMessage = '';
  editingTaskId: string | null = null;
  isSavingEdit = false;
  readonly completingTaskIds = new Set<string>();

  readonly createForm;
  readonly editForm;

  constructor(
    formBuilder: FormBuilder,
    private readonly taskService: TaskService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.createForm = formBuilder.nonNullable.group({
      title: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]]
    });
    this.editForm = formBuilder.nonNullable.group({
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

  completeTask(task: TaskItem): void {
    if (task.isCompleted || this.completingTaskIds.has(task.id)) {
      return;
    }

    this.completingTaskIds.add(task.id);
    this.taskActionErrorMessage = '';

    this.taskService.completeTask(task.id).subscribe({
      next: () => {
        this.completingTaskIds.delete(task.id);
        this.loadTasks();
      },
      error: () => {
        this.completingTaskIds.delete(task.id);
        this.taskActionErrorMessage = `Unable to complete "${task.title}".`;
      }
    });
  }

  startEdit(task: TaskItem): void {
    this.editingTaskId = task.id;
    this.taskActionErrorMessage = '';
    this.editForm.reset({ title: task.title });
  }

  cancelEdit(): void {
    if (this.isSavingEdit) {
      return;
    }

    this.editingTaskId = null;
    this.editForm.reset();
  }

  saveEdit(task: TaskItem): void {
    const title = this.editForm.controls.title.value.trim();

    if (!title || this.editForm.invalid || this.isSavingEdit) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.isSavingEdit = true;
    this.taskActionErrorMessage = '';

    this.taskService.updateTask(task.id, title).subscribe({
      next: () => {
        this.isSavingEdit = false;
        this.editingTaskId = null;
        this.editForm.reset();
        this.loadTasks();
      },
      error: () => {
        this.isSavingEdit = false;
        this.taskActionErrorMessage = `Unable to update "${task.title}".`;
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
