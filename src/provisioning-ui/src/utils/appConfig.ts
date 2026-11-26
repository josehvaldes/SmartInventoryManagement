
export interface AppConfig {
  api: {
    baseUrl: string;
    version: string;
  };
  azure: {
    clientId: string;
    tenantId: string;
    apiclientid: string;
  }
  redirectUri: string;
  postLogoutRedirectUri: string;
}

let appConfigPromise: Promise<AppConfig> | undefined;

export async function loadAppConfig(): Promise<AppConfig> {
    appConfigPromise ??= fetch('/config/app-config.json')
    .then((response) => {
      if (!response.ok) {
        throw new Error('Unable to load application configuration');
      }
      return response.json() as Promise<AppConfig>;
    });

  return appConfigPromise;
}

const config = await loadAppConfig();
export { config };