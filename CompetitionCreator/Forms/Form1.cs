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

namespace CompetitionCreator
{
    public partial class Form1 : Form
    {
        Model model = null;
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
            model = new Model(DateTime.Now.Year);
            
            Text = string.Format("Competition Creator Tool ({0})", model.year);

            this.WindowState = FormWindowState.Maximized;
            ClubListView clubview = new ClubListView(model,state);
            clubview.ShowHint = DockState.DockLeft;
            clubview.Show(dockPanel);

            //SerieView serieview = new SerieView(model, state);
            //serieview.ShowHint = DockState.DockRight;
            //serieview.Show(dockPanel);
            //serieview.Show(clubview.Pane, DockAlignment.Right, 0.35);
            //SerieTreeView serietreeview = new SerieTreeView(model, state);
            //serietreeview.ShowHint = DockState.DockRight;
            //serietreeview.Show(dockPanel);
            
            PouleListView pouleListview = new PouleListView(model, state);
            pouleListview.ShowHint = DockState.DockLeft;
            pouleListview.Show(clubview.Pane, DockAlignment.Right, 0.5);
            //pouleListview.Show(dockPanel);
            ConstraintListView constraintListview = new ConstraintListView(model, state);
            constraintListview.ShowHint = DockState.DockRight;
            constraintListview.Show(dockPanel);
            dockPanel.DockRightPortion = 500;
            List<string> ListTeam = new List<string>();
            for (int i = 0; i < 11; i++)
            {
                ListTeam.Add(string.Format("Team {0}",i+1));
            }
            model.OnMyChange += state_OnMyChange;
            state.OnMyChange += state_OnMyChange;
            model.OpenLastProject();
        }
        public void state_OnMyChange(object source, MyEventArgs e)
        {
            if (e.model != null)
            {
                model.OnMyChange -= state_OnMyChange;
                model = e.model;
                state.Clear();
                model.OnMyChange += state_OnMyChange;
            }
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => state_OnMyChange(source, e)));
                return;
            }
            lock (model)
            {
                string changed = model.stateNotSaved ? "*" : "";
                this.Text = string.Format("Competition Creator Tool ({0}{1})", model.savedFileName, changed);
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
            clubview = new ClubListView(model, state);
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
            pouleListView = new PouleListView(model, state);
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
            clubview = new ClubListView(model, state);
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
            optimizeForm = new OptimizeForm(model, state);
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
            constraintView = new ConstraintListView(model, state);
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
            anorama = new AnoramaView(model, state);
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
            serieView = new SerieView(model, state);
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
            clubConstraints = new InschrijvingenView(model, state);
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
            model.WriteProject(saveFileDialog1.FileName);
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
            model.LoadFullCompetition(openFileDialog1.FileName);
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
                    Model newModel = new Model(year);
                    model.Changed(newModel);
                    newModel.Evaluate(null);
                    newModel.Changed();
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
            teamListView = new TeamListView(model, state);
            teamListView.ShowHint = DockState.DockLeft;
            teamListView.Show(this.dockPanel);

        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            model.WriteProject(model.savedFileName);
        }

        private void ImportSubscriptionsmodelbeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Input the URL of the website that has the registrations", "URL:", MySettings.Settings.RegistrationsXML);
            form.ShowDialog();
            if(form.Result == true)
            {
                model.ImportTeamSubscriptions(XDocument.Load(form.GetInputString()).Root);
            }
        }

        private void clubRegistrationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MyDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            model.WriteClubConstraints(MyDocuments + "\\ClubRegistrations.xml");
        }

        private void exportCompetitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ExportedCompetition{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk4);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk4(object sender, CancelEventArgs e)
        {
            List<Selection> selection = new List<Selection>();
            foreach (Serie serie in model.series)
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

                model.WriteExportCompetitionXml(saveFileDialog1.FileName, series);
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
            model.ConvertVVBCompetitionToCSV(saveFileDialog1.FileName);
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
            model.ImportCSV(openFileDialog1.FileName);
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
            model.ImportTeamSubscriptions(XDocument.Load(openFileDialog1.FileName).Root);
        }

        private void convertKlvvToCSVToolStripMenuItem_Click(object sender, EventArgs e)
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
            model.ConvertKLVVCompetitionToCSV(saveFileDialog1.FileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (model.stateNotSaved)
            {
                DialogResult result = MessageBox.Show("Competition changed. Do you want to save the competition first?", "Closing application",
MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void exportRegistrationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = string.Format("ExportedRegistrations{0:00}{1:00}{2:00}_{3:00}{4:00}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute);
            saveFileDialog1.Filter = "Xml (*.xml)|*.xml";
            saveFileDialog1.InitialDirectory = BaseDirectory;
            saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk10);
            saveFileDialog1.ShowDialog();
        }
        public void saveFileDialog1_FileOk10(object sender, CancelEventArgs e)
        {
            model.WriteClubConstraints(saveFileDialog1.FileName,false /* zonder nationale teams*/); 
        }

        private void importRankingToolStripMenuItem_Click(object sender, EventArgs e)
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
                model.ImportRanking(XDocument.Load(openFileDialog1.FileName).Root);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed importing ranking. Error: " + exc.ToString());
            }
            model.Evaluate(null);
            model.Changed();
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
            model.CompareRegistrations(openFileDialog1.FileName, saveFileDialog1.FileName);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void licenseToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LicenseView license = new LicenseView(model);
            license.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(MySettings.Settings);
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
            model.WriteConflictReportPerType(saveFileDialog1.FileName);
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
            model.WriteConflictReportPerClub(saveFileDialog1.FileName);
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
            model.WriteCompetitionPerClub(saveFileDialog1.FileName);

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
            model.WriteSeriePoules(saveFileDialog1.FileName);

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
            foreach (Serie serie in model.series)
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
                model.WriteMatches(saveFileDialog1.FileName, series);
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
            model.WriteStatistics(saveFileDialog1.FileName);

        }

        private void importRankingWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputForm form = new InputForm("Input the URL of the website that has the ranking", "URL:", MySettings.Settings.RankingXML);
            form.ShowDialog();
            if (form.Result == true)
            {
                try
                {
                    model.ImportRanking(XDocument.Load(form.GetInputString()).Root);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Failed importing ranking. Error: " + exc.ToString());
                }
                model.Evaluate(null);
                model.Changed();
            }
        }

        private void invoerToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            DateTime last = DateTime.Now;
            foreach(Poule poule in model.poules)
            {
                DateTime l1 = poule.weekendsSecond[poule.weekendsSecond.Count-1].Saturday;
                if(l1<last) last = l1;
            }
            if (model.licenseKey.Valid() == false ||
                DateTime.Now > model.licenseKey.ValidUntil()||
                last > model.licenseKey.ValidUntil() ||
                (model.teams.Count > 1000 && model.licenseKey.Feature(Security.LicenseKey.FeatureType.Above1000Teams) == false)
                )
            {
                this.exportToolStripMenuItem.Enabled = false;
                this.importToolStripMenuItem.Enabled = false;
                this.saveToolStripMenuItem.Enabled = false;
                this.saveToolStripMenuItem1.Enabled = false;
                this.reportToolStripMenuItem1.Enabled = false;
            } else 
            {
                this.exportToolStripMenuItem.Enabled = true;
                this.importToolStripMenuItem.Enabled = true;
                this.saveToolStripMenuItem.Enabled = true;
                this.saveToolStripMenuItem1.Enabled = true;
                this.reportToolStripMenuItem1.Enabled = true;
            }
        }


    }
}

