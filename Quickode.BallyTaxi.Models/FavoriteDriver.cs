
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
    
public partial class FavoriteDriver
{

    public long PassengerId { get; set; }

    public long DriverId { get; set; }

    public string Notes { get; set; }

    public Nullable<System.DateTime> CreationDate { get; set; }



    public virtual User Passenger { get; set; }

    public virtual Driver Driver { get; set; }

}

}
