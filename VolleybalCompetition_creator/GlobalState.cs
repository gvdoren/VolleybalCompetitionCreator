using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VolleybalCompetition_creator
{
    public class GlobalState
    {
        public List<Club> selectedClubs = new List<Club>();
        public event MyEventHandler OnMyChange;
        public void Changed()
        {
            //call it then you need to update:
            if (OnMyChange != null)
            {
                MyEventArgs e = new MyEventArgs();
                //e.EventInfo = content;
                OnMyChange(this, e);
            }
        }
    }
    public delegate void MyEventHandler(object source, MyEventArgs e);

    public class MyEventArgs : EventArgs
    {
        public MyEventArgs()
        {
        }
    }
}
