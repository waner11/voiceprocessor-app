# Configuration Notes

## CORS AllowedOrigins

The `Cors:AllowedOrigins` setting can be overridden via environment variables for production deployment.

### Environment Variable Override

To add production origins (e.g., Vercel deployment), set environment variables:

```bash
Cors__AllowedOrigins__0=http://localhost:3000
Cors__AllowedOrigins__1=https://voiceprocessor.vercel.app
Cors__AllowedOrigins__2=https://voiceprocessor-staging.vercel.app
```

ASP.NET Core automatically reads these environment variables and merges them with `appsettings.json`.

### Railway Deployment

In Railway, set these as environment variables in the service settings:
- `Cors__AllowedOrigins__0` = `https://your-production-domain.vercel.app`
- Add additional origins as needed with incrementing indices

Note: Use double underscores (`__`) in environment variable names to represent JSON hierarchy.
