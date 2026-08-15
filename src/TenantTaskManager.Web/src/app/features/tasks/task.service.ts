import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface TaskItem {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(private readonly http: HttpClient) {}

  getTasks() {
    return this.http.get<TaskItem[]>('/api/tasks');
  }
}
