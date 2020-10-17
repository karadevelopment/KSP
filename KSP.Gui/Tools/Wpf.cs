using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace KPS.Gui.Tools
{
    public static class Wpf
    {
        private static List<Window> Windows { get; } = new List<Window>();

        public static bool AnyWindowAlive
        {
            get => 0 < Wpf.Windows.Count;
        }

        public static void Invoke(Action callback)
        {
            Application.Current.Dispatcher.Invoke(callback);
        }

        public static void Start<T>(this T param) where T : Window, IComponentConnector
        {
            param.DataContext = param;

            param.Initialized += (sender, e) =>
            {
                lock (Wpf.Windows)
                {
                    Wpf.Windows.Add(param);
                }
            };
            param.Closing += (sender, e) =>
            {
                lock (Wpf.Windows)
                {
                    Wpf.Windows.Remove(param);
                }
            };

            var topMost = param.Topmost;
            param.Initialized += (sender, e) =>
            {
                param.Topmost = true;
            };
            param.ContentRendered += (sender, e) =>
            {
                param.Topmost = topMost;
            };
            param.InitializeComponent();
            param.Show();

            Task.Run(() =>
            {
                param.Dispatcher.Invoke(() =>
                {
                    param.Activate();
                    param.Focus();
                });
            });
        }

        public static void Start(this IComponentConnector param)
        {
            param.InitializeComponent();
        }
    }
}