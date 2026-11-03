import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { RuntimeConfigService } from './runtime-config.service';

export interface AppConfig {
  llmModel: string;
  embedModel: string;
  temperature: number;
  numPredict: number;
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly http = inject(HttpClient);
  private readonly runtimeConfig = inject(RuntimeConfigService);
  readonly config = signal<AppConfig | null>(null);

  load(): void {
    this.http.get<AppConfig>(`${this.runtimeConfig.apiUrl}/config`).subscribe({
      next: c => this.config.set(c),
      error: () => {}
    });
  }
}
