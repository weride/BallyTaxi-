
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
    
public partial class Business
{

    public Business()
    {

        this.BusinessApprovedPhones = new HashSet<BusinessApprovedPhone>();

        this.Accounts = new HashSet<Account>();

        this.Users = new HashSet<User>();

        this.Orders = new HashSet<Order>();

    }


    public int BusinessId { get; set; }

    public string IsoCountry { get; set; }

    public string BusinessName { get; set; }

    public string PayPalAccountId { get; set; }

    public string Phone { get; set; }

    public Nullable<bool> isNeedFile { get; set; }

    public string CompanyPhone { get; set; }

    public Nullable<bool> isReadTermsofUse { get; set; }

    public Nullable<bool> isActive { get; set; }

    public Nullable<int> taxiStationId { get; set; }

    public System.Data.Entity.Spatial.DbGeography defaultLocationAddress { get; set; }



    public virtual ICollection<BusinessApprovedPhone> BusinessApprovedPhones { get; set; }

    public virtual ICollection<Account> Accounts { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

    public virtual TaxiStation TaxiStation { get; set; }

}

}