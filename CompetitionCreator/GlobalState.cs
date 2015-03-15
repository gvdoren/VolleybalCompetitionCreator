using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetitionCreator
{
    public class GlobalState
    {
        public List<Club> selectedClubs = new List<Club>();
        public Constraint selectedConstraint = null;
        public List<Constraint> showConstraints = new List<Constraint>();
        public Model comparisonKlvv = null;
        public bool comparison = false;
        public void ShowConstraints(List<Constraint> constraints)
        {
            var areEquivalent = (constraints.Count == showConstraints.Count) && !constraints.Except(showConstraints).Any();
            if (areEquivalent == false)
            {
                showConstraints = constraints;
                Changed();
                Console.WriteLine("Show constraints updated");
            }
        }
        public event MyEventHandler OnMyChange;
        public void Changed()
        {
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs(null);
                //e.EventInfo = content;
                OnMyChange(this, e);
            }
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
        public Model klvv;
        public MyEventArgs(Model klvv)
        {
            this.klvv = klvv;
        }
    }
}
