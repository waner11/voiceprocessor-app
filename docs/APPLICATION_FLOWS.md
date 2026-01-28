# VoiceProcessor Application Flows

This document provides comprehensive diagrams for frontend development, covering user journeys, page structure, and feature flows.

## Table of Contents
- [Site Map](#site-map)
- [User Flows](#user-flows)
- [Authentication Flows](#authentication-flows)
- [Core Feature Flows](#core-feature-flows)
- [Account Management Flows](#account-management-flows)
- [API Reference Summary](#api-reference-summary)

---

## Site Map

```mermaid
graph TB
    subgraph Public["Public Pages (No Auth Required)"]
        HOME["/\nHome/Landing"]
        LOGIN["/login\nLogin Page"]
        REGISTER["/register\nRegister Page"]
        VOICES["/voices\nVoice Catalog"]
        PRICING["/pricing\nPricing/Plans"]
    end

    subgraph Protected["Protected Pages (Auth Required)"]
        DASH["/dashboard\nDashboard"]
        GEN["/generate\nTTS Generator"]
        HIST["/history\nGeneration History"]
        SETTINGS["/settings\nAccount Settings"]
    end

    subgraph Settings["Settings Subpages"]
        PROFILE["/settings/profile\nProfile"]
        APIKEYS["/settings/api-keys\nAPI Keys"]
        OAUTH["/settings/connections\nOAuth Connections"]
        BILLING["/settings/billing\nBilling & Usage"]
    end

    HOME --> LOGIN
    HOME --> REGISTER
    HOME --> VOICES
    HOME --> PRICING
    LOGIN --> DASH
    REGISTER --> DASH

    DASH --> GEN
    DASH --> HIST
    DASH --> SETTINGS

    SETTINGS --> PROFILE
    SETTINGS --> APIKEYS
    SETTINGS --> OAUTH
    SETTINGS --> BILLING
```

### Page Descriptions

| Page | Route | Auth | Description |
|------|-------|------|-------------|
| Home | `/` | Public | Landing page with product overview, CTA to register/login |
| Login | `/login` | Public | Email/password + OAuth login buttons |
| Register | `/register` | Public | Email/password + OAuth registration |
| Voice Catalog | `/voices` | Public | Browse all available voices with filters |
| Pricing | `/pricing` | Public | Subscription tiers and pricing |
| Dashboard | `/dashboard` | Required | User overview: recent generations, usage stats, quick actions |
| TTS Generator | `/generate` | Required | Main TTS generation interface |
| History | `/history` | Required | Past generations with playback, download, feedback |
| Settings | `/settings/*` | Required | Account management pages |

---

## User Flows

### New User Registration Flow

```mermaid
flowchart TD
    START((User visits site)) --> HOME[Home Page]
    HOME --> |"Clicks 'Get Started'"| REG[Register Page]

    REG --> CHOICE{Registration Method}

    CHOICE --> |Email/Password| EMAIL[Fill email, name, password]
    EMAIL --> SUBMIT[Submit form]
    SUBMIT --> |"POST /api/v1/auth/register"| VALIDATE{Valid?}
    VALIDATE --> |Yes| TOKENS[Receive JWT tokens]
    VALIDATE --> |No| ERROR[Show validation errors]
    ERROR --> EMAIL

    CHOICE --> |Google| GOOGLE[Click Google button]
    GOOGLE --> |"GET /api/v1/auth/oauth/google/url"| GAUTH[Redirect to Google]
    GAUTH --> GCONSENT[User consents]
    GCONSENT --> GCALLBACK[Callback with code]
    GCALLBACK --> |"POST /api/v1/auth/oauth/google/callback"| TOKENS

    CHOICE --> |GitHub| GITHUB[Click GitHub button]
    GITHUB --> |"GET /api/v1/auth/oauth/github/url"| GHAUTH[Redirect to GitHub]
    GHAUTH --> GHCONSENT[User consents]
    GHCONSENT --> GHCALLBACK[Callback with code]
    GHCALLBACK --> |"POST /api/v1/auth/oauth/github/callback"| TOKENS

    TOKENS --> STORE[Store tokens in localStorage/cookies]
    STORE --> DASH[Redirect to Dashboard]
    DASH --> WELCOME[Show welcome/onboarding]
```

### Returning User Login Flow

```mermaid
flowchart TD
    START((User visits site)) --> CHECK{Has valid token?}

    CHECK --> |Yes| DASH[Dashboard]
    CHECK --> |No| LOGIN[Login Page]

    LOGIN --> METHOD{Login Method}

    METHOD --> |Email/Password| CREDS[Enter credentials]
    CREDS --> |"POST /api/v1/auth/login"| AUTH{Authenticated?}
    AUTH --> |Yes| TOKENS[Receive JWT tokens]
    AUTH --> |No| FAIL[Show error message]
    FAIL --> CREDS

    METHOD --> |Google/GitHub| OAUTH[OAuth Flow]
    OAUTH --> TOKENS

    TOKENS --> STORE[Store tokens]
    STORE --> DASH
```

### Token Refresh Flow (Background)

```mermaid
sequenceDiagram
    participant App as Frontend App
    participant Store as Token Storage
    participant API as Backend API

    Note over App: On app load or before API call

    App->>Store: Check access token expiry

    alt Token valid (>5 min remaining)
        Store-->>App: Use existing token
    else Token expiring soon or expired
        App->>Store: Get refresh token
        Store-->>App: Refresh token
        App->>API: POST /api/v1/auth/refresh

        alt Refresh successful
            API-->>App: New access + refresh tokens
            App->>Store: Update stored tokens
        else Refresh failed (expired/revoked)
            API-->>App: 401 Unauthorized
            App->>Store: Clear all tokens
            App->>App: Redirect to /login
        end
    end
```

---

## Authentication Flows

### Email/Password Authentication

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant API as Backend API
    participant DB as Database

    Note over U,DB: Registration
    U->>FE: Fill registration form
    FE->>API: POST /api/v1/auth/register
    API->>DB: Check email exists
    DB-->>API: Not found
    API->>DB: Create user with hashed password
    API->>DB: Create refresh token
    API-->>FE: {accessToken, refreshToken, user}
    FE->>FE: Store tokens
    FE-->>U: Redirect to dashboard

    Note over U,DB: Login
    U->>FE: Enter credentials
    FE->>API: POST /api/v1/auth/login
    API->>DB: Find user by email
    API->>API: Verify password hash
    API->>DB: Create refresh token
    API-->>FE: {accessToken, refreshToken, user}
    FE->>FE: Store tokens
    FE-->>U: Redirect to dashboard
```

### OAuth Authentication (Google/GitHub)

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant API as Backend API
    participant OAuth as OAuth Provider
    participant DB as Database

    U->>FE: Click "Login with Google"
    FE->>API: GET /api/v1/auth/oauth/google/url?redirectUri=...
    API-->>FE: {authorizationUrl, state}
    FE->>FE: Store state in sessionStorage
    FE->>OAuth: Redirect to authorizationUrl

    U->>OAuth: Login & consent
    OAuth->>FE: Redirect to callback with code

    FE->>FE: Verify state matches
    FE->>API: POST /api/v1/auth/oauth/google/callback
    Note right of API: {code, redirectUri}

    API->>OAuth: Exchange code for tokens
    OAuth-->>API: {access_token}
    API->>OAuth: GET /userinfo
    OAuth-->>API: {sub, email, name}

    API->>DB: Find ExternalLogin by (google, sub)

    alt ExternalLogin exists
        DB-->>API: Found → get user
    else ExternalLogin not found
        API->>DB: Find user by email
        alt User exists (auto-link)
            DB-->>API: Found user
            API->>DB: Create ExternalLogin link
        else No user exists
            API->>DB: Create new user
            API->>DB: Create ExternalLogin
        end
    end

    API->>DB: Create refresh token
    API-->>FE: {accessToken, refreshToken, user, isNewUser}
    FE->>FE: Store tokens
    FE-->>U: Redirect to dashboard
```

### API Key Authentication

```mermaid
sequenceDiagram
    participant Client as API Client
    participant API as Backend API
    participant DB as Database

    Note over Client,DB: Creating API Key (via Dashboard)
    Client->>API: POST /api/v1/auth/api-keys
    Note right of Client: Header: Authorization: Bearer {jwt}
    Note right of Client: Body: {name, expiresAt?}

    API->>API: Generate secure random key
    API->>API: Hash key (store only hash)
    API->>DB: Save ApiKey record
    API-->>Client: {id, name, key, prefix, expiresAt}
    Note over Client: key shown ONCE - user must save it

    Note over Client,DB: Using API Key
    Client->>API: POST /api/v1/generations
    Note right of Client: Header: X-API-Key: vp_abc123...

    API->>API: Extract prefix from key
    API->>DB: Find ApiKey by prefix
    DB-->>API: ApiKey record
    API->>API: Verify hash matches
    API->>API: Check not expired/revoked
    API-->>Client: Process request as user
```

---

## Core Feature Flows

### TTS Generation Flow

```mermaid
flowchart TD
    subgraph Frontend["Frontend - /generate"]
        INPUT[Text Input Area]
        VOICE[Voice Selector]
        OPTS[Options Panel]
        EST[Cost Estimate]
        GEN[Generate Button]
        PROG[Progress Indicator]
        RESULT[Audio Player]
    end

    subgraph API["Backend API"]
        ESTIMATE["/generations/estimate"]
        CREATE["/generations"]
        STATUS["/generations/{id}"]
        FEEDBACK["/generations/{id}/feedback"]
    end

    INPUT --> |Text changes| ESTIMATE
    VOICE --> |Voice selected| ESTIMATE
    ESTIMATE --> EST

    GEN --> |Click| CREATE
    CREATE --> |202 Accepted| PROG
    PROG --> |Poll| STATUS
    STATUS --> |Pending/Processing| PROG
    STATUS --> |Completed| RESULT

    RESULT --> |Rate quality| FEEDBACK
```

### TTS Generation Sequence

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant API as Backend API
    participant TTS as TTS Provider
    participant Store as File Storage

    U->>FE: Enter text, select voice
    FE->>API: POST /generations/estimate
    API-->>FE: {estimatedCost, chunks, duration}
    FE-->>U: Show cost estimate

    U->>FE: Click "Generate"
    FE->>API: POST /generations
    Note right of FE: {text, voiceId, routingPreference}
    API-->>FE: {id, status: "Pending"}

    loop Poll for status
        FE->>API: GET /generations/{id}

        alt Status: Pending
            API-->>FE: {status: "Pending"}
        else Status: Processing
            API-->>FE: {status: "Processing", progress: 45%}
        else Status: Completed
            API-->>FE: {status: "Completed", audioUrl}
        else Status: Failed
            API-->>FE: {status: "Failed", error}
        end
    end

    Note over API,Store: Background Processing
    API->>API: Chunk text
    API->>TTS: Generate audio chunks
    TTS-->>API: Audio data
    API->>API: Merge chunks
    API->>Store: Upload final audio
    Store-->>API: Audio URL

    FE-->>U: Play audio
    U->>FE: Rate generation
    FE->>API: POST /generations/{id}/feedback
```

### Voice Selection Flow

```mermaid
flowchart TD
    START[Voice Selector Component] --> LOAD[Load voices]
    LOAD --> |"GET /api/v1/voices"| LIST[Display voice list]

    LIST --> FILTER{Apply filters?}
    FILTER --> |Yes| APPLY[Filter by provider/language/gender]
    APPLY --> |"GET /api/v1/voices?provider=...&language=..."| LIST
    FILTER --> |No| SELECT

    LIST --> SELECT[User selects voice]
    SELECT --> PREVIEW{Preview available?}
    PREVIEW --> |Yes| PLAY[Play preview audio]
    PREVIEW --> |No| CONFIRM
    PLAY --> CONFIRM[Confirm selection]
    CONFIRM --> EMIT[Emit selected voice to parent]
```

### Generation History Flow

```mermaid
flowchart TD
    START["/history page"] --> LOAD[Load generations]
    LOAD --> |"GET /api/v1/generations?page=1"| LIST[Display list]

    LIST --> ACTION{User action}

    ACTION --> |View details| DETAIL[Show generation details]
    DETAIL --> |"GET /api/v1/generations/{id}"| MODAL[Detail modal]

    ACTION --> |Play audio| PLAY[Audio player]

    ACTION --> |Download| DL[Download audio file]

    ACTION --> |Filter by status| FILTER
    FILTER --> |"GET /api/v1/generations?status=..."| LIST

    ACTION --> |Load more| PAGE
    PAGE --> |"GET /api/v1/generations?page=N"| LIST

    ACTION --> |Submit feedback| FB[Feedback modal]
    FB --> |"POST /api/v1/generations/{id}/feedback"| LIST
```

---

## Account Management Flows

### Profile Settings Flow

```mermaid
flowchart TD
    START["/settings/profile"] --> LOAD[Load user info]
    LOAD --> |"GET /api/v1/auth/me"| DISPLAY[Display profile form]

    DISPLAY --> EDIT[Edit name/email]
    EDIT --> SAVE[Save changes]
    SAVE --> |"PUT /api/v1/users/me"| SUCCESS[Show success]

    DISPLAY --> PWD[Change password section]
    PWD --> |Has password| CHANGE[Change password form]
    PWD --> |OAuth only, no password| SET[Set password form]
    CHANGE --> |"PUT /api/v1/auth/password"| SUCCESS
    SET --> |"POST /api/v1/auth/password"| SUCCESS
```

### API Key Management Flow

```mermaid
flowchart TD
    START["/settings/api-keys"] --> LOAD[Load API keys]
    LOAD --> |"GET /api/v1/auth/api-keys"| LIST[Display key list]

    LIST --> CREATE[Create new key]
    CREATE --> FORM[Key name + expiry form]
    FORM --> |"POST /api/v1/auth/api-keys"| SHOW[Show key ONCE]
    SHOW --> COPY[User copies key]
    COPY --> LIST

    LIST --> REVOKE[Revoke key]
    REVOKE --> CONFIRM[Confirm dialog]
    CONFIRM --> |"DELETE /api/v1/auth/api-keys/{id}"| LIST
```

### OAuth Connections Management Flow

```mermaid
flowchart TD
    START["/settings/connections"] --> LOAD[Load linked providers]
    LOAD --> |"GET /api/v1/auth/oauth/providers"| DISPLAY[Display connections]

    DISPLAY --> LINK[Link new provider]
    LINK --> SELECT[Select Google/GitHub]
    SELECT --> |"GET /api/v1/auth/oauth/{provider}/url"| REDIRECT[OAuth flow]
    REDIRECT --> CALLBACK[Callback with code]
    CALLBACK --> |"POST /api/v1/auth/oauth/{provider}/link"| DISPLAY

    DISPLAY --> UNLINK[Unlink provider]
    UNLINK --> CHECK{Is only login method?}
    CHECK --> |Yes| WARN[Show warning: set password first]
    CHECK --> |No| CONFIRM[Confirm dialog]
    CONFIRM --> |"DELETE /api/v1/auth/oauth/{provider}"| DISPLAY
```

### Complete Settings Navigation

```mermaid
flowchart LR
    subgraph Settings["/settings"]
        NAV[Settings Nav]
        PROFILE[Profile]
        APIKEYS[API Keys]
        OAUTH[Connections]
        BILLING[Billing]
    end

    NAV --> PROFILE
    NAV --> APIKEYS
    NAV --> OAUTH
    NAV --> BILLING

    PROFILE --> |"GET /auth/me"| P_DATA[Name, Email, Password status]
    APIKEYS --> |"GET /auth/api-keys"| K_DATA[Key list with metadata]
    OAUTH --> |"GET /auth/oauth/providers"| O_DATA[Linked: Google, GitHub]
    BILLING --> |"GET /users/me/usage"| B_DATA[Credits, Usage stats]
```

---

## API Reference Summary

### Authentication Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/register` | Public | Register with email/password |
| POST | `/api/v1/auth/login` | Public | Login with email/password |
| POST | `/api/v1/auth/refresh` | Public | Refresh access token |
| POST | `/api/v1/auth/logout` | Bearer | Revoke refresh token |
| GET | `/api/v1/auth/me` | Bearer | Get current user info |
| GET | `/api/v1/auth/oauth/{provider}/url` | Public | Get OAuth authorization URL |
| POST | `/api/v1/auth/oauth/{provider}/callback` | Public | Exchange OAuth code for tokens |
| GET | `/api/v1/auth/oauth/providers` | Bearer | List linked OAuth providers |
| POST | `/api/v1/auth/oauth/{provider}/link` | Bearer | Link OAuth provider to account |
| DELETE | `/api/v1/auth/oauth/{provider}` | Bearer | Unlink OAuth provider |
| POST | `/api/v1/auth/api-keys` | Bearer | Create new API key |
| GET | `/api/v1/auth/api-keys` | Bearer | List user's API keys |
| DELETE | `/api/v1/auth/api-keys/{id}` | Bearer | Revoke API key |

### Voice Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/voices` | Public | List voices with filters |
| GET | `/api/v1/voices/{id}` | Public | Get voice details |
| GET | `/api/v1/voices/by-provider` | Public | Get voices grouped by provider |
| POST | `/api/v1/voices/refresh` | Bearer | Refresh voice catalog |

### Generation Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/generations/estimate` | Public | Estimate generation cost |
| POST | `/api/v1/generations` | Bearer/API-Key | Start TTS generation |
| GET | `/api/v1/generations` | Bearer/API-Key | List user's generations |
| GET | `/api/v1/generations/{id}` | Bearer/API-Key | Get generation status |
| POST | `/api/v1/generations/{id}/feedback` | Bearer | Submit feedback |
| DELETE | `/api/v1/generations/{id}` | Bearer/API-Key | Cancel generation |

### Authentication Methods

```
# JWT Bearer Token
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

# API Key
X-API-Key: vp_abc123def456...
```

---

## State Management Recommendations

### Auth State
```typescript
interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  accessToken: string | null;
  accessTokenExpiresAt: Date | null;
  refreshToken: string | null;
}
```

### Token Storage Strategy
- **Access Token**: Memory (React state/context) - short-lived, refreshed often
- **Refresh Token**: HttpOnly cookie (preferred) or localStorage - longer-lived
- **Token Refresh**: Automatic refresh when access token expires in <5 minutes

### Route Protection
```typescript
// Protected route wrapper
const ProtectedRoute = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return <Navigate to="/login" />;

  return children;
};
```

---

## Error Handling

### API Error Response Format
```json
{
  "code": "ERROR_CODE",
  "message": "Human readable message"
}
```

### Common Error Codes
| Code | HTTP Status | Handling |
|------|-------------|----------|
| `UNAUTHORIZED` | 401 | Redirect to login |
| `FORBIDDEN` | 403 | Show permission error |
| `NOT_FOUND` | 404 | Show not found page |
| `VALIDATION_ERROR` | 400 | Show field errors |
| `INSUFFICIENT_CREDITS` | 402 | Show upgrade prompt |
| `RATE_LIMITED` | 429 | Show retry message |

---

## UI Component Hierarchy

```
App
├── PublicLayout
│   ├── Navbar (Login/Register buttons)
│   ├── HomePage
│   ├── LoginPage
│   ├── RegisterPage
│   ├── VoicesPage
│   └── PricingPage
│
└── ProtectedLayout
    ├── Navbar (User menu, logout)
    ├── Sidebar (Navigation)
    ├── DashboardPage
    │   ├── UsageStats
    │   ├── RecentGenerations
    │   └── QuickActions
    ├── GeneratePage
    │   ├── TextInput
    │   ├── VoiceSelector
    │   ├── OptionsPanel
    │   ├── CostEstimate
    │   └── AudioPlayer
    ├── HistoryPage
    │   ├── GenerationList
    │   ├── FilterBar
    │   └── Pagination
    └── SettingsPage
        ├── SettingsNav
        ├── ProfileTab
        ├── ApiKeysTab
        ├── ConnectionsTab
        └── BillingTab
```
