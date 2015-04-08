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
        public Main()
        {
            InitializeComponent();

            //this.ShowInTaskbar = false;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.TopMost = true;
        }

        public void ChangeStatus(Status status)
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

        }
    }
}
