using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CompetitionCreator
{
    public partial class ErrorView : DockContent
    {
        Model model = null;
        GlobalState state = null;
        public ErrorView(Model model, GlobalState state)
        {
            this.model = model;
            this.state = state;
            InitializeComponent();
            model.OnMyChange += state_OnMyChange;
            GlobalState.OnMyChange += state_OnMyChange;
            ShowErrors();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                ShowErrors();
            }
        }
        private List<Error> GetTotalErrors()
        {
            List<Error> totErrors = new List<Error>(GlobalState.errors);
            foreach (var con in model.constraints)
            {
                totErrors.AddRange(con.GetErrors());
            }
            return totErrors;
        }
        private void buttonClearErrors_Click(object sender, EventArgs e)
        {
            GlobalState.ClearManualClearableErrors();
        }
        private void ShowErrors()
        {
            List<Error> errors = GetTotalErrors();
            string html = "<div style='font-family: Arial, Helvetica, sans-serif;'><font size='2'> ";
            foreach(Error e in errors)
            {
                html += GenerateHtml(e) ;
            }
            html += "</div>";
            errorBrowser.DocumentText = html;
        }
        private string GenerateHtml(Error error)
        {
            string html = "<h4>Error</h4><div>";
            html += "<b>"+ error.text + "</b>";
            html += "</div>";
            string detail = error.GetDetail();
            if (detail != null)
            {
                html += "<h4>Detailed info</h4><div> ";
                if (error.HasTime)
                    html += "TimeStamp: " + error.GetTime() + "<br/><br/>"; 
                html += detail;
                html += "</div>";
            }
            string help = error.GetHelp();
            if (help != null)
            {
                html += "<h4>Help</h4><div>";
                html += help;
                html += "</div>";
            }
            html += "<hr/>";
            return html;
        }

        private void ErrorView_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalState.OnMyChange -= new MyEventHandler(state_OnMyChange);
            model.OnMyChange -= state_OnMyChange;
        }
    }
}
