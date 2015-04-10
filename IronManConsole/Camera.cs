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
        pinch,
        rock
    }
    public class StatusEventArgs : EventArgs
    {
        public Status status;
    }
    public class Camera
    {
        public const int WIDTH = 640;
        public const int HEIGHT = 480;
        public const int MAX_ROCK_FRAMES = 20;

        private System.Timers.Timer directionTimer;
        private System.Timers.Timer pinchTimer;
        private System.Timers.Timer rockTimer;

        private Action action;


        private PXCMSession _session;
        private PXCMSenseManager _mngr;
        private PXCMSenseManager.Handler _handler;

        private Hand rightHand;
        private Hand leftHand;

        private int rockCounter;
        private int pinchCounter;

        private Status _status;

        public event EventHandler<StatusEventArgs> StatusChanged;

        public Status status
        {
            get { return _status; }
            set { 
                _status = value;
                OnStatusChanged(new StatusEventArgs { status = _status });
            }
        }


        private Point lastRightLocation;
        private Point lastLeftLocation;

        private PXCMHandModule _hand;
        private PXCMHandData _handData;

        public Camera()
        {
            
            this.action = new Action();

            this.rockCounter = 0;

            directionTimer = new System.Timers.Timer(1000);
            directionTimer.Elapsed += directionTimer_Elapsed;

            pinchTimer = new System.Timers.Timer(500);
            pinchTimer.Elapsed += pinchTimer_Elapsed;

            rockTimer = new System.Timers.Timer(300);
            rockTimer.Elapsed += rockTimer_Elapsed;

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

        void rockTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.rockCounter = 0;
            if (this.status == Status.rock)
            {
                Console.WriteLine("rock timer");
                this.status = Status.none;
            }
            this.rockTimer.Stop();

        }

        void pinchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.pinchCounter = 0;
            Console.WriteLine("pinch timer");
            if (this.status == Status.pinch)
            {
                this.status = Status.none;
            }
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
                    this.rightHand = CameraUtils.GetHandFromData(currentHandData);
                    hand = this.rightHand;
                }
                else if (side == PXCMHandData.BodySideType.BODY_SIDE_LEFT)
                {
                    this.leftHand = CameraUtils.GetHandFromData(currentHandData);
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

                        this.lastLeftLocation = this.leftHand.Index.Center;
                        this.lastRightLocation = this.rightHand.Index.Center;

                    }
                    else if (this.status == Status.pinch)
                    {
                        this.pinchTimer.Stop();
                        this.pinchTimer.Start();
                    }
                }

                if (this.status == Status.pinch)
                {
                    if (this.pinchCounter < Action.PINCH_INTERVAL)
                    {
                        this.pinchCounter++;
                    }
                    else
                    {
                        this.pinchCounter = 0;

                        var oldDistance = Math.Sqrt(Math.Pow(this.lastLeftLocation.X - this.lastRightLocation.X, 2) + Math.Pow(this.lastLeftLocation.Y - this.lastRightLocation.Y, 2));
                        var newDistance = Math.Sqrt(Math.Pow(this.leftHand.Index.Center.X - this.rightHand.Index.Center.X, 2) + Math.Pow(this.leftHand.Index.Center.Y - this.rightHand.Index.Center.Y, 2));

                        var distance = (int)(newDistance - oldDistance);

                        this.lastLeftLocation = this.leftHand.Index.Center;
                        this.lastRightLocation = this.rightHand.Index.Center;

                        this.action.Pinch(new Point() { X = distance, Y = distance });

                        //var oldDelta = calculateDelta(this.lastLeftLocation, this.lastRightLocation);
                        //var newDelta = calculateDelta(this.leftHand.Index.Center, this.rightHand.Index.Center);

                        //this.action.Pinch(new Point
                        //{
                        //    X = newDelta.X - oldDelta.X,
                        //    Y = newDelta.Y - oldDelta.Y
                        //});
                    }


                    //Console.WriteLine("diff:" + (oldDistance - newDistance).ToString());
                }
            }

            if (this.rightHand != null)
            {
                if (this.rightHand.IsRock())
                {
                    if (this.status == Status.none)
                    {
                        if (rockCounter < MAX_ROCK_FRAMES)
                        {
                            rockCounter++;
                        }
                        else
                        {
                            this.status = Status.rock;
                            this.lastRightLocation = this.rightHand.Index.Center;
                        }
                    }

                    this.rockTimer.Stop();
                    this.rockTimer.Start();
                }

                if (this.status == Status.rock)
                {
                    var diff = this.lastRightLocation.Y - this.rightHand.Index.Center.Y;

                    if (Math.Abs(diff) > 5)
                    {
                        if (this.lastRightLocation.Y > this.rightHand.Index.Center.Y)
                        {
                            this.action.VolUp();
                        }
                        else
                        {
                            this.action.VolDown();
                        }
                    }

                    this.lastRightLocation = this.rightHand.Index.Center;
                }


                if (this.status == Status.afterSpreadfingers)
                {
                    if (CalculateDistances(this.rightHand.Middle.Center, this.lastRightLocation))
                    {
                        this.directionTimer.Stop();
                        this.status = Status.none;
                        Thread.Sleep(300);
                    }
                }
                else if (this.rightHand.z < 0.4 && this.rightHand.Middle.Center.X > 200 && this.rightHand.Middle.Center.X < 450 && this.rightHand.Middle.Center.Y > 150 && this.rightHand.Middle.Center.Y < 300)
                {
                    if (this.rightHand.CountFingers() == 4 && this.status == Status.none)
                    {
                        this.status = Status.afterSpreadfingers;
                        this.lastRightLocation = this.rightHand.Middle.Center;
                        Console.Beep(880, 300);
                        this.directionTimer.Start();
                    }
                }
            }

            if (numberOfHands > 0)
            {

                //    Console.WriteLine("L:{0} \t\t R:{1}",
                //            this.leftHand != null ? this.leftHand.Middle.Center.ToString() + " - " + this.leftHand.CountFingers() : "\t\t",
                //            this.rightHand != null ? this.rightHand.Middle.Center.ToString() + " - " + this.rightHand.CountFingers() + " z:" + this.rightHand.z : "\t\t");


                //Console.WriteLine("L:{0} \t\t R:{1}",
                //        this.leftHand != null ? this.leftHand.Gestrue : "\t\t",
                //        this.rightHand != null ? this.rightHand.Gestrue : "\t\t");
            }
        }

        private Point calculateDelta(Point left, Point right)
        {
            return new Point
            {
                X = Math.Abs(right.X - left.X),
                Y = Math.Abs(right.Y - left.Y)
            };
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

        ~Camera()
        {
            this._mngr.Dispose();
            this._session.Dispose();
        }

        protected virtual void OnStatusChanged(StatusEventArgs e)
        {
            EventHandler<StatusEventArgs> handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
