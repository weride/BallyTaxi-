using System;
using System.Data.Entity.Core.EntityClient;



namespace Quickode.BallyTaxi.Models
{
    public static class ConnectionTools
    {        
        // all params are optional
        public static BallyTaxiEntities AutoLocal(this BallyTaxiEntities source, string configNameEf = "BallyTaxiEntities")
        {
            try
            {
                var key = configNameEf + "_" + Environment.MachineName;
                if (Core.configSections.ConnectionStringKeyExists(key))
                {
                    // add a reference to System.Configuration
                    var entityCnxStringBuilder = new EntityConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings[key].ConnectionString);
                    // now flip the properties that were changed
                    source.Database.Connection.ConnectionString = entityCnxStringBuilder.ProviderConnectionString;
                }
                return source;
            }
            catch (Exception)
            {
                // set log item if required
                return source;
            }
        }
    }
}