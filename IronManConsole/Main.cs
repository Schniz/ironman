using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IronManConsole
{
    public partial class Main : Form
    {
        private Camera cam;
        public Main()
        {
            InitializeComponent();
            cam = new Camera();
            cam.StatusChanged += cam_StatusChanged;
            cam.Start();
            this.ShowInTaskbar = false;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.TopMost = true;
        }

        void cam_StatusChanged(object sender, StatusEventArgs e)
        {
            this.ChangeStatus(e.status);
        }

        public void ChangeStatus(Status status)
        {
            this.BeginInvoke(new MethodInvoker(delegate
               {
                   switch (status)
            {
                case Status.afterSpreadfingers:
                    {
                        this.hand.Visible = true;
                        this.pinch.Visible = false;
                        break;
                    }
                case Status.pinch:
                    {
                        this.hand.Visible = false;
                        this.pinch.Visible = true;
                        break;
                    }
                case Status.none:
                    {
                        this.hand.Visible = false;
                        this.pinch.Visible = false;
                        break;
                    }
                case Status.rock:
                    {
                        this.hand.Visible = false;
                        this.pinch.Visible = false;
                        break;
                    }
                default:
                    {
                        break;
                    }
                   }
               }));
        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
    }
}
