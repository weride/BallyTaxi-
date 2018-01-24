using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using System.Data.Entity;

namespace Quickode.BallyTaxi.BackgroundTasks
{
    public class BackgroundTask
    {
         
        //1.	Loop (while) every one second
        //    1.1 for each orders with step 1 status:    
        //        1.1.1 Case of upcoming ride (Ander 1 hour from creation time):
        //                        Find 5 best ETA for AV drivers
        //                        Send push message for this drivers group 
        //                        Move order to step 2
        //        1.1.2      Case of future ride (more than 1 hour):
        //                        Find favorite drivers that “CenGetFuture”
        //                        Send push message to driver
        //                        Move order to step 2
        //    1.2 For each orders with step 2 status
        //        1.2.1 If order still on status “Pending” (no driver accepted)
        //                        Case of upcoming ride (Ander 1 hour from creation time):
        //                        1.2.1.1 If 10 seconds have passed from last step 2 status time – will resend push message for other                                     driver’s group.
        //                                        Update time in last update step field (without change status – only time)
        //                        1.2.1.2 If 15 minutes have passed from creation time of this order – 
        //                                        Change step status to step 3 - Completion of the order
        //                                        Send push message to passenger – “DriverNotFounded”
        //                        Case of future ride (more than 1 hour): 
        //                        1.2.1.1   If 1 minute have passed from last step 2 status time – will resend push message for all                                            drivers that “CenGetFuture”

        //?        1.2.2 If order have status “Confirmed” (has driver that accept)
        //?                        Change step field to step 3 - has driver that accepted
        //?                       ? Send push message to passenger “DriverFounded”
        //?                       ? Send push message to all driver’s group from step 2 “rideNotRelevantAnymore”

        public void OrderFlow()
        {
            //new Thread(() =>
            //{
            //    while (true)
            //    {
                   
            //        Thread.Sleep(1000);

            //        //other tasks
            //    }
            //}).Start();

            using (BallyTaxiEntities db = new BallyTaxiEntities())
            {
                while (true)
                {
                     
                    List<Order> pending_orders = db.Orders
                        .Include(x => x.Drivers)
                        .Where(x => x.StatusId == (int)OrderStatus.Pending)
                        .ToList();
                    // treatment in oreders with STEP 1
                    //upcoming rides and have step 1
                    foreach (var item in pending_orders.Where(x => x.FlowStep == (int)FlowSteps.Step1 && x.OrderTime.HasValue && x.OrderTime.Value < DateTime.UtcNow.AddHours(1)))
                    {
                        
                    }

                    //feture rides and have step 1
                    foreach (var item in pending_orders.Where(x => x.FlowStep == (int)FlowSteps.Step1 && x.OrderTime.HasValue && x.OrderTime.Value >= DateTime.UtcNow.AddHours(1)))
                    {

                    }
                    // end of treatment in oreders with STEP 1

                    // treatment in oreders with STEP 2
                    //upcoming rides and have step 2
                    foreach (var item in pending_orders.Where(x => x.FlowStep == (int)FlowSteps.Step2 && x.OrderTime.HasValue && x.OrderTime.Value < DateTime.UtcNow.AddHours(1)))
                    {

                    }

                    //feture rides and have step 2
                    foreach (var item in pending_orders.Where(x => x.FlowStep == (int)FlowSteps.Step2 && x.OrderTime.HasValue && x.OrderTime.Value >= DateTime.UtcNow.AddHours(1)))
                    {

                    }
                    // end of treatment in oreders with STEP 2



                    //List<Order> confirmed_orders = db.Orders.Where(x => x.StatusId == (int)OrderStatus.Confirmed).ToList();
                    //foreach (var item in confirmed_orders.Where(x => x.FlowStep == (int)FlowSteps.Step2))
                    //{

                    //} 
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
