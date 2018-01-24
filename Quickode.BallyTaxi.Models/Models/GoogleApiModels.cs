namespace Quickode.BallyTaxi.Models.Models
{

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class DurationInTraffic
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Element
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string status { get; set; }
        public DurationInTraffic duration_in_traffic { get; set; }
    }

    public class Row
    {
        public Element[] elements { get; set; }
    }

    public class Parent
    {
        public string[] destination_addresses { get; set; }
        public string[] origin_addresses { get; set; }
        public Row[] rows { get; set; }
        public string status { get; set; }
    }


    public class timeZoneFromLocation
    {
        public long dstOffset { get; set; }
        public long rawOffset { get; set; }
        public string status { get; set; }
        public string timeZoneId { get; set; }
        public string timeZoneName { get; set; }
    }
    public class timeZoneObj
    {
        public string timeZoneId { get; set; }
        public System.DateTime dateTime { get; set; }
    }


    public class ShabbatLocation
    {
        public string country { get; set; }
        public string geo { get; set; }
        public string tzid { get; set; }
        public string city { get; set; }
        public double latitude { get; set; }
        public int geonameid { get; set; }
        public string title { get; set; }
        public string admin1 { get; set; }
        public double longitude { get; set; }
    }

    public class ShabbatEvent
    {
        public string memo { get; set; }
        public string category { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        //public System.DateTime date { get; set; }
        public System.DateTime date { get; set; }
        public string hebrew { get; set; }
        public Leyning leyning { get; set; }
    }

    public class Leyning
    {
        public string maftir { get; set; }
    
        public string torah { get; set; }
     
        public string haftarah { get; set; }

    }

    public class ShabbatObj
    {
        public string link { get; set; }
        public System.Collections.Generic.List<ShabbatEvent> items { get; set; }
        public ShabbatLocation location { get; set; }
        public string title { get; set; }
        public System.DateTime date { get; set; }
       //?? public double date { get; set; }

    }

}