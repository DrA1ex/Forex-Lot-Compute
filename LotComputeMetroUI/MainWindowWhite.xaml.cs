#if WHITE_STYLE

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media;

namespace LotComputeMetroUI
{
    public partial class MainWindow : Window
    {
        const byte RED = 48;
        const byte GREEN = 171;
        const byte BLUE = 48;

        private Color RecomendedRiskColor = Color.FromRgb(RED, GREEN, BLUE);
        private Color UnrecomendedRiskColor = Color.FromRgb(GREEN, RED, BLUE);

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeForm();
        }

        private void HandleClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void HandleMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void TimeOut(object sender, ElapsedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(
                 delegate
                 {
                     Dispatcher.Invoke(DispatcherPriority.Normal, (UpdateRatesDelegate)UpdateRates);
                 }
                 ));
            t.Start();
        }

        private void cDepositCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lCurrencySymbol.Content = depositCurrencies[(string)cDepositCurrency.SelectedValue];

            UpdateData();
        }

        private void cCurrencyPair_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            CleanTextBoxData((TextBox)sender);
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        void VisualiseRisk(int stressTest)
        {
            double fraction = (double)(stressTest - criticalStressTestValue) / recomendedStressTestValue;

            if (fraction > 1)
                fraction = 1;
            else if (fraction < 0)
                fraction = 0;

            Color color = Color.FromArgb(255,
                (byte)(RecomendedRiskColor.R * fraction + UnrecomendedRiskColor.R * (1 - fraction)),
                (byte)(RecomendedRiskColor.G * fraction + UnrecomendedRiskColor.G * (1 - fraction)),
                (byte)(RecomendedRiskColor.B * fraction + UnrecomendedRiskColor.B * (1 - fraction)));

            gResult.Fill = new SolidColorBrush(color);
        }
    }
}

#endif