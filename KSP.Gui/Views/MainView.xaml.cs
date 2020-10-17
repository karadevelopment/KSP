using KPS.Gui.Tools;
using KSP.Gui.ViewModels;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace KSP.Gui.Views
{
    public partial class MainView
    {
        private MainViewModel ViewModel { get; }

        public MainView()
        {
            this.ViewModel = new MainViewModel(this);
            this.Start();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.ViewModel.Run();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.ViewModel.Dispose();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            await this.ViewModel.HandleKeyAsync(e.Key);
        }
    }
}