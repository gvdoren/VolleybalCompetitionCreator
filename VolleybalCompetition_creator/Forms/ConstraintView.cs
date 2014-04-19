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
        GlobalState state = null;
        public ConstraintView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            Show("dummy", null);
            klvv.OnMyChange += state_OnMyChange;
            objectListView1.ShowGroups = false;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv) ;
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
            try
            {
                lock (klvv) ;
                if (e.TabPage == tabPage1) UpdateTabPage1();
                if (e.TabPage == tabPage2) UpdateTabPage2();
            }
            catch { }
        }
        private void UpdateTabPage1()
        {
            richTextBox1.Clear();
            if (constraint != null)
            {
                this.Text = constraint.Title;
                foreach (string str in constraint.GetTextDescription())
                {
                    richTextBox1.AppendText(str + Environment.NewLine);
                }
            }
        }
        private void UpdateTabPage2()
        {
            objectListView1.ClearObjects();
            label1.Text = "";
            this.Text = "No conflicts selected";
            if (constraint != null)
            {
                objectListView1.Objects = constraint.conflictMatches;
                this.Text = constraint.Title;
                label1.Text = "Conflict wedstrijden (" + constraint.conflictMatches.Count.ToString() + ")";
                objectListView1.BuildList(false);
                richTextBox2.Clear();
                foreach (string str in constraint.GetTextDescription())
                {
                    richTextBox2.AppendText(str + Environment.NewLine);
                }
            }
        }

        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Match match = objectListView1.GetModelObject(hit.Item.Index) as Match;
                if (match != null && match.poule != null)
                {
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        PouleView pouleview = content as PouleView;
                        if (pouleview != null)
                        {
                            if (match.poule == pouleview.poule)
                            {
                                pouleview.Activate();
                                return;
                            }
                        }
                    }
                    PouleView pouleView = new PouleView(klvv, state, match.poule);
                    pouleView.Show(this.DockPanel);
                }
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
