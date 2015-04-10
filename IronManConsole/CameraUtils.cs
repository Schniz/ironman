using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    static class CameraUtils
    {
        public static Finger GetFingerForJoints(PXCMHandData.IHand handData, JointTypesForFinger jointTypes)
        {
            return new Finger
            {
                Tip = GetPointFromJoint(handData, jointTypes.Tip),
                BelowTip = GetPointFromJoint(handData, jointTypes.BelowTip),
                AboveBase = GetPointFromJoint(handData, jointTypes.AboveBase),
                Base = GetPointFromJoint(handData, jointTypes.Base)
            };
        }

        public static Hand GetHandFromData(PXCMHandData.IHand handData)
        {
            return new Hand
            {
                Thumb = GetFingerForJoints(handData, JointTypesForFinger.Thumb),
                Index = GetFingerForJoints(handData, JointTypesForFinger.Index),
                Middle = GetFingerForJoints(handData, JointTypesForFinger.Middle),
                Ring = GetFingerForJoints(handData, JointTypesForFinger.Ring),
                Pinky = GetFingerForJoints(handData, JointTypesForFinger.Pinky)
            };
        }

        public static Point GetPointFromJoint(PXCMHandData.IHand currentHandData, PXCMHandData.JointType jointType)
        {
            PXCMHandData.JointData jointData;
            currentHandData.QueryTrackedJoint(jointType, out jointData);
            var positionImage = jointData.positionImage;
            int x = (int)(Camera.WIDTH - positionImage.x);
            int y = (int)positionImage.y;
            return new Point
            {
                X = x,
                Y = y
            };
        }
    }
}