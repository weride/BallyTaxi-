
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
    
public partial class TaxiStation
{

    public TaxiStation()
    {

        this.Drivers = new HashSet<Driver>();

        this.ISR_TaxiStations = new HashSet<ISR_TaxiStations>();

        this.Users = new HashSet<User>();

        this.Regions = new HashSet<Region>();

        this.Businesses = new HashSet<Business>();

        this.Orders = new HashSet<Order>();

    }


    public int StationId { get; set; }

    public string HebrewName { get; set; }

    public string EnglishName { get; set; }

    public string StationOperator { get; set; }

    public string IsoCountry { get; set; }

    public string Region { get; set; }



    public virtual ICollection<Driver> Drivers { get; set; }

    public virtual ICollection<ISR_TaxiStations> ISR_TaxiStations { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<Region> Regions { get; set; }

    public virtual ICollection<Business> Businesses { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

}

}
