using ApplicationDbLibrary.Entities.Context;
using ApplicationDbLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

namespace ApplicationDBPersisTence
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public LoadWindow lw;

        public App()
        {

            lw = new LoadWindow();
            lw.Show();

            try{

                //Database.SetInitializer<AppDbContext>(new MigrateDatabaseToLatestVersion<AppDbContext, ApplicationDbLibrary.Entities.Context.Configuration>());
            }catch { }

            lw.Hide();
        }
    }
}
