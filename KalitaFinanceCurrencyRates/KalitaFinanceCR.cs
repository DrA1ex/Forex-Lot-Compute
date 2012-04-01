using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CurrencyRatesInterface;
using HtmlAgilityPack;

namespace KalitaFinanceCurrencyRates
{

    [VendorInformation( Provider = "Kalita Finance",
        Version = "1.0",
        Vendor = "A1ex Inc" )]
    public class KalitaFinanceCurrencyRates : AbstractCurrencyRates
    {
        protected override Dictionary<string, Rate> LoadRates()
        {
            Dictionary<string, Rate> rates = new Dictionary<string, Rate>();

            WebClient wClient = new WebClient();
            wClient.Proxy = null;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(wClient.DownloadString("http://www.kalita-finance.ru/outer/quotations.php"));

            var tableRows = document.DocumentNode.SelectNodes("//table/tr[@class]");

            var parseFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

            foreach (var row in tableRows)
            {
                string name;
                Rate rate;

                var tableData = row.SelectNodes("td");

                name = tableData[0].InnerText;

                if (name.Length != 6)
                    continue;

                rate.Bid = Convert.ToDecimal(tableData[1].InnerText, parseFormat);
                rate.Ask = Convert.ToDecimal(tableData[3].InnerText, parseFormat);

                rates.Add(name, rate);
            }

            return rates;
        }
    }
}
