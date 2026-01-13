using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperProject.Models
{
    internal class NetworkData
    {
    }

    public class NetworData
    {
        public string Nickname { get; set; } = string.Empty; //
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int MineCount { get; set; }
        public List<CellState> Cells { get; set; } = new(); //
    }

    public class CellState
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public bool IsMine { get; set; }
        public bool IsOpened { get; set; }
        public bool IsFlagged { get; set; }
        public int NeighborMineCount { get; set; }
        public int CurrentTime { get; set; }
    }
}
