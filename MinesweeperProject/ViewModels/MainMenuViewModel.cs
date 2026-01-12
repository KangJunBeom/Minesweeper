using MinesweeperProject.Services;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace MinesweeperProject.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        public ICommand StartGameCommand { get; }
        public ICommand LoadGameCommand { get; }
        public ICommand OpenSettingCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand ShowRankingCommand { get; }
        public string DisplayName => $"현재 사용자: {_mainParent.Nickname ?? "Guest"}";
        public MainMenuViewModel(MainViewModel mainParent)
        {
            _mainParent = mainParent;

            StartGameCommand = new RelayCommand(o =>
            {
                string saveFilePath = "savegame.json";
                if (File.Exists(saveFilePath))
                {
                    try
                    {
                        File.Delete(saveFilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("저장 파일 삭제 실패: " + ex.Message);
                    }
                }

                string difficulty = o?.ToString() ?? "쉬움";
                _mainParent.ShowGameView(difficulty);
            });

            LoadGameCommand = new RelayCommand(
                o => {
                    if (File.Exists("savegame.json"))
                    {
                        string json = File.ReadAllText("savegame.json");
                        var saveData = System.Text.Json.JsonSerializer.Deserialize<Models.SaveData>(json);
                        if (saveData != null)
                        {
                            _mainParent.RestoreGame(saveData);
                        }
                    }
                },
                o => File.Exists("savegame.json")
            );


            ExitCommand = new RelayCommand(o =>
            {
                if (MessageBox.Show("게임을 종료하시겠습니까?", "종료 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            });

            ShowRankingCommand = new RelayCommand(o => _mainParent.ShowRankingView());

            OpenSettingCommand = new RelayCommand(o => _mainParent.ShowSettingView());
        }
    }
}