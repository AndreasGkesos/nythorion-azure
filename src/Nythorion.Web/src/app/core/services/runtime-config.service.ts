import { Injectable } from '@angular/core';

interface RuntimeConfig {
  apiUrl: string;
  authUrl: string;
}

const CONFIG_PATH = '/config.json';

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  readonly clientId = 'nythorion-spa';
  readonly scope = 'openid profile api';

  private config: RuntimeConfig | null = null;

  async load(): Promise<void> {
    const response = await fetch(CONFIG_PATH);
    if (!response.ok) {
      throw new Error(`Could not load ${CONFIG_PATH}: ${response.status}`);
    }
    this.config = (await response.json()) as RuntimeConfig;
  }

  get apiUrl(): string {
    return this.loaded.apiUrl;
  }

  get authUrl(): string {
    return this.loaded.authUrl;
  }

  private get loaded(): RuntimeConfig {
    if (!this.config) {
      throw new Error('Runtime config accessed before it was loaded.');
    }
    return this.config;
  }
}
