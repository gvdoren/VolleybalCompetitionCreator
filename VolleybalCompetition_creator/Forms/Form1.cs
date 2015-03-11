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
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace VolleybalCompetition_creator
{
    public partial class Form1 : Form
    {
        Klvv klvv = null;
        GlobalState state = new GlobalState();
        string BaseDirectory = "";
            
        public Form1()
        {
            BaseDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+@"\CompetitionCreator";
            if (Directory.Exists(BaseDirectory) == false)
            {
                Directory.CreateDirectory(BaseDirectory);
            }
            //Weekend.Show();
            //Weekend.Test();
            InitializeComponent();
            // reading club-constraints
            klvv = new Klvv(DateTime.Now.Year);
            
            Text = string.Format("Volleyball competition creation tool ({0})", klvv.year);

            this.WindowState = FormWindowState.Maximized;
            ClubListView clubview = new ClubListView(klvv,state);
            clubview.ShowHint = DockState.DockLeft;
            clubview.Show(dockPanel);

            //SerieView serieview = new SerieView(klvv, state);
            //serieview.ShowHint = DockState.DockRight;
            //serieview.Show(dockPanel);
            //serieview.Show(clubview.Pane, DockAlignment.Right, 0.35);
            //SerieTreeView serietreeview = new SerieTreeView(klvv, state);
            //serietreeview.ShowHint = DockState.DockRight;
            //serietreeview.Show(dockPanel);
            
            PouleListView pouleListview = new PouleListView(klvv, state);
            pouleListview.ShowHint = DockState.DockLeft;
            pouleListview.Show(clubview.Pane, DockAlignment.Right, 0.5);
            //pouleListview.Show(dockPanel);
            ConstraintListView constraintListview = new ConstraintListView(klvv, state);
            constraintListview.ShowHint = DockState.DockRight;
            constraintListview.Show(dockPanel);
            dockPanel.DockRightPortion = 500;
            List<string> ListTeam = new List<string>();
            for (int i = 0; i < 11; i++)
            {
                ListTeam.Add(string.Format("Team {0}",i+1));
            }
            klvv.OnMyChange += state_OnMyChange;
            state.OnMyChange += state_OnMyChange;
            klvv.OpenLastProject();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.klvv != null)
            {
                klvv.OnMyChange -= state_OnMyChange;
                klvv = e.klvv;
                state.Clear();
                klvv.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (klvv)
            {
                string changed = klvv.stateNotSaved ? "*" : "";
                this.Text = string.Format("Volleyball competition creation tool ({0}{1})", klvv.savedFileName, changed);
            }
        }


        private void clubsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClubListView clubview = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubview = content as ClubListView;
                if (clubview != null)
                {
                    clubview.Activate();
                    return;
                }
            }
            clubview = new ClubListView(klvv, state);
            clubview.ShowHint = DockState.DockLeft;
            
            clubview.Show(this.dockPanel);

        }

        private void seriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PouleListView pouleListView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                pouleListView = content as PouleListView;
                if (pouleListView != null)
                {
                    pouleListView.Activate();
                    return;
                }
            }
            pouleListView = new PouleListView(klvv, state);
            pouleListView.ShowHint = DockState.DockLeft;
            pouleListView.Show(this.dockPanel);

        }

        private void lijstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClubListView clubview = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubview = content as ClubListView;
                if (clubview != null)
                {
                    clubview.Activate();
                    return;
                }
            }
            clubview = new ClubListView(klvv, state);
            clubview.ShowHint = DockState.DockLeft;

            clubview.Show(this.dockPanel);
        }

        private void optimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptimizeForm optimizeForm = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                optimizeForm = content as OptimizeForm;
                if (optimizeForm != null)
                {
                    optimizeForm.Activate();
                    return;
                }
            }
            optimizeForm = new OptimizeForm(klvv, state);
            optimizeForm.Show(this.dockPanel);

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void constraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConstraintListView constraintView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                constraintView = content as ConstraintListView;
                if (constraintView != null)
                {
                    constraintView.Activate();
                    return;
                }
            }
            constraintView = new ConstraintListView(klvv, state);
            constraintView.ShowHint = DockState.DockRight;
            constraintView.Show(this.dockPanel);
        }

        private void anoramaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AnoramaView anorama = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                anorama = content as AnoramaView;
                if (anorama != null)
                {
                    anorama.Activate();
                    return;
                }
            }
            anorama = new AnoramaView(klvv, state);
            anorama.Show(this.dockPanel);


        }

        private void poulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SerieView serieView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                serieView = content as SerieView;
                if (serieView != null)
                {
                    serieView.Activate();
                    return;
                }
            }
            serieView = new SerieView(klvv, state);
            serieView.Show(this.dockPanel);

        }

        private void subscriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InschrijvingenView clubConstraints = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                clubConstraints = content as InschrijvingenView;
                if (clubConstraints != null)
                {
                    clubConstraints.Activate();
                    return;
                }
            }
            clubConstraints = new InschrijvingenView(klvv, state);
            clubConstraints.Show(this.dockPanel);
        }

        private void perTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void perClubToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void perClubToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("FullCompetition{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk3);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk3(object sender, CancelEventArgs e)
        {
            klvv.WriteProject(saveFileDialog1.FileName);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "FullCompetition.xml";
            openFileDialog1.Filter = "Xml (*.xml)|*.xml";
            openFileDialog1.InitialDirectory = BaseDirectory;
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk1);
            openFileDialog1.ShowDialog();

        }
        public void openFileDialog1_FileOk1(object sender, CancelEventArgs e)
        {
            klvv.LoadFullCompetition(openFileDialog1.FileName);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Creeer een nieuw seizoen", "Geef een jaartal");
            form.ShowDialog();
            if (form.Result)
            {
                int year;
                bool ok = int.TryParse(form.GetInputString(), out year);
                if (ok)
                {
                    Klvv newKlvv = new Klvv(year);
                    klvv.Changed(newKlvv);
                    newKlvv.Evaluate(null);
                    newKlvv.Changed();
                }
            }

        }
        private void teamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamListView teamListView = null;
            foreach (DockContent content in this.dockPanel.Contents)
            {
                teamListView = content as TeamListView;
                if (teamListView != null)
                {
                    teamListView.Activate();
                    return;
                }
            }
            teamListView = new TeamListView(klvv, state);
            teamListView.ShowHint = DockState.DockLeft;
            teamListView.Show(this.dockPanel);

        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            klvv.WriteProject(klvv.savedFileName);
        }

        private void kLVVTeamSubscriptionsklvvbeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Input the URL of the website that has the registrations", "URL:", MySettings.Settings.RegistrationsXML);
            form.ShowDialog();
            if(form.Result == true)
            {
                klvv.ImportTeamSubscriptions(XDocument.Load(form.GetInputString()).Root);
            }
        }

        private void clubRegistrationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            klvv.WriteClubConstraints(MyDocuments + "\\ClubRegistrations.xml");
        }

        private void competitionxmlForKlvvsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ExportToKlvv{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk4);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk4(object sender, CancelEventArgs e)
        {
            List<Selection> selection = new List<Selection>();
            foreach (Serie serie in klvv.series)
            {
                Selection sel = new Selection(serie.name);
                sel.obj = serie;
                sel.selected = (serie.imported == false);
                selection.Add(sel);
            }

            SelectionDialog diag = new SelectionDialog(selection,true);
            diag.ShowDialog();
            List<Serie> series = new List<Serie>();
            if (diag.Ok)
            {
                foreach (Selection sel in diag.Selections)
                {
                    if (sel.selected) series.Add((Serie)sel.obj);
                }

                klvv.WriteExportToKLVVXml(saveFileDialog1.FileName, series);
            }
        }

        private void poulesSeriescsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void matchescsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void statisticscsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void vVBConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ImportedVVBCompetition{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk9);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk9(object sender, CancelEventArgs e)
        {
            klvv.ConvertVVBCompetitionToCSV(saveFileDialog1.FileName);
        }

        private void cSVCompetitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "ImportedCompetition.csv";
            openFileDialog1.Filter = "CSV (*.csv)|*.csv";
            openFileDialog1.InitialDirectory = BaseDirectory;
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk2);
            openFileDialog1.ShowDialog();
        }
        public void openFileDialog1_FileOk2(object sender, CancelEventArgs e)
        {
            klvv.ImportCSV(openFileDialog1.FileName);
        }
        private void subscriptionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "Registrations.xml";
            openFileDialog1.Filter = "Registrations (*.xml)|*.xml";
            openFileDialog1.InitialDirectory = BaseDirectory;
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk3);
            openFileDialog1.ShowDialog();
        }
        public void openFileDialog1_FileOk3(object sender, CancelEventArgs e)
        {
            klvv.ImportTeamSubscriptions(XDocument.Load(openFileDialog1.FileName).Root);
        }

        private void kLVVConvertToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ImportedKLVVCompetition{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk8);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk8(object sender, CancelEventArgs e)
        {
            klvv.ConvertKLVVCompetitionToCSV(saveFileDialog1.FileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (klvv.stateNotSaved)
            {
                DialogResult result = MessageBox.Show("Competition changed. Do you want to save the competition first?", "Closing application",
MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void clubRegistrationsKlvvsitexmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ExportToKlvv_registrations{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk10);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk10(object sender, CancelEventArgs e)
        {
            klvv.WriteClubConstraints(saveFileDialog1.FileName,false /* zonder nationale teams*/); 
        }

        private void importKLVVRankingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "Rankings.xml";
            openFileDialog1.Filter = "Rankings (*.xml)|*.xml";
            openFileDialog1.InitialDirectory = BaseDirectory;
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk5);
            openFileDialog1.ShowDialog();
        }
        public void openFileDialog1_FileOk5(object sender, CancelEventArgs e)
        {
            try
            {
                klvv.ImportRanking(XDocument.Load(openFileDialog1.FileName).Root);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed importing ranking. Error: " + exc.ToString());
            }
            klvv.Evaluate(null);
            klvv.Changed();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void clubteamVergelijkingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select vorig project waarmee te vergelijken";
            openFileDialog1.FileName = "FullCompetition.xml";
            openFileDialog1.Filter = "Xml (*.xml)|*.xml";
            openFileDialog1.InitialDirectory = BaseDirectory;
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk4);
            openFileDialog1.ShowDialog();
        }
        public void openFileDialog1_FileOk4(object sender, CancelEventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("Compare_registrations{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk11);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk11(object sender, CancelEventArgs e)
        {
            klvv.CompareRegistrations(openFileDialog1.FileName, saveFileDialog1.FileName);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void licenseToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LicenseView license = new LicenseView(klvv);
            license.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySettings mySettings = MySettings.Load(BaseDirectory + @"\MySettings.xml");
            Settings settings = new Settings(mySettings);
            settings.Show();
        }

        private void conflictPerTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1 = new SaveFileDialog();
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("ConflictReportPerType{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute); 
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            klvv.WriteConflictReportPerType(saveFileDialog1.FileName);
        }

        private void conflictsPerClubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1 = new SaveFileDialog();
            DateTime now = DateTime.Now;
            saveFileDialog1.FileName = string.Format("ConflictReportPerClub{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute); 
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk1);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk1(object sender, CancelEventArgs e)
        {
            klvv.WriteConflictReportPerClub(saveFileDialog1.FileName);
        }

        private void matchesPerClubcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("CompetitionPerClub{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk2);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk2(object sender, CancelEventArgs e)
        {
            klvv.WriteCompetitionPerClub(saveFileDialog1.FileName);

        }

        private void poulesSeriescsvToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("PouleSeries{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk5);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk5(object sender, CancelEventArgs e)
        {
            klvv.WriteSeriePoules(saveFileDialog1.FileName);

        }

        private void matchescsvToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("Matches{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk6);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk6(object sender, CancelEventArgs e)
        {
            List<Selection> selection = new List<Selection>();
            foreach (Serie serie in klvv.series)
            {
                Selection sel = new Selection(serie.name);
                sel.obj = serie;
                sel.selected = (serie.imported == false);
                selection.Add(sel);
            }
            SelectionDialog diag = new SelectionDialog(selection, true);
            diag.ShowDialog();
            if (diag.Ok)
            {
                List<Serie> series = new List<Serie>();
                foreach (Selection sel in diag.Selections)
                {
                    if (sel.selected) series.Add((Serie)sel.obj);
                }
                klvv.WriteMatches(saveFileDialog1.FileName, series);
            }

        }

        private void statisticscsvToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("Statistics{0:00}{1:00}{2:00}_{3:00}{4:00}.csv", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Comma-separated (*.csv)|*.csv";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk7);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk7(object sender, CancelEventArgs e)
        {
            klvv.WriteStatistics(saveFileDialog1.FileName);

        }

        private void importRankingWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Input the URL of the website that has the ranking", "URL:", MySettings.Settings.RankingXML);
            form.ShowDialog();
            if (form.Result == true)
            {
                try
                {
                    klvv.ImportRanking(XDocument.Load(form.GetInputString()).Root);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Failed importing ranking. Error: " + exc.ToString());
                }
                klvv.Evaluate(null);
                klvv.Changed();
            }
        }


    }
}

