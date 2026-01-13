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
    public class MultiGameViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainParent;
        public ObservableCollection<Cell> Cells { get; } = new();

        public int Rows { get; private set; } = 15;
        public int Cols { get; private set; } = 15;

        public ObservableCollection<Cell> RemoteCells { get; } = new();

        public int RemoteRows { get; } = 15;

        public int RemoteCols { get; } = 15;

        public int MineCount { get; private set; } = 23; // 지뢰 개수

        private bool _isFirstClick = true; // 첫 클릭 여부
        public ICommand OpenCellCommand { get; } // 셀 열기 명령
        public ICommand ReturnToMenuCommand { get; } // 메뉴로 돌아가기 명령
        public bool IsGameWon // 게임 승리 여부
        {
            get => _isGameWon;
            set => SetProperty(ref _isGameWon, value);
        }
        public ICommand FlagCellCommand { get; } // 셀 깃발 명령
        private bool _isGameWon; // 게임 승리 여부 저장

        private readonly SocketServerService? Server_;
        private readonly SocketClientService? Client_;



        public bool IsRemoteGameWon
        {
            get => _isRemoteGameWon;
            set => SetProperty(ref _isRemoteGameWon, value);
        }
        private bool _isRemoteGameWon;

        public MultiGameViewModel(MainViewModel mainParent, SocketServerService server) // 일반 게임 생성자
        {
            _mainParent = mainParent;
            if (server != null)
                Server_ = server;

            InitializeBoard();
            InitializeRemoteBoard();
            OpenCellCommand = new RelayCommand(o => OpenCell(o as Cell));
            FlagCellCommand = new RelayCommand(o => FlagCell(o as Cell));


            if (Server_ != null)
            {
                Server_.JsonReceived_ += OnJsonReceived;
                Server_.Disconnected_ += OnDisconnected;
            }
            ReturnToMenuCommand = new RelayCommand(o => {

                LeaveGame();
            });
        }



        public MultiGameViewModel(MainViewModel mainParent, SocketClientService client) // 일반 게임 생성자
        {
            _mainParent = mainParent;
            if (client != null)
                Client_ = client;
            InitializeBoard();
            InitializeRemoteBoard();
            OpenCellCommand = new RelayCommand(o => OpenCell(o as Cell));
            FlagCellCommand = new RelayCommand(o => FlagCell(o as Cell));
            ReturnToMenuCommand = new RelayCommand(o => {
                LeaveGame();
            });

            if (Client_ != null)
            {
                Client_.JsonReceived_ += OnJsonReceived;
                Client_.Disconnected_ += OnDisconnected;
            }
        }

        private void LeaveGame()
        {
            // 🔹 서버
            if (Server_ != null)
            {
                Server_.JsonReceived_ -= OnJsonReceived;
                Server_.Disconnected_ -= OnDisconnected;
                Server_.StopServer();
            }

            // 🔹 클라이언트
            if (Client_ != null)
            {
                Client_.JsonReceived_ -= OnJsonReceived;
                Client_.Disconnected_ -= OnDisconnected;
                Client_.Disconnect();
            }

            _mainParent.ShowMainMenuView(_mainParent.Nickname!);
        }
        private void OnJsonReceived(string json)
        {
            NetworData? data;
            try
            {
                data = JsonSerializer.Deserialize<NetworData>(json);
                if (data == null) return;
            }
            catch
            {
                return;
            }

            // WPF UI 스레드에서 RemoteCells 업데이트
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 상대 보드 초기 상태면 닫혀있다가,
                // 받은 상태대로 열리고 깃발 표시됨

                foreach (var cs in data.Cells)
                {
                    int idx = cs.Row * RemoteCols + cs.Col;
                    if (idx < 0 || idx >= RemoteCells.Count) continue;

                    var cell = RemoteCells[idx];
                    cell.IsMine = cs.IsMine;
                    cell.IsOpened = cs.IsOpened;
                    cell.IsFlagged = cs.IsFlagged;
                    cell.NeighborMineCount = cs.NeighborMineCount;
                }
                CheckRemoteWin();
            });
        }

        private void OnDisconnected()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("상대와 연결이 끊겼습니다.");
                LeaveGame();
            });
        }
        private void InitializeBoard() // 게임 보드 초기화 함수 (지뢰 생성 없음)
        {
            Cells.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells.Add(new Cell { Row = r, Col = c });
                }
            }
            _isFirstClick = true;
            IsGameWon = false;
        }

        private void InitializeRemoteBoard() // 게임 보드 초기화 함수 (지뢰 생성 없음)
        {
            RemoteCells.Clear();
            for (int r = 0; r < RemoteRows; r++)
            {
                for (int c = 0; c < RemoteCols; c++)
                {
                    RemoteCells.Add(new Cell { Row = r, Col = c });
                }
            }
            IsRemoteGameWon = false;
        }

        private void PlaceMines(Cell firstCell) // 첫 클릭 후, 지뢰 배치 함수
        {
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
            }

            cell.IsOpened = true;

            if (cell.IsMine)
            {
                _ = SendMyGameAsync();
                GameOver(1);
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
            _ = SendMyGameAsync();

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
            _ = SendMyGameAsync();
        }

        private void GameOver(int WinCheck) // 게임 종료 처리 함수
        {
            if (WinCheck == 0)
            {
                MessageBox.Show($"축하합니다! 지뢰를 모두 찾으셨습니다!");
                LeaveGame();
            }
            else if (WinCheck == 1)
            {
                MessageBox.Show("지뢰를 밟았습니다! 새판으로 시작합니다.");
                InitializeBoard();
                _ = SendMyGameAsync();
            }
            else if (WinCheck == 2)
            {
                MessageBox.Show("아쉽습니다! 패배하셨습니다.");
                LeaveGame();
            }

        }


        private void CheckWin() // 승리 조건 확인 함수
        {

            bool won = Cells.Where(c => !c.IsMine).All(c => c.IsOpened);
            if (won)
            {
                IsGameWon = true;
                _ = SendMyGameAsync();
                GameOver(0);
            }
        }



        private void CheckRemoteWin() // 승리 조건 확인 함수
        {

            bool won = RemoteCells.Where(c => !c.IsMine).All(c => c.IsOpened);
            if (won)
            {
                IsRemoteGameWon = true;
                GameOver(2);
            }
        }


        public async Task SendMyGameAsync()
        {
            var networkData = new NetworData
            {
                Nickname = _mainParent.Nickname ?? "Guest",
                Rows = this.Rows,
                Cols = this.Cols,
                MineCount = this.MineCount,
                Cells = Cells.Select(c => new CellState
                {
                    Row = c.Row,
                    Col = c.Col,
                    IsMine = c.IsMine,
                    IsOpened = c.IsOpened,
                    IsFlagged = c.IsFlagged,
                    NeighborMineCount = c.NeighborMineCount
                }).ToList()
            };

            string json = JsonSerializer.Serialize(networkData);

            if (Server_ != null && Server_.IsConnected_)
            {
                await Server_.SendAsync(json);
            }
            else if (Client_ != null && Client_.IsConnected_)
            {
                await Client_.SendAsync(json);
            }

        }
    }
}