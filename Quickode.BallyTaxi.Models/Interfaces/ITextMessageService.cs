using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Interfaces
{ 
        public interface ITextMessageService
        {
            bool SendSMS(string stream, string filename, bool debug = false); 
        } 
}