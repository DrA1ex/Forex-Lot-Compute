using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyRatesInterface
{
    /// <summary>
    /// Абстрактный класс для провайдера котировок. 
    /// В списке пар обязательно должны присутсвовать пары
    /// с валютами депозита. На данный момент это RUR, EUR, USD
    /// Т.е. обязательными должны быть следубщие пары:
    /// EURUSD,
    /// USDRUR
    /// </summary>
    public abstract class AbstractCurrencyRates
    {
        enum Errors { UnknownPair, CannotLoadData };
        static private Dictionary<Errors, string> errorStrings = new Dictionary<Errors, string>
        {
            {Errors.UnknownPair, "Данные о паре \"{0}\" не найдены!"},
            {Errors.CannotLoadData, "Не удалось загрузить данные. {0}" }
        };


        public string[] GetPosiblePairs()
        {
            return rates.Keys.ToArray();
        }

        public Rate GetPairRates( string pair )
        {
            try
            {
                return rates[pair];
            }
            catch( Exception )
            {
                throw new Exception( String.Format( errorStrings[Errors.UnknownPair], pair ) );
            }
        }

        public decimal GetPairSpraed( string pair )
        {
            try
            {
                var rate = rates[pair];
                return (rate.Ask - rate.Bid) / GetPairPoint( pair );
            }
            catch( Exception )
            {
                throw new Exception( String.Format( errorStrings[Errors.UnknownPair], pair ) );
            }
        }

        public decimal GetPairPoint( string pair )
        {
            try
            {
                var rate = rates[pair];
                string tmp = (rate.Ask + rate.Bid).ToString();

                int precision = tmp.Length - tmp.IndexOf( ',' ) - 1;

                decimal point = Convert.ToDecimal( Math.Pow( 10, -precision ) );
                return point;
            }
            catch( System.Exception )
            {
                throw new Exception( String.Format( errorStrings[Errors.UnknownPair], pair ) );
            }
        }

        public void Refresh()
        {
            try
            {
                rates = LoadRates();
            }
            catch( System.Exception ex )
            {
                throw new Exception( String.Format( errorStrings[Errors.CannotLoadData], ex.Message ) );
            }
        }

        protected abstract Dictionary<string, Rate> LoadRates();

        private Dictionary<string, Rate> rates = new Dictionary<string, Rate>();
    }

    public struct Rate
    {
        public Rate( decimal bid = 0, decimal ask = 0 )
        {
            Bid = bid;
            Ask = ask;
        }

        public decimal Bid;
        public decimal Ask;
    }

    public class VendorInformation : Attribute
    {
        public string Provider, Vendor, Version;
    }
}
