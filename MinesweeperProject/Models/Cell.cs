using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperProject.Models
{
    public class Cell : ViewModels.ViewModelBase
    {
        private int _neighborMineCount;
        public int NeighborMineCount
        {
            get => _neighborMineCount;
            set => SetProperty(ref _neighborMineCount, value);
        }

        public int Row { get; set; }
        public int Col { get; set; }

        private bool _isMine;
        public bool IsMine
        {
            get => _isMine;
            set => SetProperty(ref _isMine, value);
        }

        private bool _isOpened;
        public bool IsOpened
        {
            get => _isOpened;
            set => SetProperty(ref _isOpened, value);
        }

        private bool _isFlagged;
        public bool IsFlagged
        {
            get => _isFlagged;
            set => SetProperty(ref _isFlagged, value);
        }
        public Cell()
        {
        }
        public Cell(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}