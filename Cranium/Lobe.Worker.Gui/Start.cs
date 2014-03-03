using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cranium.Lobe.Worker;

namespace Lobe.Worker.Gui
{
    public partial class Start : Form
    {
        private Cranium.Lobe.Worker.Worker _Worker;
        public Start()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            Settings WorkerSettings = new Settings();
            WorkerSettings.CommsManagerIP = ManangerIP.Text;
            int port;
            if (!int.TryParse(ManagerPort.Text, out port))
            {
                MessageBox.Show("Manager Port Invalid");
                return;
            }
            WorkerSettings.CommsManagerPort = port;
            WorkerSettings.WorkBufferCount = 1;
            WorkerSettings.WorkerThreadCount = (int)ThreadCount.Value;
            WorkerSettings.PendingWorkDirectory = "PendingWork";
            WorkerSettings.CompletedWorkDirectory = "CompletedWork";
            _Worker = new Cranium.Lobe.Worker.Worker();
            _Worker.HandelMessage += PushMessage;
            new Task(()=>_Worker.Start(WorkerSettings)).Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            ThreadCount.Maximum = Environment.ProcessorCount;
            ThreadCount.Minimum = 0;
            ThreadCount.Value = Environment.ProcessorCount;
        }

        public void PushMessage(string message)
        {
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = message;
            notifyIcon1.ShowBalloonTip(300);
        }

        private void Start_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "I'm hiding down here if you need me";
                notifyIcon1.ShowBalloonTip(400);
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void Start_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Worker != null)
            {
                _Worker.Stop();
                _Worker.HandelMessage -= PushMessage;
            }
        }


    }
}
