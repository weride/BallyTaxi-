using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Filters;
using System.Xml;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class BusinessServices
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static List<Business> GetListOfBusiness(string isoCountry)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.Businesses.ByCountry(isoCountry).Where(b=>b.isActive!=false).ToList();
            }
        }

        public static Business GetBusinessById(int businessId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.Businesses.Where(b => b.BusinessId == businessId).FirstOrDefault();
            }
        }

        public static string getCityfromLocation(double lat, double lon)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                var Address_ShortName = "";
                //doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon + "&sensor=false");
                 doc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon +  "&key=" + ConfigurationHelper.GoogleAPIKey);

                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    //return ("No data available for the specified location");
                    return null;
                }
                else
                {
                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                    string longname = "";
                    string shortname = "";
                    string typename = "";
                    bool fHit = false;

                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                    foreach (XmlNode xn in xnList)
                    {
                        longname = xn["long_name"].InnerText;
                        shortname = xn["short_name"].InnerText;
                        typename = xn["type"].InnerText;
                        fHit = true;
                        switch (typename)
                        {
                            //Add whatever you are looking for below
                            case "country":
                                {
                                    // var Address_country = longname;
                                    Address_ShortName = shortname;
                                    break;
                                }
                            default:
                                fHit = false;
                                break;
                        }
                    }
                }
                return Address_ShortName;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Business AddBusiness(string isoCountry, string businessName, string paypalAccount, string phone, bool isNeedFile)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses.ByCountry(isoCountry).ByName(businessName).SingleOrDefault();

                if (business != null)
                    throw new CompanyExistsException();

                business = new Business()
                {
                    BusinessName = businessName,
                    IsoCountry = isoCountry,
                    PayPalAccountId = paypalAccount,
                    Phone = phone,
                    isNeedFile = isNeedFile
                };


                db.Businesses.Add(business);
                db.SaveChanges();

                return business;
            }
        }

        public static bool UpdateBusiness(string isoCountry, string businessName, string paypalAccount, string phone)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses.ByCountry(isoCountry).ByName(businessName).SingleOrDefault();

                if (business == null)
                    throw new CompanyNotExistsException();

                business.BusinessName = businessName;
                business.PayPalAccountId = paypalAccount;
                business.Phone = phone;

                db.SaveChanges();

                return true;
            }
        }
        public static bool DeleteBusiness(string isoCountry, string businessName)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses.ByCountry(isoCountry).ByName(businessName).SingleOrDefault();

                if (business == null)
                    throw new CompanyNotExistsException();



                db.Businesses.Remove(business);
                db.SaveChanges();

                return true;
            }
        }


        public static bool AddPerson(string isoCountry, string businessName, string phone)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses
                    .Include("BusinessApprovedPhones")
                    .ByCountry(isoCountry).ByName(businessName).SingleOrDefault();

                if (business == null)
                    throw new CompanyNotExistsException();

                var record = business.BusinessApprovedPhones.Where(x => x.Phone == phone).SingleOrDefault();

                if (record == null)
                {
                    business.BusinessApprovedPhones.Add(new BusinessApprovedPhone()
                    {
                        ApprovedDate = DateTime.UtcNow,
                        BusinessId = business.BusinessId,
                        CancelledDate = null,
                        Phone = phone
                    });
                }
                else if (record.CancelledDate.HasValue)

                    record.CancelledDate = null;
                else

                    throw new PhoneAlreadyRegisteredToCompany();

                db.SaveChanges();

                return true;
            }
        }


        public static bool DeletePerson(string isoCountry, string businessName, string phone)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses
                    .Include("BusinessApprovedPhones")
                    .ByCountry(isoCountry).ByName(businessName).SingleOrDefault();

                if (business == null)
                    throw new CompanyNotExistsException();

                var record = business.BusinessApprovedPhones.Where(x => x.Phone == phone).SingleOrDefault();

                if (record == null)
                    throw new PhoneNotRegistetedToCompany();

                if (record.CancelledDate.HasValue)
                    throw new PhoneNotRegistetedToCompany();

                record.CancelledDate = DateTime.UtcNow;

                db.SaveChanges();

                return true;
            }
        }
        public static bool CheckPerson(string isoCountry, string businessName, string phone, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var business = db.Businesses
                    .Include("BusinessApprovedPhones")
                    .ByName(businessName).SingleOrDefault(); //.ByCountry(isoCountry)

                if (business == null)
                    throw new CompanyNotExistsException();

                var record = business.BusinessApprovedPhones.Where(x => x.Phone == phone).SingleOrDefault();

                if (record == null)
                    return false;
                
                if (record.CancelledDate.HasValue)
                    return false;

                if (record != null)
                {
                    var user = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                    if (user != null)
                    {
                        user.BusinessId = business.BusinessId;
                        db.SaveChanges();
                    }
                }

                return true;
            }
        }
    }
}
