using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using CurrencyRatesInterface;
using System.Reflection;
using System.IO;

namespace LotComputeMetroUI
{
    public partial class MainWindow
    {

        delegate void UpdateRatesDelegate();

        private System.Timers.Timer updateTmer = new System.Timers.Timer();
        private AbstractCurrencyRates currencyRates;
        private Dictionary<string, string> depositCurrencies = new Dictionary<string, string>() { 
            { "RUR", "P" },
            { "USD", "$" },
            { "EUR", "€" } };

        const int recomendedStressTestValue = 50;
        const int criticalStressTestValue = 2;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                currencyRates = LoadCurrencyRatesPlugin();

                VendorInformation info = GetVendorInfo( currencyRates.GetType() );
                if( info != null )
                    lCaption.Content += String.Format( " ({0}, v{1})", info.Provider, info.Version );

                currencyRates.Refresh();
                InitComboBoxes();
            }
            catch( System.Exception ex )
            {
                MessageBox.Show( ex.Message, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error );
                Thread.CurrentThread.Abort();
            }
        }

        private AbstractCurrencyRates LoadCurrencyRatesPlugin()
        {
            string[] libs = Directory.GetFiles( Directory.GetCurrentDirectory(), "*.dll" );

            foreach( var lib in libs )
            {
                Assembly AboutAssembly = Assembly.LoadFrom( lib );

                foreach( Type t in AboutAssembly.GetExportedTypes() )
                {
                    if( t.IsClass && !t.IsAbstract && typeof( AbstractCurrencyRates ).IsAssignableFrom( t ) )
                    {
                        AbstractCurrencyRates currencyRates = (AbstractCurrencyRates)Activator.CreateInstance( t );
                        return currencyRates;
                    }
                }
            }


            throw new Exception( "Не удалось загрузить провайдера котировок." );
        }

        private VendorInformation GetVendorInfo( System.Type t )
        {
            // Using reflection.
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes( t );  // Reflection.

            // Displaying output.
            foreach( System.Attribute attr in attrs )
            {
                if( attr is VendorInformation )
                {
                    return (VendorInformation)attr;
                }
            }

            return null;
        }

        private void UpdateData()
        {
            string pair = (string)cCurrencyPair.SelectedValue;
            var rate = currencyRates.GetPairRates( pair );

            tBid.Text = rate.Bid.ToString();
            tAsk.Text = rate.Ask.ToString();

            var spredSize = currencyRates.GetPairSpraed( pair );
            tSpread.Text = spredSize.ToString();

            var stopLoss = decimal.Parse( tSL.Text );
            var effectiveSL = stopLoss + spredSize;

            var lot = computeLotData( pair,
                stopLoss, decimal.Parse( tRisk.Text ) );
            var pipCost = ComputePipCost( pair ) * lot;

            tPipCost.Content = pipCost.ToString( "f2" );
            tOptimalLot.Content = lot.ToString();

            tMaxLose.Content = (effectiveSL * pipCost).ToString( "f2" );

            int stressTest = GetStressTestResults( decimal.Parse( tDeposit.Text ), pipCost * effectiveSL );
            tSurviveChance.Content = stressTest.ToString();

            VisualiseRisk( stressTest );
        }

        private void InitComboBoxes()
        {
            foreach( var name in currencyRates.GetPosiblePairs() )
            {
                cCurrencyPair.Items.Add( name );
            }

            foreach( var v in depositCurrencies )
            {
                cDepositCurrency.Items.Add( v.Key );
            }
        }

        private void InitializeForm()
        {
            LoadSettings();

            riskSlider.Value = double.Parse( tRisk.Text );

            cDepositCurrency.SelectionChanged += cDepositCurrency_SelectionChanged;
            tDeposit.TextChanged += TextChanged;

            cCurrencyPair.SelectionChanged += cCurrencyPair_SelectionChanged;

            tLotSize.TextChanged += TextChanged;
            tMinLot.TextChanged += TextChanged;
            tLotStep.TextChanged += TextChanged;

            tSL.TextChanged += TextChanged;
            tRisk.TextChanged += TextChanged;

            cDepositCurrency_SelectionChanged( null, null );
            UpdateData();

            updateTmer.Interval = 5000;
            updateTmer.Elapsed += TimeOut;
            updateTmer.Enabled = true;
        }

        void LoadSettings()
        {
            var s = Properties.Settings.Default;

            cDepositCurrency.SelectedValue = s.vrDepistiCurrency;
            tDeposit.Text = s.vrDeposit.ToString();

            cCurrencyPair.SelectedItem = s.vrCurrencyPair;

            tLotSize.Text = s.vrLotSize.ToString();
            tMinLot.Text = s.vrMinLot.ToString();
            tLotStep.Text = s.vrLotStep.ToString();

            tSL.Text = s.vrSL.ToString();
            tRisk.Text = s.vrRisk.ToString();
        }

        void SaveSettings()
        {
            var s = Properties.Settings.Default;

            s.vrDepistiCurrency = (string)cDepositCurrency.SelectedItem;
            s.vrDeposit = decimal.Parse( tDeposit.Text );

            s.vrCurrencyPair = (string)cCurrencyPair.SelectedItem;

            s.vrLotSize = long.Parse( tLotSize.Text );
            s.vrMinLot = decimal.Parse( tMinLot.Text );
            s.vrLotStep = decimal.Parse( tLotStep.Text );

            s.vrSL = short.Parse( tSL.Text );
            s.vrRisk = decimal.Parse( tRisk.Text );

            s.Save();
        }


        private void UpdateRates()
        {
            currencyRates.Refresh();
            UpdateData();
        }

        private void CleanTextBoxData( TextBox s )
        {
            s.Text = s.Text.Replace( ".", "," );

            try
            {
                if( decimal.Parse( s.Text ) < 0 )
                    throw null;

                UpdateData();
            }
            catch( Exception )
            {
                s.Text = "0";
            }
        }

        private decimal computeLotData( string pair, decimal StopLoss, decimal risk )
        {
            decimal pipCost = ComputePipCost( pair );

            var spredSize = currencyRates.GetPairSpraed( pair );
            var effectiveSL = StopLoss + spredSize;
            var point = currencyRates.GetPairPoint( pair );
            var loseCost = effectiveSL * point;

            decimal minLot = decimal.Parse( tMinLot.Text );
            decimal maxLose = (decimal.Parse( tDeposit.Text ) * risk) / 100;

            decimal lotStep = decimal.Parse( tLotStep.Text );

            decimal lot = maxLose / (pipCost * effectiveSL);

            try
            {
                lot = Math.Floor( lot / lotStep ) * lotStep;
            }
            catch( System.Exception )
            {
                lot = minLot;
            }


            if( lot < minLot )
                lot = minLot;

            return lot;
        }

        private static int GetStressTestResults( decimal deposit, decimal maxLose )
        {
            if( maxLose == 0 )
                return -1;

            return (int)Math.Floor( deposit / (2 * maxLose) ) + 1;
        }

        private decimal ComputePipCost( string pair )
        {
            var rate = currencyRates.GetPairRates( pair );
            var point = currencyRates.GetPairPoint( pair );

            decimal pipCost = 0;
            decimal lotSize = long.Parse( tLotSize.Text );

            if( pair.Contains( "USD" ) )
            {
                if( pair.EndsWith( "USD" ) )
                    pipCost = point * lotSize;
                else
                    pipCost = (point * lotSize) / rate.Ask;
            }
            else
            {
                var pairs = currencyRates.GetPosiblePairs();
                string rateCurrency = pair.Substring( 3 );

                foreach( var str in pairs )
                {
                    if( rateCurrency + "USD" == str )
                    {
                        pipCost = point * lotSize * currencyRates.GetPairRates( rateCurrency + "USD" ).Ask;
                    }
                    else if( "USD" + rateCurrency == str )
                    {
                        pipCost = (point * lotSize) / currencyRates.GetPairRates( "USD" + rateCurrency ).Ask;
                    }

                }
            }

            switch( (string)cDepositCurrency.SelectedValue )
            {
                case "RUR":
                    pipCost *= currencyRates.GetPairRates( "USDRUR" ).Bid;
                    break;
                case "EUR":
                    pipCost /= currencyRates.GetPairRates( "EURUSD" ).Bid;
                    break;
            }

            return pipCost;
        }
    }
}
