using System.ComponentModel;

namespace KidSafeApp.States;

public sealed class AdminSessionState : INotifyPropertyChanged
{
    private bool _isAdmin;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsAdmin
    {
        get => _isAdmin;
        private set
        {
            if (_isAdmin == value) return;
            _isAdmin = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdmin)));
        }
    }

    public void SignInAdmin() => IsAdmin = true;

    public void SignOut() => IsAdmin = false;
}
