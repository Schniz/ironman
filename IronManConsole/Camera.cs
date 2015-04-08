using System;
using System.Collections.Concurrent;
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

        private Win32framework win;

        private MedianStuff cursorMedian = new MedianStuff();

        private PXCMSession _session;
        private PXCMSenseManager _mngr;
        private PXCMSenseManager.Handler _handler;

        private Dictionary<int, Hand> hands;
        private ConcurrentDictionary<int, Hand> waitingHands;

        private PXCMHandModule _hand;
        private PXCMHandData _handData;

        private PXCMHandConfiguration.OnFiredGestureDelegate[] delegates;
        private PXCMHandData.AlertData lastAlertData;

        public Camera(params PXCMHandConfiguration.OnFiredGestureDelegate[] dlgts)
        {
            this.hands = new Dictionary<int, Hand>();
            this.waitingHands = new ConcurrentDictionary<int, Hand>();
            this.win = new Win32framework();
            this.delegates = dlgts;

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
            conf.EnableGesture("swipe_up", false);
            conf.EnableGesture("swipe_down", false);
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
                this.updateHands();


            }

            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void updateHands()
        {
            int numberOfHands = this._handData.QueryNumberOfHands();

            this.hands.Clear();

            for (int handIndex = 0; handIndex < numberOfHands; handIndex++)
            {
                PXCMHandData.IHand currentHandData;
                PXCMHandData.GestureData currentGestureData;

                this._handData.QueryFiredGestureData(0, out currentGestureData);

                this._handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, handIndex, out currentHandData);

                //currentHandData.QueryBodySide();

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
                    },
                    Status = Status.none
                };

                this.hands.Add(currentHandData.QueryUniqueId(), h);
            }

            foreach (var handId in this.waitingHands.Keys)
            {
                if (this.hands.ContainsKey(handId))
                {
                    var currentHand = this.hands[handId];
                    var waitingHand = this.waitingHands[handId];

                    currentHand.Status = waitingHand.Status;

                    if (waitingHand.Status == Status.afterSpreadfingers)
                    {
                        var newX = currentHand.Middle.Tip.X;
                        var oldX = waitingHand.Middle.Tip.X;
                        

                        if (Math.Abs(newX - oldX) > 100)
                        {
                            if (newX < oldX)
                            {
                                Console.WriteLine("Left");
                                this.win.KeyLeft();
                            }
                            else
                            {
                                Console.WriteLine("Right");
                                this.win.KeyRight();
                            }

                            Hand x;
                            this.waitingHands.TryRemove(handId, out x);
                        }
                    }
                }
                else
                {
                    Hand x;
                    this.waitingHands.TryRemove(handId, out x);
                }
            }

            //Console.WriteLine(this.hand.CountFingers()); 
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


        private void onFiredGesture(PXCMHandData.GestureData gestureData)
        {
            PXCMHandData.IHand currentHandData;
            //Console.WriteLine(gestureData.name + ": " + gestureData.state);

            if (!this.hands.ContainsKey(gestureData.handId)) return;
            var hand = this.hands[gestureData.handId];

            this._handData.QueryHandDataById(gestureData.handId, out currentHandData);
            if (gestureData.state == PXCMHandData.GestureStateType.GESTURE_STATE_START)
            {
                if (gestureData.name == "spreadfingers")
                {
                    if (hand.Status == Status.none)
                    {
                        Console.Beep();
                        hand.Status = Status.afterSpreadfingers;
                        this.waitingHands.TryAdd(gestureData.handId, hand);
                    }
                }
                else if (waitingHands.ContainsKey(gestureData.handId))
                {
                    var waitingHand = this.waitingHands[gestureData.handId];
                    if (waitingHand.Status != Status.none)
                    {
                        switch (gestureData.name)
                        {
                            case "swipe_up":

                                //this.win.KeyUp();
                                Console.WriteLine("Up");
                                break;
                            case "swipe_down":
                                // this.win.KeyDown();
                                Console.WriteLine("Down");
                                break;
                            default:
                                break;
                        }

                        Hand x;
                        this.waitingHands.TryRemove(gestureData.handId,out x);
                    }

                    hand.Status = Status.none;
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
