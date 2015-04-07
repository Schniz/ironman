using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    public class Camera
    {
        const int WIDTH = 640;
        const int HEIGHT = 480;

        private PXCMSession _session;
        private PXCMSenseManager _mngr;
        private PXCMSenseManager.Handler _handler;

        private PXCMHandModule _hand;
        private PXCMHandData _handData;

        private PXCMHandConfiguration.OnFiredGestureDelegate[] delegates;
        private PXCMHandData.AlertData lastAlertData;

        public Camera(params PXCMHandConfiguration.OnFiredGestureDelegate[] dlgts)
        {
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
            conf.EnableGesture("thumb_up", false);
            conf.EnableGesture("swipe_left", false);
            conf.EnableGesture("swipe_up", false);
            conf.EnableGesture("swipe_down", false);
            conf.EnableGesture("swipe_right", false);

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
                this.alertForEveryHand();
            }

            return pxcmStatus.PXCM_STATUS_NO_ERROR;
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
            Console.WriteLine("{0} {1}", positionImage.x, positionImage.y);
        }

        private void onFiredGesture(PXCMHandData.GestureData gestureData)
        {
            Console.WriteLine(gestureData.name + ": " + gestureData.state);
        }

        private void onFiredAlert(PXCMHandData.AlertData alertData)
        {
            // Console.WriteLine("ALERT: " + alertData.label.ToString());
        }

        ~Camera()
        {
            this._mngr.Dispose();
            this._session.Dispose();
        }
    }
}
