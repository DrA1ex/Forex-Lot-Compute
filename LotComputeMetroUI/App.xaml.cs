using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace LotComputeMetroUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
#if WHITE_STYLE
            StartupUri = new System.Uri("/LotComputeMetroUi;component/MainWindowWhite.xaml", UriKind.Relative);
#else
            StartupUri = new System.Uri("/LotComputeMetroUi;component/MainWindowBlack.xaml", UriKind.Relative);
#endif
        }
    }
}
