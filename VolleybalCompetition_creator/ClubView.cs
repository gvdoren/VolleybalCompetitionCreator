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
            klvv.OnMyChange += state_OnMyChange;
            
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            objectListView1.BuildList(true);
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
        private void objectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = objectListView1.HitTest(e.Location);
            if (hit.Item != null)
            {
                Club club = objectListView1.GetModelObject(hit.Item.Index) as Club;
                if (club != null)
                {
                    // check whether the PouleView is already existing
                    foreach (DockContent content in this.DockPanel.Contents)
                    {
                        PouleListView poulelistview = content as PouleListView;
                        if (poulelistview != null)
                        {
                            poulelistview.Activate();
                            state.selectedClubs.Clear();
                            foreach (Object obj in objectListView1.SelectedObjects)
                            {
                                Club club1 = (Club)obj;
                                state.selectedClubs.Add(club1);
                            }
                            state.Changed();
                            return;
                        }
                    }
                    PouleListView poulelistView = new PouleListView(klvv, state);
                    poulelistView.Show(this.DockPanel);
                }
            };
        }
    }
}
