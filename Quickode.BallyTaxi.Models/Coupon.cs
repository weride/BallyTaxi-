
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Quickode.BallyTaxi.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Coupon
{

    public int couponId { get; set; }

    public string number { get; set; }

    public double amount { get; set; }

    public string currency { get; set; }

    public Nullable<System.DateTime> dtStart { get; set; }

    public System.DateTime dtEnd { get; set; }

    public Nullable<long> orderId { get; set; }

    public Nullable<long> passengerId { get; set; }

    public Nullable<long> passengerIdSMS { get; set; }



    public virtual Order Order { get; set; }

    public virtual User User { get; set; }

    public virtual User User1 { get; set; }

}

}
