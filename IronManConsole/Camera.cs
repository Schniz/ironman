using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    public enum Status
    {
        none,
        afterSpreadfingers
    }
    public class Camera
    {
        const int WIDTH = 640;
        const int HEIGHT = 480;
        private const int FRAMES_WAIT = 0;

        private Win32framework win;

        private MedianStuff cursorMedian = new MedianStuff();

        private PXCMSession _session;
        private PXCMSenseManager _mngr;
        private PXCMSenseManager.Handler _handler;

        private Hand hand;

        private PXCMHandModule _hand;
        private PXCMHandData _handData;
        private Point lastLocation;

        private Status status;
        private int countFrames;

        private PXCMHandConfiguration.OnFiredGestureDelegate[] delegates;
        private PXCMHandData.AlertData lastAlertData;

        public Camera(params PXCMHandConfiguration.OnFiredGestureDelegate[] dlgts)
        {
            this.win = new Win32framework();
            this.delegates = dlgts;

            this.status = Status.none;
            this.countFrames = 0;

            // Create the manager
            this._session = PXCMSession.CreateInstance();
            this._mngr = this._session.CreateSenseManager();

            // streammmm
            PXCMVideoModule.DataDesc desc = new PXCMVideoModule.DataDesc();
            desc.deviceInfo.streams = PXCMCapture.StreamType.STREAM_TYPE_COLOR | PXCMCapture.StreamType.STREAM_TYPE_DEPTH;
            this._mngr.EnableStreams(desc);
            //this._mngr.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, Camera.WIDTH, Camera.HEIGHT, 30);
            //this._mngr.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, Camera.WIDTH, Camera.HEIGHT, 30);


            // Hands
            this._mngr.EnableHand();
            this._hand = this._mngr.QueryHand();
            this._handData = this._hand.CreateOutput();

            // Hands config
            PXCMHandConfiguration conf = this._hand.CreateActiveConfiguration();
            //conf.EnableGesture("thumb_up", false);
            //conf.EnableGesture("swipe_left", false);
            //conf.EnableGesture("swipe_up", false);
            //conf.EnableGesture("swipe_down", false);
            //conf.EnableGesture("swipe_right", false);
            conf.EnableGesture("spreadfingers", false);

            // Subscribe hands alerts
            conf.EnableAllAlerts();
            conf.SubscribeAlert(this.onFiredAlert);

            conf.EnableTrackedJoints(true);

            // and the private one for debug
            conf.SubscribeGesture(this.onFiredGesture);

            // Apply it all
            conf.ApplyChanges();

            // Set events
            this._handler = new PXCMSenseManager.Handler();
            this._handler.onModuleProcessedFrame = this.onModuleProcessedFrame;

            this._mngr.Init(this._handler);
        }

        public void Start()
        {
            this._mngr.StreamFrames(false);
        }

        public void Stop()
        {
            this._mngr.Close();
        }

        private pxcmStatus onModuleProcessedFrame(int mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            if (mid == PXCMHandModule.CUID)
            {
                this._handData.Update();
                this.updateHand();

                
            }

            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void updateHand()
        {
            int numberOfHands = this._handData.QueryNumberOfHands();
            if (numberOfHands < 1) return;

            if (this.status == Status.afterSpreadfingers)
            {
                if (this.countFrames == FRAMES_WAIT)
                {
                    var x = this.hand.Middle.Tip.X;

                    if (Math.Abs(x - this.lastLocation.X) > 100)
                    {
                        this.countFrames = 0;
                        this.status = Status.none;
                        if (x < this.lastLocation.X)
                        {
                            Console.WriteLine("Left");
                        }
                        else
                        {
                            Console.WriteLine("Right");
                        }
                    }
                }
                else
                {
                    this.countFrames++;
                }
            }

            PXCMHandData.IHand currentHandData;
            this._handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, 0, out currentHandData);

            var center = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_CENTER);

            Hand h = new Hand
            {
                Thumb = new Finger
                {
                    Tip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_THUMB_TIP),
                    BelowTip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_THUMB_JT2),
                    AboveBase = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_THUMB_JT1),
                    Base = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_THUMB_BASE),
                    Center = center
                },
                Index = new Finger
                {
                    Tip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_INDEX_TIP),
                    BelowTip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_INDEX_JT2),
                    AboveBase = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_INDEX_JT1),
                    Base = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_INDEX_BASE),
                    Center = center
                },
                Middle = new Finger
                {
                    Tip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_MIDDLE_TIP),
                    BelowTip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_MIDDLE_JT2),
                    AboveBase = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_MIDDLE_JT1),
                    Base = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_MIDDLE_BASE),
                    Center = center
                },
                Ring = new Finger
                {
                    Tip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_RING_TIP),
                    BelowTip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_RING_JT2),
                    AboveBase = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_RING_JT1),
                    Base = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_RING_BASE),
                    Center = center
                },
                Pinky = new Finger
                {
                    Tip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_PINKY_TIP),
                    BelowTip = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_PINKY_JT2),
                    AboveBase = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_PINKY_JT1),
                    Base = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_PINKY_BASE),
                    Center = center
                }
            };
            this.hand = h;

            Console.WriteLine(this.hand.CountFingers()); 
        }

        private static Point GetPointFromJoint(PXCMHandData.IHand currentHandData, PXCMHandData.JointType jointType)
        {
            PXCMHandData.JointData jointData;
            currentHandData.QueryTrackedJoint(jointType, out jointData);
            var positionImage = jointData.positionImage;
            int x = (int)(WIDTH - positionImage.x);
            int y = (int)positionImage.y;
            return new Point
            {
                X = x,
                Y = y
            };
        }

        private void alertForEveryHand()
        {
            int numberOfHands = this._handData.QueryNumberOfHands();
            if (numberOfHands < 1) return;
            PXCMHandData.IHand currentHandData;
            PXCMHandData.JointData indexFingerJointData;
            this._handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, 0, out currentHandData);
            currentHandData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_INDEX_TIP, out indexFingerJointData);
            var positionImage = indexFingerJointData.positionImage;
            //Console.WriteLine("{0} {1}", positionImage.x, positionImage.y);

            int x = (int)(((WIDTH - positionImage.x) / WIDTH) * System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
            int y = (int)((positionImage.y / HEIGHT) * System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
        }

        private void onFiredGesture(PXCMHandData.GestureData gestureData)
        {
            if (this.hand == null) return;

            PXCMHandData.IHand currentHandData;
            //Console.WriteLine(gestureData.name + ": " + gestureData.state);
            this._handData.QueryHandDataById(gestureData.handId, out currentHandData);
            if (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_START)
            {
                if (gestureData.name == "spreadfingers" && this.status == Status.none)
                {
                    Console.Beep();
                    this.status = Status.afterSpreadfingers;
                    this.lastLocation = this.hand.Middle.Tip;
                }
            }
        }

        private void onFiredAlert(PXCMHandData.AlertData alertData)
        {
            if (alertData.label == PXCMHandData.AlertType.ALERT_HAND_TRACKED)
            {
                //Console.WriteLine("STARTED");
            };
        }

        ~Camera()
        {
            this._mngr.Dispose();
            this._session.Dispose();
        }
    }
}
