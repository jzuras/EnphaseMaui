# MAUI Blazor Hybrid App - Design Document

## Project Overview

This document outlines the design for a MAUI Blazor Hybrid application that will serve as the GUI follow-on to the TUI console application for monitoring Enphase solar panel systems.

## Architecture Overview

The solution will use Visual Studio 2022's MAUI Blazor Hybrid template, which creates:
- **MAUI Blazor Hybrid App**: Cross-platform GUI (Windows, Android, iOS)
- **Blazor Web App**: Handles Enphase cloud OAuth and token management
- **Shared Razor Class Library (RCL)**: Common UI components and business logic

## Token Management Strategy

### Problem
The TUI application hard-codes a JWT token that expires after 1 year. The GUI needs automated token acquisition and refresh through Enphase's cloud OAuth APIs.

### Solution: Separate Web App for Token Management

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   MAUI App      │───▶│  Blazor Web App  │───▶│ Enphase Cloud   │
│ (Local Monitor) │    │ (Token Manager)  │    │ OAuth APIs      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐    ┌──────────────────┐
│  envoy.local    │    │ Token Storage    │
│ (Local APIs)    │    │ (Database/File)  │
└─────────────────┘    └──────────────────┘
```

## Component Responsibilities

### MAUI Blazor Hybrid App
**Primary Purpose**: Real-time solar system monitoring and data visualization

**Key Features**:
- Real-time dashboard with production/consumption metrics
- Individual panel performance visualization
- Historical data graphs and trends
- Cross-platform support (Windows, Android, iOS)
- Offline capability with cached data

**Token Handling**:
- Check token validity on startup
- If token missing/expired: Display user-friendly message with instructions
- Monitor shared token storage location for updates
- Automatically resume monitoring when valid token detected

### Blazor WASM App (PWA Template)
**Primary Purpose**: Standalone PWA for monitoring with token management

**Key Features**:
- Progressive Web App with offline capabilities
- Token storage and validation
- Real-time monitoring dashboard
- Cross-platform token sharing
- Service worker with comprehensive caching

**Capabilities**:
- `Token Management` - Store and validate tokens locally
- `Monitoring Dashboard` - Real-time energy metrics display
- `Offline Operation` - Full functionality without server connection
- `PWA Installation` - Install as desktop/mobile app

### Shared Razor Class Library (RCL)
**Primary Purpose**: Common components and business logic

**Shared Components**:
- Panel performance display components
- Data visualization charts/graphs
- Configuration settings UI
- API data models and DTOs
- Utility classes for data formatting

## Data Flow

### Token Acquisition Flow
1. **MAUI App Startup**: Check for valid token in shared storage
2. **Token Missing/Expired**: Display message: "Please run the Token Manager web app to authenticate with Enphase"
3. **User Runs Web App**: Complete OAuth flow, token saved to shared storage
4. **MAUI App Detects Token**: Automatically resumes monitoring
5. **Background Refresh**: Web app periodically refreshes token before expiration

### Monitoring Flow
1. **MAUI App**: Uses token to call local `envoy.local` APIs
2. **Real-time Updates**: 15-second refresh cycle (same as TUI)
3. **Data Storage**: Enhanced CSV storage + SQLite for structured querying
4. **Visualization**: Live graphs and historical trend analysis

## Token Sharing Implementation

### Option A: File-based Sharing
```csharp
// Shared location: %LOCALAPPDATA%/EnphaseMonitor/token.json
public class TokenStorage
{
    public string? AccessToken { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? RefreshToken { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(AccessToken) && DateTime.UtcNow < ExpiryDate;
}
```

### Option B: Local REST API
Web app runs locally and provides simple REST endpoints for token access.

### Option C: SQLite Database
Shared SQLite database with token table for structured storage.

**Recommendation**: Start with Option A (file-based) for simplicity.

## Technology Stack

### MAUI Hybrid App
- **.NET 9 MAUI**: Cross-platform framework
- **Blazor Hybrid**: Web UI in native container
- **System.Text.Json**: API serialization
- **SQLite**: Enhanced local data storage
- **Chart Libraries**: For data visualization (Chart.js, Syncfusion, etc.)

### Blazor WASM App (PWA Template)
- **.NET 9 Blazor WASM**: Standalone PWA operation
- **HttpClient**: Local envoy.local API calls (via CORS proxy)
- **localStorage**: Browser-based token persistence
- **Service Worker**: Comprehensive offline caching

### Shared RCL
- **Blazor Components**: Reusable UI elements
- **Data Models**: Common DTOs and entities
- **Business Logic**: Shared utilities and services

## Development Phases

### Phase 1: Project Setup
- Create MAUI Blazor Hybrid solution from VS template
- Set up shared RCL with basic data models
- Implement file-based token sharing mechanism

### Phase 2: Token Management
- Implement token storage and validation in PWA
- Add token monitoring to MAUI app
- Cross-platform token sharing capabilities

### Phase 3: Core Monitoring
- Port TUI monitoring logic to MAUI app
- Implement real-time dashboard UI
- Add basic data visualization

### Phase 4: Enhanced Features
- Historical data visualization
- Enhanced SQLite storage
- Mobile-specific UI optimizations
- Offline capability

## Security Considerations

### Token Security
- **Web App**: Store refresh tokens securely (encrypted in database)
- **MAUI App**: Only access tokens in shared storage (shorter lifespan)
- **File Permissions**: Restrict access to token files to current user only

### Local API Access
- **Certificate Validation**: Maintain bypass for local `envoy.local` calls only
- **Network Isolation**: Ensure local-only access to Enphase device
- **Token Scope**: Use minimum required permissions for Enphase APIs

## Benefits of This Architecture

### Development Benefits
- **Proven OAuth**: Leverage existing working Blazor OAuth implementation
- **Platform Agnostic**: Avoid mobile-specific OAuth complexities
- **Separation of Concerns**: Web app handles cloud, MAUI handles local monitoring
- **Shared Code**: RCL maximizes code reuse between apps

### User Experience Benefits
- **Simple Setup**: Get token from OAuth app, then use MAUI/PWA apps daily
- **Cross-Platform**: Same monitoring experience on Windows, Android, iOS, and web browsers
- **Offline Capable**: Both MAUI and PWA apps work independently once token is acquired
- **Progressive Web App**: Install PWA on any device for native-like experience

### Maintenance Benefits
- **OAuth Updates**: Changes only affect OAuth app, not MAUI/PWA deployments
- **PWA Updates**: Service worker handles caching and offline updates
- **Local Monitoring**: Independent of cloud connectivity issues
- **Testing**: Each component can be tested and deployed independently