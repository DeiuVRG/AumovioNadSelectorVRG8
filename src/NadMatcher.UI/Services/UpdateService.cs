using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace NadMatcher.UI.Services;

public class UpdateService
{
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _updateInfo;

    public UpdateService()
    {
        // GitHub releases URL
        var source = new GithubSource("https://github.com/DeiuVRG/AumovioNadSelectorVRG8", null, false);
        _updateManager = new UpdateManager(source);
    }

    public bool IsInstalled => _updateManager.IsInstalled;

    public string? CurrentVersion => _updateManager.CurrentVersion?.ToString();

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            if (!_updateManager.IsInstalled)
            {
                // Running in development mode
                return false;
            }

            _updateInfo = await _updateManager.CheckForUpdatesAsync();
            return _updateInfo != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string? GetNewVersion()
    {
        return _updateInfo?.TargetFullRelease?.Version?.ToString();
    }

    public async Task DownloadAndApplyUpdateAsync(Action<int>? progressCallback = null)
    {
        if (_updateInfo == null)
            return;

        try
        {
            await _updateManager.DownloadUpdatesAsync(_updateInfo, progressCallback);
            _updateManager.ApplyUpdatesAndRestart(_updateInfo);
        }
        catch (Exception)
        {
            // Update failed, continue running current version
        }
    }

    public async Task ApplyUpdateOnExitAsync()
    {
        if (_updateInfo == null)
            return;

        try
        {
            await _updateManager.DownloadUpdatesAsync(_updateInfo);
            _updateManager.ApplyUpdatesAndExit(_updateInfo);
        }
        catch (Exception)
        {
            // Update failed
        }
    }
}
