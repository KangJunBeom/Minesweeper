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

        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int MineCount { get; private set; }
        private bool _isFirstClick = true;
        public ICommand OpenCellCommand { get; }
        public ICommand ReturnToMenuCommand { get; }
        public bool IsGameWon
        {
            get => _isGameWon;
            set => SetProperty(ref _isGameWon, value);
        }

        public ICommand FlagCellCommand { get; } // 깃발 커맨드 추가
        private bool _isGameWon;
        private DispatcherTimer _timer;
        private int _currentTime;
        private bool _isTimerRunning;
        public string TimeDisplay => $"{CurrentTime / 60:D2}:{CurrentTime % 60:D2}";
        public string DifficultyName { get; private set; }
        public GameViewModel(string difficulty, MainViewModel mainParent)
        {
            _mainParent = mainParent;
            SetDifficulty(difficulty);
            InitializeBoard();
            this.DifficultyName = difficulty;
            OpenCellCommand = new RelayCommand(o => OpenCell(o as Cell));
            // 우클릭 시 실행될 커맨드 연결
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

        private void SetDifficulty(string difficulty)
        {
            switch (difficulty)
            {
                case "쉬움": Rows = 10; Cols = 10; MineCount = 5; break;
                case "보통": Rows = 20; Cols = 20; MineCount = 40; break;
                case "어려움": Rows = 30; Cols = 30; MineCount = 150; break;
                case "극한": Rows = 30; Cols = 60; MineCount = 400; break;
            }
        }

        public GameViewModel(SaveData saveData, MainViewModel mainParent)
        {
            _mainParent = mainParent;
            this.DifficultyName = saveData.DifficultyName;
            // 1. 기본 정보 복구
            this.Rows = saveData.Rows;
            this.Cols = saveData.Cols;
            this.MineCount = saveData.MineCount;

            // 2. 셀 리스트 복구
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

            // 3. 커맨드 초기화 (기존 생성자와 동일)
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

            this.CurrentTime = saveData.CurrentTime; // 저장된 시간 불러오기
            SetupTimer();
            OnPropertyChanged(nameof(TimeDisplay));
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => {
                CurrentTime++;
                OnPropertyChanged(nameof(TimeDisplay));
            };
        }

        private void InitializeBoard()
        {
            Cells.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells.Add(new Cell{ Row = r, Col = c });
                }
            }
            _isFirstClick = true; // 플래그 초기화
            CurrentTime = 0;
            IsGameWon = false;
        }

        // 2. 지뢰 배치 메서드 (첫 클릭 위치를 인자로 받음)
        private void PlaceMines(Cell firstCell)
        {
            Random rand = new Random();

            // 클릭한 칸과 그 주변 8칸을 안전 영역으로 설정
            var safeZone = GetNeighbors(firstCell).ToList();
            safeZone.Add(firstCell);

            // 안전 영역을 제외한 나머지 칸들 중 지뢰를 심을 후보지 선정
            var candidates = Cells.Except(safeZone).OrderBy(x => rand.Next()).ToList();

            // 지뢰 배치
            foreach (var cell in candidates.Take(MineCount))
            {
                cell.IsMine = true;
            }

            // 모든 셀의 주변 지뢰 개수 계산
            foreach (var cell in Cells)
            {
                cell.NeighborMineCount = GetNeighbors(cell).Count(n => n.IsMine);
            }
        }

        // 3. OpenCell 메서드 수정
        public void OpenCell(Cell? cell)
        {
            if (cell == null || cell.IsOpened || cell.IsFlagged) return;

            // [핵심] 첫 클릭이라면 이때 지뢰를 배치함
            if (_isFirstClick)
            {
                PlaceMines(cell);
                _isFirstClick = false;

                // 타이머 시작
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

        // 주변 8개의 셀을 가져오는 헬퍼 메서드
        private IEnumerable<Cell> GetNeighbors(Cell cell)
        {
            for (int r = cell.Row - 1; r <= cell.Row + 1; r++)
            {
                for (int c = cell.Col - 1; c <= cell.Col + 1; c++)
                {
                    if (r == cell.Row && c == cell.Col) continue; // 자기 자신 제외
                    if (r >= 0 && r < Rows && c >= 0 && c < Cols)
                    {
                        yield return Cells[r * Cols + c];
                    }
                }
            }
        }

        public void FlagCell(Cell? cell)
        {
            if (cell == null || cell.IsOpened) return;
            cell.IsFlagged = !cell.IsFlagged;
        }

        private void GameOver(bool isWin)
        {
            if (isWin)
            {
                _mainParent.UpdateRanking(this.DifficultyName, this.CurrentTime); // DifficultyName 속성 필요
                MessageBox.Show($"축하합니다! {TimeDisplay} 만에 클리어하여 랭킹에 등록되었습니다!");
            }
            else
            {
                MessageBox.Show("지뢰를 밟았습니다!");
            }
            _mainParent.ShowMainMenuView(_mainParent.Nickname!);
        }


        private void CheckWin()
        {
            // 지뢰가 아닌 모든 칸이 열렸는지 확인
            bool won = Cells.Where(c => !c.IsMine).All(c => c.IsOpened);
            if (won)
            {
                _timer.Stop();
                IsGameWon = true; // "VICTORY!" 글자 애니메이션은 유지하고 싶다면 남겨두세요.

                // 지체 없이 바로 승리 알림을 띄웁니다.
                GameOver(true);
                File.Delete("savegame.json");
            }
        }

        public void SaveGame()
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
            File.WriteAllText("savegame.json", json); // 실행 파일 위치에 저장
        }

        public int CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }
    }
}