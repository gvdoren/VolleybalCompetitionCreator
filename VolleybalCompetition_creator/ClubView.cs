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
    public partial class ClubView : DockContent
    {
        Klvv klvv = null;
        GlobalState state;
        public ClubView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            InitializeComponent();
            objectListView1.SetObjects(klvv.clubs);
            
        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            state.selectedClubs.Clear();
            foreach (Object obj in objectListView1.SelectedObjects)
            {
                Club club = (Club)obj;
                state.selectedClubs.Add(club);
            }
            state.Changed();
        }
    }
}
