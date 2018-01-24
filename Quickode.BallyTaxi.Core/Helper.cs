using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quickode.BallyTaxi.Core
{
    public class Helper
    {
        readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Translate(string phrase, string fallbackString)
        {
            var translatedPhrase = "";
            var found = true;
            try
            {
                // fail safe for languages
                if (Resources.Resources.Culture == null)
                {
                    Resources.Resources.Culture = new System.Globalization.CultureInfo(CultureHelper.GetCurrentCulture());
                }

                translatedPhrase = Resources.Resources.ResourceManager.GetString(phrase, Resources.Resources.Culture);
            }
            catch (Exception)
            {
                translatedPhrase = fallbackString;
                found = false;
            }
            if (!found || string.IsNullOrWhiteSpace(translatedPhrase))
            {
                logger.DebugFormat("Missing string {0} - {1}", phrase, fallbackString);
                translatedPhrase = fallbackString;
            }
            return translatedPhrase;
        }

    }
}
