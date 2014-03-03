using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cranium.Lobe.Worker.Gui.Properties;

namespace Cranium.Lobe.Worker.Gui
{
    public partial class Start : Form
    {
        private Worker _Worker;
        public Start()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (_Worker == null)
            {
                StartButton.Enabled = false;
                Settings workerSettings = new Settings();
                workerSettings.CommsManagerIP = ManangerIP.Text;
                int port;
                if (!int.TryParse(ManagerPort.Text, out port))
                {
                    MessageBox.Show("Manager Port Invalid");
                    return;
                }
                workerSettings.CommsManagerPort = port;
                workerSettings.WorkBufferCount = 1;
                workerSettings.WorkerThreadCount = (int) ThreadCount.Value;
                workerSettings.PendingWorkDirectory = "PendingWork";
                workerSettings.CompletedWorkDirectory = "CompletedWork";
                _Worker = new Worker();
                _Worker.HandelMessage += PushMessage;
                new Task(() => _Worker.Start(workerSettings)).Start();
                StartButton.Text = "Stop";
                StartButton.Enabled = true;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                StartButton.Enabled = false;
                Close();
            }
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
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "I'm hiding down here if you need me";
                notifyIcon1.ShowBalloonTip(400);
                Hide();
            }

            else if (FormWindowState.Normal == WindowState)
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
