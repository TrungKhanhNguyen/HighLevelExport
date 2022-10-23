using HighLevelExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighLevelExport.Helper
{
    public class Utility
    {
        public LatLonObject getLatLonFromString(string latlon)
        {
            var lat = "";
            var lon = "";
            if (!String.IsNullOrEmpty(latlon))
            {
                var indexOfComma = latlon.IndexOf(',');
                var indexOfOpenSquare = latlon.IndexOf('[');
                var indexOfCloseSquare = latlon.IndexOf('[');
                lat = latlon.Substring(indexOfOpenSquare + 1, indexOfComma - indexOfOpenSquare - 1);
                lon = latlon.Substring(indexOfComma + 1, indexOfCloseSquare - indexOfComma - 1);
            }
            var latlonObj = new LatLonObject
            {
                lat = lat,
                lon = lon
            };
            return latlonObj;
        }

        public string getNetworkByCGI(string cgi)
        {
            var network = "";
            try
            {
                if (!String.IsNullOrEmpty(cgi))
                {
                    var tempNetwork = cgi.Substring(4, 2);
                    switch (tempNetwork)
                    {
                        case "01": network = "Mobifone"; break;
                        case "02": network = "Vinaphone"; break;
                        case "04": network = "Viettel"; break;
                    }
                }
                return network;
            }
            catch { return ""; }
            
        }

        public double getUnixTimeByDate(DateTime date)
        {
            var timeSpan = (date - new DateTime(1970, 1, 1, 0, 0, 0));
            return timeSpan.TotalSeconds;
        }

        public string getDecodedSMSFromUrl(string url)
        {
            string sms = "";
            var subUrl = url.Replace("file://var/intellego/", "").Replace("/", @"\");
            var absoluteUrl = StaticKey.MAIN_URL_FOLDER + subUrl.Trim();
            string[] lines = System.IO.File.ReadAllLines(absoluteUrl);
            if(lines.Length >= 2)
            {
                sms = lines[1];
            }
            return sms;
        }


        
    }
}
