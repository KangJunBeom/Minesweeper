using System.Windows.Input;
using MinesweeperProject.Services;

namespace MinesweeperProject.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        private string _nickname = string.Empty;

        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainParent)
        {
            _mainParent = mainParent;
            LoginCommand = new RelayCommand(
                execute: o => _mainParent.ShowMainMenuView(Nickname),
                canExecute: o => !string.IsNullOrWhiteSpace(Nickname)
            );
        }
    }
}