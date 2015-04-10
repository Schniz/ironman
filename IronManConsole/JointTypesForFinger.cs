using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class JointTypesForFinger
    {
        public static readonly JointTypesForFinger Thumb = new JointTypesForFinger(PXCMHandData.JointType.JOINT_PINKY_TIP, PXCMHandData.JointType.JOINT_PINKY_JT2, PXCMHandData.JointType.JOINT_PINKY_JT1, PXCMHandData.JointType.JOINT_PINKY_BASE);
        public static readonly JointTypesForFinger Index = new JointTypesForFinger(PXCMHandData.JointType.JOINT_INDEX_TIP, PXCMHandData.JointType.JOINT_INDEX_JT2, PXCMHandData.JointType.JOINT_INDEX_JT1, PXCMHandData.JointType.JOINT_PINKY_BASE);
        public static readonly JointTypesForFinger Middle = new JointTypesForFinger(PXCMHandData.JointType.JOINT_MIDDLE_TIP, PXCMHandData.JointType.JOINT_MIDDLE_JT2, PXCMHandData.JointType.JOINT_MIDDLE_JT1, PXCMHandData.JointType.JOINT_PINKY_BASE);
        public static readonly JointTypesForFinger Ring = new JointTypesForFinger(PXCMHandData.JointType.JOINT_RING_TIP, PXCMHandData.JointType.JOINT_RING_JT2, PXCMHandData.JointType.JOINT_RING_JT1, PXCMHandData.JointType.JOINT_RING_BASE);
        public static readonly JointTypesForFinger Pinky = new JointTypesForFinger(PXCMHandData.JointType.JOINT_PINKY_TIP, PXCMHandData.JointType.JOINT_PINKY_JT2, PXCMHandData.JointType.JOINT_PINKY_JT1, PXCMHandData.JointType.JOINT_PINKY_BASE);

        public PXCMHandData.JointType Tip { get; set; }
        public PXCMHandData.JointType Base { get; set; }
        public PXCMHandData.JointType BelowTip { get; set; }
        public PXCMHandData.JointType AboveBase { get; set; }

        private JointTypesForFinger(PXCMHandData.JointType tip, PXCMHandData.JointType belowTip, PXCMHandData.JointType aboveBase, PXCMHandData.JointType fingerBase)
        {
            this.Tip = tip;
            this.Base = fingerBase;
            this.BelowTip = belowTip;
            this.AboveBase = aboveBase;
        }
    }
}
