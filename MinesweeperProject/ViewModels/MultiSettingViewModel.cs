using MinesweeperProject.Services;
using MinesweeperProject.Models;
using MinesweeperProject.View;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace MinesweeperProject.ViewModels
{
    public class MultiSettingViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        public ICommand MakeRoomCommand { get; }
        public ICommand IncomeRoomCommand { get; }
        public ICommand CloseCommand { get; }


        private string ServerIp_ = "127.0.0.1";

        private string MyPlayName_ => _mainParent.Nickname ?? "Guest";

        public string ServerIp
        {
            get => ServerIp_;
            set
            {
                ServerIp_ = value;
                OnPropertyChanged();
            }
        }



        public string DisplayName => $"현재 사용자: {_mainParent.Nickname ?? "Guest"}";

        public MultiSettingViewModel(MainViewModel mainParent)
        {
            _mainParent = mainParent;

            CloseCommand = new RelayCommand(o => _mainParent.ShowMainMenuView(_mainParent.Nickname!));

            MakeRoomCommand = new RelayCommand(o =>
            {
                _mainParent.ShowWaitRoomView(isHost: true, serverIp: ServerIp);
            });

            IncomeRoomCommand = new RelayCommand(o =>
            {
                _mainParent.ShowWaitRoomView(isHost: false, serverIp: ServerIp);
            });

        }
    }
}