
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
    
public partial class LocationForRegion
{

    public LocationForRegion()
    {

        this.Regions = new HashSet<Region>();

    }


    public int locationId { get; set; }

    public System.Data.Entity.Spatial.DbGeography location { get; set; }



    public virtual ICollection<Region> Regions { get; set; }

}

}
