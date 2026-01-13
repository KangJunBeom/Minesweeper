using MinesweeperProject.Models;
using MinesweeperProject.Services;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

// ViewModels/MainViewModel.cs
namespace MinesweeperProject.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SizeToContent _windowSizeToContent = SizeToContent.Manual;
        public SizeToContent WindowSizeToContent
        {
            get => _windowSizeToContent;
            set => SetProperty(ref _windowSizeToContent, value);
        }
        public ICommand ShowSettingCommand { get; } // 설정 창 전환 커맨드
        // 창의 기본 가로/세로 크기도 바인딩할 수 있도록 추가
        private double _windowWidth = 400;
        public double WindowWidth { get => _windowWidth; set => SetProperty(ref _windowWidth, value); }

        private double _windowHeight = 500;
        public double WindowHeight { get => _windowHeight; set => SetProperty(ref _windowHeight, value); }
        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // 속성 이름을 Nickname으로 통일하고, SetProperty를 사용해 UI 알림을 보냅니다.
        private string? _nickname;

        private const string RankingFileName = "rankings.json";
        public string? Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        public MainViewModel()
        {
            ShowSettingCommand = new RelayCommand(o => ShowSettingView());
            ShowLoginView();
            AudioService.Instance.PlayBGM("Drum_Or_Bass.mp3");
        }

        public void ShowLoginView() // 로그인화면 불러오기
        {
            WindowSizeToContent = SizeToContent.Manual;
            WindowWidth = 400;
            WindowHeight = 400;
            CurrentViewModel = new LoginViewModel(this);
        }

        public void ShowMainMenuView(string nickname) //메인 메뉴 화면 불러오기
        {
            this.Nickname = nickname;
            WindowSizeToContent = SizeToContent.Manual;
            WindowWidth = 500;
            WindowHeight = 700;
            CurrentViewModel = new MainMenuViewModel(this);
        }

        public void ShowGameView(string difficulty) // 새 게임 시작
        {
            System.Windows.MessageBox.Show($"{difficulty} 난이도로 게임을 시작합니다!");
            WindowSizeToContent = SizeToContent.WidthAndHeight;
            CurrentViewModel = new GameViewModel(difficulty, this);
        }

        public void RestoreGame(SaveData data) // 게임 불러오기
        {

            WindowSizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            var gameVM = new GameViewModel(data, this);
            CurrentViewModel = gameVM;
        }

        public void UpdateRanking(string difficulty, int time) // 랭킹 업데이트 (파일 불러오기)
        {
            RankingData data;

            if (File.Exists(RankingFileName))
            {
                try
                {
                    string json = File.ReadAllText(RankingFileName);
                    data = JsonSerializer.Deserialize<RankingData>(json) ?? new RankingData();
                }
                catch
                {
                    data = new RankingData();
                }
            }
            else
            {
                data = new RankingData();
            }

            if (data.DifficultyRankings == null)
            {
                data.DifficultyRankings = new Dictionary<string, List<RankingEntry>>();
            }

            if (!data.DifficultyRankings.ContainsKey(difficulty))
            {
                data.DifficultyRankings[difficulty] = new List<RankingEntry>();
            }

            var entry = new RankingEntry
            {
                Nickname = string.IsNullOrEmpty(this.Nickname) ? "Guest" : this.Nickname,
                Time = time,
                Difficulty = difficulty
            };

            data.DifficultyRankings[difficulty].Add(entry);
            data.DifficultyRankings[difficulty] = data.DifficultyRankings[difficulty]
                .OrderBy(x => x.Time)
                .Take(3)
                .ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            string serializedData = JsonSerializer.Serialize(data, options);
            File.WriteAllText(RankingFileName, serializedData);
        }

        public void ShowRankingView() // 랭킹 화면 불러오기
        {
            WindowSizeToContent = System.Windows.SizeToContent.Manual;
            WindowWidth = 500;
            WindowHeight = 700;
            CurrentViewModel = new RankingViewModel(this);
        }

        public void ShowSettingView()
        {
            WindowSizeToContent = SizeToContent.Manual;
            WindowWidth = 500;
            WindowHeight = 700;
            CurrentViewModel = new SettingViewModel(this);
        }

        public void ShowMultiSettingView()
        {
            WindowSizeToContent = SizeToContent.Manual; // 고정 크기 모드
            WindowWidth = 500;
            WindowHeight = 700;
            CurrentViewModel = new MultiSettingViewModel(this);
        }

        public void ShowWaitRoomView(bool isHost, string serverIp)
        {
            WindowSizeToContent = SizeToContent.Manual; // 고정 크기 모드
            WindowWidth = 500;
            WindowHeight = 600;
            CurrentViewModel = new WaitRoomViewModel(this, isHost, serverIp);
        }

        public void ShowServerMultiGameView(SocketServerService server)
        {
            // 우선 전환이 잘 되는지 확인하기 위해 메시지 박스를 띄웁니다.
            System.Windows.MessageBox.Show($"멀티 게임을 시작합니다!");
            WindowSizeToContent = SizeToContent.WidthAndHeight;
            CurrentViewModel = new MultiGameViewModel(this, server);
        }
        public void ShowClientMultiGameView(SocketClientService client)
        {
            // 우선 전환이 잘 되는지 확인하기 위해 메시지 박스를 띄웁니다.
            System.Windows.MessageBox.Show($"멀티 게임을 시작합니다!");
            WindowSizeToContent = SizeToContent.WidthAndHeight;
            CurrentViewModel = new MultiGameViewModel(this, client);
        }

    }
}
