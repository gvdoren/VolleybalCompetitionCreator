using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public partial class ConstraintView : DockContent
    {
        Constraint constraint = null;
        Klvv klvv = null;
        public ConstraintView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            InitializeComponent();
            Show("dummy", null);
            klvv.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            int i = this.tabControl1.SelectedIndex;
            this.tabControl1.SelectTab(0);
            this.tabControl1.SelectTab(i);
        }
        public void Show(string tabName, Constraint constraint)
        {
            this.tabControl1.SelectTab(0);
            this.constraint = constraint;
            TabPage page = tabControl1.TabPages[0];
            foreach (TabPage p in tabControl1.TabPages)
            {
                if (p.Text == tabName) page = p;
            }
            this.tabControl1.SelectTab(page);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPage1) UpdateTabPage1();
        }
        private void UpdateTabPage1()
        {
            richTextBox1.Clear();
            this.Text = constraint.name + " - " + constraint.club.name;
            foreach (string str in constraint.GetTextDescription())
            {
                richTextBox1.AppendText(str+Environment.NewLine);
            }
        }


    }
}
class TablessTabControl : TabControl
{
    protected override void WndProc(ref Message m)
    {
        // Hide tabs by trapping the TCM_ADJUSTRECT message
        if (m.Msg == 0x1328 && !DesignMode) m.Result = (IntPtr)1;
        else base.WndProc(ref m);
    }
}
