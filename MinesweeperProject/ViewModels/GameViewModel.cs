using MinesweeperProject.Models;
using MinesweeperProject.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MinesweeperProject.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        public ObservableCollection<Cell> Cells { get; } = new();

        public int Rows { get; private set; } // 열 정보
        public int Cols { get; private set; } // 행 정보
        public int MineCount { get; private set; } // 지뢰 개수
        private bool _isFirstClick = true; // 첫 클릭 여부
        public ICommand OpenCellCommand { get; } // 셀 열기 명령
        public ICommand ReturnToMenuCommand { get; } // 메뉴로 돌아가기 명령
        public bool IsGameWon // 게임 승리 여부
        {
            get => _isGameWon;
            set => SetProperty(ref _isGameWon, value);
        }
        public int CurrentTime // 현재 시간 속성
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }
        public ICommand FlagCellCommand { get; } // 셀 깃발 명령
        private bool _isGameWon; // 게임 승리 여부 저장
        private DispatcherTimer _timer; // 타이머
        private int _currentTime; // 현재 시간
        private bool _isTimerRunning; // 타이머 실행 여부
        public string TimeDisplay => $"{CurrentTime / 60:D2}:{CurrentTime % 60:D2}"; // 시간 표시 형식
        public string DifficultyName { get; private set; } // 난이도 이름 (랭킹 저장용)
        public GameViewModel(string difficulty, MainViewModel mainParent) // 일반 게임 생성자
        {
            _mainParent = mainParent;
            SetDifficulty(difficulty);
            InitializeBoard();
            this.DifficultyName = difficulty;
            OpenCellCommand = new RelayCommand(o => OpenCell(o as Cell));
            FlagCellCommand = new RelayCommand(o => FlagCell(o as Cell));
            ReturnToMenuCommand = new RelayCommand(o => {
                if (MessageBox.Show("현재 진행 상황을 저장하고 나갈까요?", "저장 확인",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SaveGame();
                }
                _mainParent.ShowMainMenuView(_mainParent.Nickname!);
            });
            SetupTimer();
        }

        public GameViewModel(SaveData saveData, MainViewModel mainParent)
        {
            _mainParent = mainParent;
            this.DifficultyName = saveData.DifficultyName;
            this.Rows = saveData.Rows;
            this.Cols = saveData.Cols;
            this.MineCount = saveData.MineCount;

            Cells.Clear();
            foreach (var state in saveData.Cells)
            {
                var cell = new Cell(state.Row, state.Col)
                {
                    IsMine = state.IsMine,
                    IsOpened = state.IsOpened,
                    IsFlagged = state.IsFlagged,
                    NeighborMineCount = state.NeighborMineCount
                };
                Cells.Add(cell);
            }

            _isFirstClick = !Cells.Any(c => c.IsOpened);

            OpenCellCommand = new RelayCommand(o => OpenCell(o as Cell));
            FlagCellCommand = new RelayCommand(o => FlagCell(o as Cell));
            ReturnToMenuCommand = new RelayCommand(o => {
                if (MessageBox.Show("현재 진행 상황을 저장하고 나갈까요?", "저장 확인",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    SaveGame();
                }
                _mainParent.ShowMainMenuView(_mainParent.Nickname!);
            });

            this.CurrentTime = saveData.CurrentTime;
            SetupTimer();

            if (!_isFirstClick)
            {
                _timer.Start();
                _isTimerRunning = true;
            }

            OnPropertyChanged(nameof(TimeDisplay));
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(Cols));
        }

        private void SetDifficulty(string difficulty) // 난이도 설정 함수
        {
            switch (difficulty)
            {
                case "쉬움": Rows = 10; Cols = 10; MineCount = 6; break;
                case "보통": Rows = 20; Cols = 20; MineCount = 40; break;
                case "어려움": Rows = 30; Cols = 30; MineCount = 150; break;
                case "극한": Rows = 30; Cols = 60; MineCount = 400; break;
            }
        }

        private void SetupTimer() // 타이머 설정 함수
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => {
                CurrentTime++;
                OnPropertyChanged(nameof(TimeDisplay));
            };
        }

        private void InitializeBoard() // 게임 보드 초기화 함수 (지뢰 생성 없음)
        {
            Cells.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells.Add(new Cell{ Row = r, Col = c });
                }
            }
            _isFirstClick = true;
            CurrentTime = 0;
            IsGameWon = false;
        }

        private void PlaceMines(Cell firstCell) // 첫 클릭 후, 지뢰 배치 함수
        {
            if (Cells.Any(c => c.IsMine)) return;
            Random rand = new Random();

            var safeZone = GetNeighbors(firstCell).ToList();
            safeZone.Add(firstCell);

            var candidates = Cells.Except(safeZone).OrderBy(x => rand.Next()).ToList();

            foreach (var cell in candidates.Take(MineCount))
            {
                cell.IsMine = true;
            }

            foreach (var cell in Cells)
            {
                cell.NeighborMineCount = GetNeighbors(cell).Count(n => n.IsMine);
            }
        }

        public void OpenCell(Cell? cell) // 셀 열기 함수
        {
            if (cell == null || cell.IsOpened || cell.IsFlagged) return;

            if (_isFirstClick)
            {
                PlaceMines(cell);
                _isFirstClick = false;

                if (!_isTimerRunning)
                {
                    _timer.Start();
                    _isTimerRunning = true;
                }
            }

            cell.IsOpened = true;

            if (cell.IsMine)
            {
                _timer.Stop();
                GameOver(false);
                return;
            }

            if (cell.NeighborMineCount == 0)
            {
                foreach (var neighbor in GetNeighbors(cell))
                {
                    if (!neighbor.IsOpened) OpenCell(neighbor);
                }
            }

            CheckWin();
        }

        private IEnumerable<Cell> GetNeighbors(Cell cell) // 인접 셀 정보 확인 함수
        {
            for (int r = cell.Row - 1; r <= cell.Row + 1; r++)
            {
                for (int c = cell.Col - 1; c <= cell.Col + 1; c++)
                {
                    if (r == cell.Row && c == cell.Col) continue;
                    if (r >= 0 && r < Rows && c >= 0 && c < Cols)
                    {
                        yield return Cells[r * Cols + c];
                    }
                }
            }
        }

        public void FlagCell(Cell? cell) // 깃발 설치 함수
        {
            if (cell == null || cell.IsOpened) return;
            cell.IsFlagged = !cell.IsFlagged;
        }

        private void GameOver(bool isWin) // 게임 종료 처리 함수
        {
            if (isWin)
            {
                AudioService.Instance.PlaySFX("Victory.wav");
                _mainParent.UpdateRanking(this.DifficultyName, this.CurrentTime);
                MessageBox.Show($"축하합니다! {TimeDisplay} 만에 클리어하였습니다!");
            }
            else
            {
                AudioService.Instance.PlaySFX("Bomb.wav");
                MessageBox.Show("지뢰를 밟았습니다!");
            }
            _mainParent.ShowMainMenuView(_mainParent.Nickname!);
        }


        private void CheckWin() // 승리 조건 확인 함수
        {
            if (IsGameWon) return;
            bool won = Cells.Where(c => !c.IsMine).All(c => c.IsOpened);
            if (won)
            {
                _timer.Stop();
                IsGameWon = true;
                GameOver(true);
                File.Delete("savegame.json");
            }
        }

        public void SaveGame() // 게임 저장 함수
        {
            var saveData = new SaveData
            {
                DifficultyName = this.DifficultyName,
                CurrentTime = this.CurrentTime,
                Nickname = _mainParent.Nickname ?? "Guest",
                Rows = this.Rows,
                Cols = this.Cols,
                MineCount = this.MineCount,
                Cells = Cells.Select(c => new CellSaveState
                {
                    Row = c.Row,
                    Col = c.Col,
                    IsMine = c.IsMine,
                    IsOpened = c.IsOpened,
                    IsFlagged = c.IsFlagged,
                    NeighborMineCount = c.NeighborMineCount
                }).ToList()
            };

            string json = JsonSerializer.Serialize(saveData);
            File.WriteAllText("savegame.json", json);
        }

        
    }
}