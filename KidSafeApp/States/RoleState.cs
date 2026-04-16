using System.ComponentModel;

namespace KidSafeApp.States;

public enum AppRole
{
    None = 0,
    Child = 1,
    Parent = 2,
    Teacher = 3
}

public sealed class RoleState : INotifyPropertyChanged
{
    private AppRole _currentRole = AppRole.None;
    private bool _isRoleLoaded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AppRole CurrentRole
    {
        get => _currentRole;
        private set
        {
            if (_currentRole == value) return;
            _currentRole = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRole)));
        }
    }

    public bool IsRoleLoaded
    {
        get => _isRoleLoaded;
        private set
        {
            if (_isRoleLoaded == value) return;
            _isRoleLoaded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRoleLoaded)));
        }
    }

    public async Task InitializeAsync(AppRole role, CancellationToken cancellationToken = default)
    {
        CurrentRole = role;
        IsRoleLoaded = false;

        // Placeholder for role-based initialization (API calls, permissions, module preloading).
        // Keep minimal for now to avoid breaking existing flows.
        await Task.Delay(150, cancellationToken);

        IsRoleLoaded = true;
    }

    public void SelectRole(AppRole role)
    {
        CurrentRole = role;
        IsRoleLoaded = false;
    }

    public void Reset()
    {
        CurrentRole = AppRole.None;
        IsRoleLoaded = false;
    }
}
