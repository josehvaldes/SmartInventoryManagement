import { config } from './appConfig';

export const apiScopes = [
  `api://${config.azure.apiclientid}/provisioning.write`
];