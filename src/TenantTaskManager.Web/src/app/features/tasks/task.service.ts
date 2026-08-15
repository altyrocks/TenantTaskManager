import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface TaskItem {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

interface CreateTaskResponse {
  id: string;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  constructor(private readonly http: HttpClient) {}

  getTasks() {
    return this.http.get<TaskItem[]>('/api/tasks');
  }

  createTask(title: string) {
    return this.http.post<CreateTaskResponse>('/api/tasks', { title });
  }

  completeTask(id: string) {
    return this.http.patch<void>(`/api/tasks/${id}/complete`, null);
  }

  updateTask(id: string, title: string) {
    return this.http.put<void>(`/api/tasks/${id}`, { title });
  }
}
