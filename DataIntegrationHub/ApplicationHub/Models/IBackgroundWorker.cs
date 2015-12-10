using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub.Models
{
    public interface IBackgroundWorker
    {
        void DoWork(object o, DoWorkEventArgs args);
        void ProgressChanged(object o, ProgressChangedEventArgs args);
        void RunWorkerCompleted(object o, RunWorkerCompletedEventArgs e);
        void SetWorker(BackgroundWorker worker);
    }
}
