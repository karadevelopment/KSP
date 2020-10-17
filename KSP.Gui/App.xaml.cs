using KSP.Gui.Views;
using System.Windows;

namespace KSP.Gui
{
    public partial class App
    {
        public App()
        {
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainView();
        }
    }
}