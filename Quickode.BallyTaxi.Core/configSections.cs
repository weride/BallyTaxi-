using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickode.BallyTaxi.Core
{
    public static class configSections
    {
        public static bool ConnectionStringKeyExists(string key)
        {
            if (ConfigurationManager.ConnectionStrings[key] != null)
                return true;
            else
                return false;
        }
    }
}
