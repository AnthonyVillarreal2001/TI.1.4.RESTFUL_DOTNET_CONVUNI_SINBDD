using System;
using System.Windows.Forms;
using CLIESC_CONVUNI_DOTNET_GR69.Views;

namespace CLIESC_CONVUNI_DOTNET_GR69
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
