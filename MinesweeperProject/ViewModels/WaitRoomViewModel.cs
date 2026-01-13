using MinesweeperProject.Models;
using MinesweeperProject.Services;
using MinesweeperProject.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MinesweeperProject.ViewModels
{
    internal class WaitRoomViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;

        private readonly bool IsHost_;
        private readonly string ServerIp_;

        public SocketServerService? ServerSocket_;
        public SocketClientService? ClientSocket_;

        // 🔹 플레이어 목록
        public ObservableCollection<string> Players { get; } = new();

        // 🔹 게임 시작 가능 여부
        private bool CanStartGame_;
        public bool CanStartGame
        {
            get => CanStartGame_;
            set
            {
                CanStartGame_ = value;
                OnPropertyChanged();
            }
        }

        // 🔹 커맨드들
        public ICommand StartGameCommand { get; }
        public ICommand CloseCommand { get; }

        public WaitRoomViewModel(MainViewModel mainParent, bool IsHost, string ServerIp)
        {
            _mainParent = mainParent;
            IsHost_ = IsHost;
            ServerIp_ = ServerIp;

            // 화면 테스트용 기본값
            Players.Add("상대방 대기 중");
            CanStartGame = false;

            CloseCommand = new RelayCommand(o =>
            {
                StopSocket();
                _mainParent.ShowMainMenuView(_mainParent.Nickname!);
            });

            if (IsHost_)
            {
                ServerSocket_ = new SocketServerService();

                ServerSocket_.ClientConnected_ += OnClientConnected;
                ServerSocket_.Log_ += OnLog;

                // 서버는 대기실 들어오자마자 "리스닝 시작"
                _ = ServerSocket_.StartServerAsync(ServerIp_);
                StartGameCommand = new RelayCommand(o =>
                {

                    _mainParent.ShowServerMultiGameView(ServerSocket_);
                });
            }
            else
            {
                ClientSocket_ = new SocketClientService();

                ClientSocket_.Connected_ += OnClientConnected;
                ClientSocket_.Log_ += OnLog;

                // 클라는 바로 접속 시도
                _ = ClientSocket_.ConnectAsync(ServerIp_);
                StartGameCommand = new RelayCommand(o =>
                {

                    _mainParent.ShowClientMultiGameView(ClientSocket_);
                });
            }
        }
        private void OnClientConnected()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players.Add("상대방 접속됨");
                CanStartGame = true;
            });
        }

        private void OnLog(string Message)
        {
            System.Diagnostics.Debug.WriteLine(Message);
        }

        private void StopSocket()
        {
            ServerSocket_?.StopServer();
            ClientSocket_?.Disconnect();
        }

    }
}
