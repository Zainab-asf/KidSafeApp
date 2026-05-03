using System.ComponentModel;

namespace KidSafeApp.StateManagement;

public enum AppRole
{
    None = 0,
    Child = 1,
    Parent = 2,
    Teacher = 3,
    Admin = 4
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

        IsRoleLoaded = true;
        await Task.CompletedTask;
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
