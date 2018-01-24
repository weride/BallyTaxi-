using Quickode.BallyTaxi.API.Controllers;
using Quickode.BallyTaxi.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
namespace ConsoleApplication
{
    class Program
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Logger.Info("Starting Ballytaxi background tasks");
            //

            Console.WriteLine("Starting Ballytaxi background tasks");
            //Console.WriteLine("DEBUG ==> delete readkey in Program.cs");
            //Console.ReadKey();
            OrderService.HandlePendingOrders();

            Logger.Info("Ballytaxi background tasks End");
            Console.WriteLine("Ballytaxi background tasks End");

        }
    }
}
