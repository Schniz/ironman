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

        public static Hand FromHandData(PXCMHandData.IHand handData)
        {
            Hand hand = new Hand();

            return null;
        }

        public IEnumerable<Finger> Fingers
        {
            get
            {
                yield return Thumb;
                yield return Index;
                yield return Middle;
                yield return Ring;
                yield return Pinky;
            }
        }

        public IEnumerable<Finger> FingersWithoutThumb
        {
            get
            {
                yield return Index;
                yield return Middle;
                yield return Ring;
                yield return Pinky;
            }
        }

        public string Gestrue { get; set; }

        public float z { get; set; }

        public int CountFingers()
        {
            return this.FingersWithoutThumb.Count(finger => finger.IsFingerFolded());
        }

        public bool IsRock()
        {
            return !Index.IsFingerFolded() && !Pinky.IsFingerFolded() && Middle.IsFingerFolded() && Ring.IsFingerFolded();
        }
    }
}
