
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
    
public partial class Image
{

    public Image()
    {

        this.Users = new HashSet<User>();

    }


    public System.Guid ImageId { get; set; }

    public string Extension { get; set; }



    public virtual ICollection<User> Users { get; set; }

}

}