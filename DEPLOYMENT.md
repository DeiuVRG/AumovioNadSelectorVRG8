# AumovioNadSelectorVRG8 - Deployment Guide

## Prerequisites

1. **.NET 9.0 SDK** installed on development machine
2. **GitHub account** with a repository created
3. **Velopack CLI** (will be installed automatically by the publish script)

## Initial Setup (One-time)

### 1. Create GitHub Repository

1. Go to https://github.com/new
2. Create a repository named `AumovioNadSelectorVRG8` (or your preferred name)
3. Keep it public (free) or private (requires GitHub Pro for release downloads)

### 2. GitHub Repository URL (Already Configured)

The application is configured to check for updates from:
```
https://github.com/DeiuVRG/AumovioNadSelectorVRG8
```

This is set in `src/NadMatcher.UI/Services/UpdateService.cs`.

## Publishing a New Release

### Step 1: Update Version Number

Edit `src/NadMatcher.UI/NadMatcher.UI.csproj` and update:

```xml
<Version>1.0.1</Version>
<FileVersion>1.0.1.0</FileVersion>
<InformationalVersion>1.0.1</InformationalVersion>
```

### Step 2: Run the Publish Script

Open PowerShell in the NadMatcher folder and run:

```powershell
.\publish.ps1 -Version "1.0.1"
```

This will:
- Build the application for Windows x64
- Create a self-contained executable (no .NET install required for users)
- Package it with Velopack

### Step 3: Create GitHub Release

1. Go to your GitHub repository > Releases > "Create a new release"
2. Tag: `v1.0.1` (same as your version)
3. Title: `AumovioNadSelectorVRG8 v1.0.1`
4. Description: List the changes in this version
5. **Upload all files** from the `releases` folder:
   - `AumovioNadSelectorVRG8-win-Setup.exe` - Main installer for new users
   - `AumovioNadSelectorVRG8-1.0.1-full.nupkg` - Full update package
   - `RELEASES` - Release manifest file
6. Publish the release

## User Installation

### First-time Installation

Users download and run `AumovioNadSelectorVRG8-win-Setup.exe` from the latest GitHub release.

The installer will:
- Install the application to `%LOCALAPPDATA%\AumovioNadSelectorVRG8`
- Create Start Menu shortcut
- Create Desktop shortcut (optional)

### Automatic Updates

When users launch the application:
1. It checks GitHub for new releases
2. If a new version is available, a green "Update" button appears in the header
3. Clicking the button downloads and installs the update
4. The app restarts automatically with the new version

## Troubleshooting

### Application doesn't detect updates
- Ensure the GitHub repository URL in `UpdateService.cs` is correct
- Ensure the release is published (not draft)
- Ensure the RELEASES file is uploaded with each release

### Users can't download from private repo
- For private repos, you need to provide a GitHub personal access token
- Alternatively, use OneDrive/SharePoint for distribution (simpler for internal tools)

## Alternative: OneDrive/SharePoint Distribution

If you prefer not to use GitHub, you can use OneDrive:

1. Create a shared folder on OneDrive
2. Run the publish script as usual
3. Copy the `releases` folder contents to OneDrive
4. Share the folder link with colleagues

Update `UpdateService.cs` to use a simple HTTP source:
```csharp
var source = new SimpleWebSource("https://your-onedrive-direct-link/releases");
```

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-29 | Initial release |
