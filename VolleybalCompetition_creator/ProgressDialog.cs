using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VolleybalCompetition_creator
{
    public interface IProgress
    {
        bool Progress(int current, int total);
        bool Cancelled();
        void SetText(string str);
    }
    public partial class ProgressDialog : Form, IProgress
    {
        BackgroundWorker bw = new BackgroundWorker();
        public event MyEventHandler WorkFunction;
        public event MyEventHandler CompletionFunction;
        private MyEventArgs args;
        public ProgressDialog()
        {
            InitializeComponent();
            ControlBox = false;
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }
        public void ProgressBar(int i)
        {
            progressBar1.Value = i;
        }
        public void Start(string text,MyEventArgs args)
        {
            Text = text;
            if (bw.IsBusy != true)
            {
                this.args = args;
                bw.RunWorkerAsync();
            }
            else if (bw.WorkerSupportsCancellation == true)
            {
                bw.CancelAsync();
            }
            ShowDialog();
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
                return;
            }
            else
            {
                WorkFunction(this, this.args);
            }
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
            }
            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
            }
            else
            {
                //this.tbProgress.Text = "Done!";
            }
            this.Close();
            CompletionFunction(this, this.args);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar(e.ProgressPercentage);
        }
        public bool Progress(int current, int total)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => Progress(current,total)));
                return Cancelled();
            }
            ProgressBar((current * 100) / total);
            return Cancelled();
        }
        public bool Cancelled()
        {
            return (bw.CancellationPending == true);
        }
        public void SetText(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => SetText(str)));
                return;
            }
            Text = str;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
        }

    }
}
