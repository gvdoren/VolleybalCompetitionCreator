using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace CompetitionCreator
{
    public class Error
    {
        string detail = null;
        string help;
        DateTime datetime;
        public bool HasTime { get; set; }
        public string GetTime()
        {
            return datetime.ToShortDateString() + " "+ datetime.ToShortTimeString();
        }
        public void AddDetailHtml(string html)
        {
            detail = html;
        }
        public void AddDetailText(string text)
        {
            detail = text;
        }
        public string GetDetail() { return detail; }
        public string GetHelp() { return help; }
        public void AddHelpHtml(string html)
        {
            help = html;
        }
        static public void AddManualError(string text, string text2 = null)
        {
            Error error = new Error(ErrorType.ManualClearable);
            error.text = text;
            error.detail = text2;
            error.HasTime = true;
            error.datetime = DateTime.Now;
            GlobalState.errors.Add(error);
            GlobalState.Changed();
        }
        
        public enum ErrorType {Persistant, AutoClearable, ManualClearable};
        public ErrorType Type;
        public Error(ErrorType type = ErrorType.AutoClearable)
        {
            Type = type;
        }
        public string text { get; set; }
    }
}
