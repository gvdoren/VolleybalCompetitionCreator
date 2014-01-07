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
    public partial class ClubTreeView : DockContent
    {
        Klvv klvv = null;
        public ClubTreeView(Klvv klvv)
        {
            this.klvv = klvv;
            InitializeComponent();
            this.treeListView1.CanExpandGetter = delegate(object x) { return (x is Club); };
            this.treeListView1.ChildrenGetter = delegate(object x)
            {
                Club club = (Club)x;
                return club.teams;
            };
            treeListView1.SetObjects(klvv.clubs);
        }
    }
}
