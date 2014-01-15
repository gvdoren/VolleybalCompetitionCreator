using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using BrightIdeasSoftware;

namespace VolleybalCompetition_creator
{
    public partial class SerieView : DockContent
    {
        Klvv klvv = null;
        SerieFilter serieFilter;
        GlobalState state;
        public SerieView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            this.serieFilter = new SerieFilter(klvv,state);
            InitializeComponent();
            objectListView1.SetObjects(klvv.series.Values);
            objectListView1.ModelFilter = serieFilter;
            objectListView1.UseFiltering = true;
            state.OnMyChange += new MyEventHandler(state_OnMyChange); 

        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            objectListView1.BuildList(true);
        }
        public class SerieFilter: IModelFilter 
        {
            Klvv klvv = null;
            GlobalState state = null;
            public SerieFilter(Klvv klvv,GlobalState state)
            {
                this.klvv = klvv;
                this.state = state;
            }
            public bool Filter(object modelObject)
            {
                Serie serie = (Serie)modelObject;
                foreach (Club club in state.selectedClubs)
                {
                    if (club.series.Contains(serie)) return true;
                }
                
                return false;
            } 
        }


    
    }
}
