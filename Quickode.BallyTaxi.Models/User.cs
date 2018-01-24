
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
    
public partial class User
{

    public User()
    {

        this.FavoriteAddresses = new HashSet<FavoriteAddress>();

        this.FavoriteDrivers = new HashSet<FavoriteDriver>();

        this.Orders = new HashSet<Order>();

        this.CreditCards = new HashSet<CreditCard>();

        this.Messages = new HashSet<Message>();

        this.CreditCardUsers = new HashSet<CreditCardUser>();

        this.Coupons = new HashSet<Coupon>();

        this.Coupons1 = new HashSet<Coupon>();

    }


    public long UserId { get; set; }

    public System.DateTime RegistrationDate { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public string DeviceId { get; set; }

    public string DriverNotificationToken { get; set; }

    public string PassengerNotificationToken { get; set; }

    public Nullable<int> PlatformId { get; set; }

    public string VersionOS { get; set; }

    public string AuthenticationToken { get; set; }

    public bool Active { get; set; }

    public string Name { get; set; }

    public string AppVersion { get; set; }

    public Nullable<System.Guid> ImageId { get; set; }

    public int LanguageId { get; set; }

    public Nullable<bool> AlwaysApproveSum { get; set; }

    public Nullable<bool> IsDebug { get; set; }

    public Nullable<int> PreferredPaymentMethod { get; set; }

    public bool DriverValidNotificationToken { get; set; }

    public bool PassengerValidNotificationToken { get; set; }

    public Nullable<int> PreferedStationId { get; set; }

    public string PayPalId { get; set; }

    public Nullable<bool> isFromWeb { get; set; }

    public System.Data.Entity.Spatial.DbGeography locationHome { get; set; }

    public string homeAddress { get; set; }

    public string homeCity { get; set; }

    public System.Data.Entity.Spatial.DbGeography locationBusiness { get; set; }

    public string businessAddress { get; set; }

    public string businessCity { get; set; }

    public Nullable<int> BusinessId { get; set; }

    public Nullable<bool> isHandicapped { get; set; }

    public Nullable<bool> isVirtual { get; set; }

    public Nullable<bool> isFromStations { get; set; }

    public Nullable<bool> isReadTermsOfUse { get; set; }

    public Nullable<System.DateTime> lastSendNotificationForUpdate { get; set; }

    public Nullable<bool> isFromIVR { get; set; }



    public virtual Image Image { get; set; }

    public virtual Language Language { get; set; }

    public virtual ICollection<FavoriteAddress> FavoriteAddresses { get; set; }

    public virtual ICollection<FavoriteDriver> FavoriteDrivers { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

    public virtual Driver Driver { get; set; }

    public virtual ICollection<CreditCard> CreditCards { get; set; }

    public virtual ICollection<Message> Messages { get; set; }

    public virtual Passenger Passenger { get; set; }

    public virtual ICollection<CreditCardUser> CreditCardUsers { get; set; }

    public virtual Business Business { get; set; }

    public virtual ICollection<Coupon> Coupons { get; set; }

    public virtual ICollection<Coupon> Coupons1 { get; set; }

    public virtual TaxiStation TaxiStation { get; set; }

}

}