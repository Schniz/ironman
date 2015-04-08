using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class Hand
    {
        public Finger Thumb { get; set; }
        public Finger Index { get; set; }
        public Finger Middle { get; set; }
        public Finger Ring { get; set; }
        public Finger Pinky { get; set; }

        public string Gestrue { get; set; }

        public float z { get; set; }

        public int CountFingers()
        {
            int fingers = 4;

            if (Index.IsFingerFolded()) fingers--;
            if (Middle.IsFingerFolded()) fingers--;
            if (Ring.IsFingerFolded()) fingers--;
            if (Pinky.IsFingerFolded()) fingers--;

            return fingers;
        }
    }
}
