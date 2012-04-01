using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using CurrencyRatesInterface;
using System.Linq;


namespace RoboForexCurrencyRates
{
    [VendorInformation( Provider = "RoboForex",
        Version = "1.0",
        Vendor = "A1ex Inc" )]
    public class RoboForexCurrencyRates : AbstractCurrencyRates
    {

        protected override Dictionary<string, Rate> LoadRates()
        {
            //https://my.roboforex.com/app/cache/common/quotes.json

            Dictionary<string, Rate> rates = new Dictionary<string, Rate>();

            var resp = HttpWebRequest.Create( "https://my.roboforex.com/app/cache/common/quotes.json" ).GetResponse();
            var stream = resp.GetResponseStream();
            string data = (new StreamReader( stream )).ReadToEnd();

            
            var parseFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;
            Rate tmp = new Rate();

            foreach( Match row in exp.Matches( data ) )
            {
                try
                {
                    tmp.Bid = Convert.ToDecimal( row.Groups[2].Value, parseFormat );
                    tmp.Ask = Convert.ToDecimal( row.Groups[3].Value, parseFormat );

                    if( !rates.Keys.Contains( row.Groups[1].Value ) )
                        rates.Add( row.Groups[1].Value, tmp );
                }
                catch( Exception ) {}
            }
            

            return rates;
        }

        Regex exp = new Regex( "\\{[^{}]*\"name\":\"(\\w*)\"[^{}]*\"bid\":\"([0-9.]*)\"[^{}]*\"ask\":\"([0-9.]*)\"[^{}]*\"group\":\"1\"[^{}]*[^{}]*\\}");
    }
}
