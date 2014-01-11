using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using WeifenLuo.WinFormsUI.Docking;

namespace VolleybalCompetition_creator
{
    public partial class ConstraintListView : DockContent
    {
        Klvv klvv;
        GlobalState state;
        public ConstraintListView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.constraints);
            klvv.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            objectListView1.BuildList(true);
        }
    }
}
