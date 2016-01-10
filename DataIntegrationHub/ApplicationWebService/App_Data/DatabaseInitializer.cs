using ApplicationDbLibrary.Entities.Context;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Web;

namespace ApplicationWebService
{
    public class DatabaseInitializer : IServiceBehavior
    {
        private bool initializeDatabase;
        public DatabaseInitializer(bool initializeDatabase)
        {
            this.initializeDatabase = initializeDatabase;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            if (initializeDatabase) 
                Database.SetInitializer<AppDbContext>(new MigrateDatabaseToLatestVersion<AppDbContext, ApplicationDbLibrary.Entities.Context.Configuration>(true));
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }
    }

    public class DatabaseInitializerExtension : BehaviorExtensionElement
    {
        const string InitializeDatabasePropertyName = "initializeDatabase";
        [ConfigurationProperty(InitializeDatabasePropertyName)]
        public bool InitializeDatabase
        {
            get
            {
                return (bool)base[InitializeDatabasePropertyName];
            }
            set
            {
                base[InitializeDatabasePropertyName] = value;
            }
        }

        public override Type BehaviorType
        {
            get { return typeof(DatabaseInitializer); }
        }

        protected override object CreateBehavior()
        {
            return new DatabaseInitializer(InitializeDatabase);
        }
    }
}