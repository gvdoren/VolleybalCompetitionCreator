using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetitionCreator
{
    public class GlobalState
    {
        static public List<Error> errors = new List<Error>();

        static public int optimizeLevel;
        static public List<Club> selectedClubs = new List<Club>();
        static public List<Poule> selectedPoules = new List<Poule>();
        static public List<Poule> shownPoules = new List<Poule>();

        static public Constraint selectedConstraint = null;
        static public List<Constraint> showConstraints = new List<Constraint>();
        static public bool comparison = false;
        static public void ShowConstraints(List<Constraint> constraints)
        {
            var areEquivalent = (constraints.Count == showConstraints.Count) && !constraints.Except(showConstraints).Any();
            if (areEquivalent == false)
            {
                showConstraints = constraints;
                GlobalState.Changed();
            }
        }
        static public event MyEventHandler OnMyChange;
        static public void Changed()
        {
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs(null);
                //e.EventInfo = content;
                GlobalState.OnMyChange(null, e);
            }
        }
        static public void ClearAutoClearableErrors()
        {
            errors.RemoveAll(e => e.Type == Error.ErrorType.AutoClearable);
            Changed();
        }
        static public void ClearManualClearableErrors()
        {
            errors.RemoveAll(e => e.Type == Error.ErrorType.ManualClearable);
            Changed();
        }
        public void Clear()
        {
            selectedClubs = new List<Club>();
            selectedConstraint = null;
            showConstraints = new List<Constraint>();
            Changed();
        }
    }
    public delegate void MyEventHandler(object source, MyEventArgs e);

    public class MyEventArgs : EventArgs
    {
        public Model model;
        public MyEventArgs(Model model)
        {
            this.model = model;
        }
    }
}
