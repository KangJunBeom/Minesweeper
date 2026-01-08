using MinesweeperProject.Models;
using System.Windows;
using System.Text.Json;
using System.IO;

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
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            WindowSizeToContent = SizeToContent.Manual; // 고정 크기 모드
            WindowWidth = 400;
            WindowHeight = 500;
            CurrentViewModel = new LoginViewModel(this);
        }

        public void ShowMainMenuView(string nickname)
        {
            this.Nickname = nickname;
            WindowSizeToContent = SizeToContent.Manual; // 고정 크기 모드
            WindowWidth = 500;
            WindowHeight = 600;
            CurrentViewModel = new MainMenuViewModel(this);
        }

        // 4. 게임 화면으로 전환하는 메서드를 추가합니다.
        public void ShowGameView(string difficulty)
        {
            // 우선 전환이 잘 되는지 확인하기 위해 메시지 박스를 띄웁니다.
            System.Windows.MessageBox.Show($"{difficulty} 난이도로 게임을 시작합니다!");
            WindowSizeToContent = SizeToContent.WidthAndHeight;
            CurrentViewModel = new GameViewModel(difficulty, this);
        }

        public void RestoreGame(SaveData data)
        {

            WindowSizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            var gameVM = new GameViewModel(data, this);
            CurrentViewModel = gameVM;
        }

        public void UpdateRanking(string difficulty, int time)
        {
            RankingData data;
            if (File.Exists(RankingFileName))
            {
                string json = File.ReadAllText(RankingFileName);
                data = JsonSerializer.Deserialize<RankingData>(json) ?? new RankingData();
            }
            else
            {
                data = new RankingData();
            }

            var entry = new RankingEntry { Nickname = this.Nickname!, Time = time, Date = DateTime.Now };
            data.DifficultyRankings[difficulty].Add(entry);

            data.DifficultyRankings[difficulty] = data.DifficultyRankings[difficulty]
                .OrderBy(x => x.Time)
                .Take(3)
                .ToList();

            File.WriteAllText(RankingFileName, JsonSerializer.Serialize(data));
        }

        public void ShowRankingView()
        {
            WindowSizeToContent = System.Windows.SizeToContent.Manual;
            WindowWidth = 400;
            WindowHeight = 550;
            CurrentViewModel = new RankingViewModel(this);
        }
    }
}
