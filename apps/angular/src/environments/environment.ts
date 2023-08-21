import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl: baseUrl,
    name: 'Tasky',
    logoUrl: '',
  },
  //pointing to Auth-Server
  oAuthConfig: {
    issuer: 'https://localhost:7600',
    redirectUri: baseUrl,
    clientId: 'Tasky_App',
    responseType: 'code',
    scope: 'IdentityService AdministrationService SaaSService',
    requireHttps: true,
  },
  //pointing to API Gateway
  apis: {
    default: {
      url: 'https://localhost:7500',
      rootNamespace: 'Tasky',
    },
  },
} as Environment;
