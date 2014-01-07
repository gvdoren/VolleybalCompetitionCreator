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
    public partial class Form1 : Form
    {
        Klvv klvv = new Klvv();
        GlobalState state = new GlobalState();
        public Form1()
        {
            InitializeComponent();
            
            ClubView clubview = new ClubView(klvv,state);
            clubview.ShowHint = DockState.DockLeft;
            clubview.Show(dockPanel);

            SerieView serieview = new SerieView(klvv, state);
            serieview.ShowHint = DockState.DockRight;
            serieview.Show(dockPanel);
            SerieTreeView serietreeview = new SerieTreeView(klvv, state);
            serietreeview.ShowHint = DockState.DockRight;
            serietreeview.Show(dockPanel);
            PouleListView pouleListview = new PouleListView(klvv, state);
            pouleListview.ShowHint = DockState.DockRight;
            pouleListview.Show(dockPanel);

        }
    }
}
