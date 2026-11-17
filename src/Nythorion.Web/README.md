# Nythorion.Web

The Angular frontend: standalone components, Angular Material and signals. It signs in against `Nythorion.Auth` with the authorization code flow and PKCE, and calls `Nythorion.Api` with the access token.

```bash
npm install
npm start          # http://localhost:4200
npm run build      # output in dist/Nythorion.Web/browser
```

## Runtime configuration

The backend addresses are not compiled in. The app loads `/config.json` at startup:

```json
{ "apiUrl": "https://localhost:7185", "authUrl": "https://localhost:7087" }
```

`public/config.json` holds the local values. For the Azure deployment the `deploy-web.yml` workflow overwrites the file in the build output with the addresses it reads from Azure, so the same build works against any backend.

`public/staticwebapp.config.json` sends unknown paths to `index.html`, which the `/callback` route of the login flow needs on Azure Static Web Apps.

The deployment is described in the [repository README](../../README.md).
