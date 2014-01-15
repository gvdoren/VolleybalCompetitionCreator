using System;
using System.Collections.Generic;
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
    public partial class SerieTreeView : DockContent
    {
        Klvv klvv = null;
        SerieFilter serieFilter;
        GlobalState state;
       public SerieTreeView(Klvv klvv, GlobalState state)
        {
            this.klvv = klvv;
            this.state = state;
            this.serieFilter = new SerieFilter(klvv,state);
            InitializeComponent();
            treeListView1.SetObjects(klvv.series.Values);
            treeListView1.ModelFilter = serieFilter;
            treeListView1.UseFiltering = true;
            state.OnMyChange += new MyEventHandler(state_OnMyChange);
            this.treeListView1.CanExpandGetter = delegate(object x) { return (x is Serie); };
            this.treeListView1.ChildrenGetter = delegate(object x)
            {
                Serie serie = (Serie)x;
                return serie.poules.Values;
            };
            treeListView1.ExpandAll();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            treeListView1.UseFiltering = false; 
            treeListView1.UseFiltering = true;
            treeListView1.ExpandAll();
            //treeListView1.BuildList(false);
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
                Serie serie = modelObject as Serie;
                if (serie != null)
                {
                    if (state.selectedClubs.Count == 0) return true;
                    foreach (Club club in state.selectedClubs)
                    {
                        if (club.series.Contains(serie)) return true;
                    }
                }
                else 
                {
                    Poule poule = modelObject as Poule;
                    if (poule != null)
                    {
                        if (state.selectedClubs.Count == 0) return true;
                        foreach (Match match in poule.matches)
                        {
                            if (match.homeTeam != null && state.selectedClubs.Contains(match.homeTeam.club))
                            {
                                return true;
                            }
                        }
                        return false;
                        
                    }
                }
                return false;
            } 
        }


    
    }
}
