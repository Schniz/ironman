using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace IronManConsole
{
    public enum Status
    {
        none,
        afterSpreadfingers,
        pinch
    }
    public class Camera
    {
        const int WIDTH = 640;
        const int HEIGHT = 480;

        private System.Timers.Timer directionTimer;
        private System.Timers.Timer pinchTimer;

        private Action action;

        private MedianStuff cursorMedian = new MedianStuff();

        private PXCMSession _session;
        private PXCMSenseManager _mngr;
        private PXCMSenseManager.Handler _handler;

        private Hand rightHand;
        private Hand leftHand;

        private Status status;

        private Point lastRightLocation;
        private Point lastLeftLocation;

        private PXCMHandModule _hand;
        private PXCMHandData _handData;

        private PXCMHandConfiguration.OnFiredGestureDelegate[] delegates;

        public Camera(params PXCMHandConfiguration.OnFiredGestureDelegate[] dlgts)
        {
            this.action = new Action();
            this.delegates = dlgts;

            directionTimer = new System.Timers.Timer(1000);
            directionTimer.Elapsed += directionTimer_Elapsed;

            pinchTimer = new System.Timers.Timer(500);
            pinchTimer.Elapsed += pinchTimer_Elapsed;

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
            //conf.EnableGesture("spreadfingers", false);
            conf.EnableGesture("two_fingers_pinch_open", true);

            // Subscribe hands alerts
            conf.EnableAllAlerts();
            //conf.SubscribeAlert(this.onFiredAlert);

            conf.EnableTrackedJoints(true);

            // and the private one for debug
            //conf.SubscribeGesture(this.onFiredGesture);

            // Apply it all
            conf.ApplyChanges();

            // Set events
            this._handler = new PXCMSenseManager.Handler();
            this._handler.onModuleProcessedFrame = this.onModuleProcessedFrame;

            this._mngr.Init(this._handler);
        }

        void pinchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("pinch timer");
            this.status = Status.none;
            this.pinchTimer.Stop();
        }

        void directionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("direction timer");

            this.status = Status.none;
            this.directionTimer.Stop();
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
                this.updateHands();
            }

            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void updateHands()
        {
            int numberOfHands = this._handData.QueryNumberOfHands();

            this.leftHand = null;
            this.rightHand = null;

            for (int handIndex = 0; handIndex < numberOfHands; handIndex++)
            {
                PXCMHandData.IHand currentHandData;
                PXCMHandData.GestureData currentGestureData;

                this._handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, handIndex, out currentHandData);

                var side = currentHandData.QueryBodySide();

                Hand hand = null;

                if (side == PXCMHandData.BodySideType.BODY_SIDE_RIGHT)
                {
                    this.rightHand = GetHandData(currentHandData);
                    hand = this.rightHand;
                }
                else if (side == PXCMHandData.BodySideType.BODY_SIDE_LEFT)
                {
                    this.leftHand = GetHandData(currentHandData);
                    hand = this.leftHand;
                }
                else
                {
                    return;
                }

                this._handData.QueryFiredGestureData(handIndex, out currentGestureData);

                if (currentGestureData != null && currentGestureData.name != "")
                {
                    hand.Gestrue = currentGestureData.name;
                }
            }

            if (this.rightHand != null && this.leftHand != null)
            {
                if (this.rightHand.Gestrue == "two_fingers_pinch_open" && this.leftHand.Gestrue == "two_fingers_pinch_open")
                {
                    if (this.status == Status.none)
                    {
                        this.status = Status.pinch;
                        this.pinchTimer.Start();

                        this.lastLeftLocation = this.leftHand.Index.Tip;
                        this.lastRightLocation = this.rightHand.Index.Tip;

                    }
                    else if (this.status == Status.pinch)
                    {
                        this.pinchTimer.Stop();
                        this.pinchTimer.Start();
                    }

                }

                if (this.status == Status.pinch)
                {
                    var oldDistance = Math.Sqrt(Math.Pow(this.lastLeftLocation.X - this.lastRightLocation.X, 2) + Math.Pow(this.lastLeftLocation.Y - this.lastRightLocation.Y, 2));
                    var newDistance = Math.Sqrt(Math.Pow(this.leftHand.Index.Tip.X - this.rightHand.Index.Tip.X, 2) + Math.Pow(this.leftHand.Index.Tip.Y - this.rightHand.Index.Tip.Y, 2));

                    var oldAverage = new Point
                    {
                        X = (this.lastLeftLocation.X + this.lastRightLocation.X) / 2,
                        Y = (this.lastLeftLocation.Y + this.lastRightLocation.Y) / 2
                    };

                    var newAverage = new Point
                    {
                        X = (this.leftHand.Index.Tip.X + this.rightHand.Index.Tip.X) / 2,
                        Y = (this.leftHand.Index.Tip.Y + this.rightHand.Index.Tip.Y) / 2
                    };

                    this.lastLeftLocation = this.leftHand.Index.Tip;
                    this.lastRightLocation = this.rightHand.Index.Tip;

                    this.action.Pinch((int)(newDistance - oldDistance), new Point
                    {
                        X = 0,
                        Y = 0
                    });

                    Console.WriteLine("diff:" + (oldDistance - newDistance).ToString());
                }
            }

            if (this.rightHand != null)
            {
                if (this.status == Status.afterSpreadfingers)
                {
                    if (CalculateDistances(this.rightHand.Middle.Tip, this.lastRightLocation))
                    {
                        this.directionTimer.Stop();
                        this.status = Status.none;
                        Thread.Sleep(500);
                    }
                }
                else if (this.rightHand.z < 0.4 && this.rightHand.Middle.Tip.X > 200 && this.rightHand.Middle.Tip.X < 450 && this.rightHand.Middle.Tip.Y > 150 && this.rightHand.Middle.Tip.Y < 300)
                {
                    if (this.rightHand.CountFingers() == 4 && this.status == Status.none)
                    {
                        this.status = Status.afterSpreadfingers;
                        this.lastRightLocation = this.rightHand.Middle.Tip;
                        Console.Beep(880, 300);
                        this.directionTimer.Start();
                    }
                }
            }

            if (numberOfHands > 0)
            {

                //    Console.WriteLine("L:{0} \t\t R:{1}",
                //            this.leftHand != null ? this.leftHand.Middle.Tip.ToString() + " - " + this.leftHand.CountFingers() : "\t\t",
                //            this.rightHand != null ? this.rightHand.Middle.Tip.ToString() + " - " + this.rightHand.CountFingers() + " z:" + this.rightHand.z : "\t\t");


                //Console.WriteLine("L:{0} \t\t R:{1}",
                //        this.leftHand != null ? this.leftHand.Gestrue : "\t\t",
                //        this.rightHand != null ? this.rightHand.Gestrue : "\t\t");
            }
        }

        private Hand GetHandData(PXCMHandData.IHand currentHandData)
        {
            PXCMHandData.JointData jointData;
            currentHandData.QueryTrackedJoint(PXCMHandData.JointType.JOINT_CENTER, out jointData);
            var positionImage = jointData.positionWorld;
            float z = positionImage.z;

            var center = GetPointFromJoint(currentHandData, PXCMHandData.JointType.JOINT_CENTER);
            Hand hand = new Hand
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
                },
                z = z
            };

            return hand;
        }

        private bool CalculateDistances(Point newLoc, Point oldLoc)
        {
            var diffX = Math.Abs(newLoc.X - oldLoc.X) + 70;
            var diffY = Math.Abs(newLoc.Y - oldLoc.Y);

            if (diffX > 220 && diffX > diffY)
            {
                if (newLoc.X < oldLoc.X)
                {
                    Console.WriteLine("Left");
                    this.action.Left();
                }
                else
                {
                    Console.WriteLine("Right");
                    this.action.Right();
                }

                return true;

            }
            else if (diffY > 100 && diffY >= diffX)
            {
                if (newLoc.Y < oldLoc.Y)
                {
                    Console.WriteLine("Up");
                    this.action.Up();
                }
                else
                {
                    Console.WriteLine("Down");
                    this.action.Down();
                }

                return true;
            }

            return false;
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

        ~Camera()
        {
            this._mngr.Dispose();
            this._session.Dispose();
        }
    }
}
