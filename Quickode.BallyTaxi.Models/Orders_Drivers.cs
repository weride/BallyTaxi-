
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
    
public partial class Orders_Drivers
{

    public long OrderId { get; set; }

    public long DriverId { get; set; }

    public int StatusId { get; set; }

    public int Priority { get; set; }

    public Nullable<bool> isReadTheOrderForDriver { get; set; }



    public virtual Order Order { get; set; }

    public virtual Driver Driver { get; set; }

}

}