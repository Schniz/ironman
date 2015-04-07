using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class Finger
    {
        public Point Tip { get; set; }
        public Point Base { get; set; }
        public Point AboveBase { get; set; }
        public Point BelowTip { get; set; }
        public Point Center { get; set; }

        public bool IsFingerFolded()
        {
            return Math.Pow(Tip.X - Center.X, 2) + Math.Pow(Tip.Y - Center.Y, 2) <
                Math.Pow(AboveBase.X - Center.X, 2) + Math.Pow(AboveBase.Y - Center.Y, 2);
        }
    }
}
