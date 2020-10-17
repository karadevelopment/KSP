using KSP.Gui.Views;
using KSP.Shared;
using KSP.Shared.Modules;
using KSP.Shared.Tools;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace KSP.Gui.ViewModels
{
    public class MainViewModel
    {
        private MainView View { get; }
        private KSPC KSPC { get; }
        private int DirectionIndex { get; set; }

        private Brush BrushGreen
        {
            get => Brushes.Green;
        }

        private Brush BrushRed
        {
            get => Brushes.Red;
        }

        private V3 Direction
        {
            get
            {
                switch (this.DirectionIndex)
                {
                    case 1:
                        return new V3(0, 0, 1);
                    case 2:
                        return new V3(.5, 0, .5);
                    case 3:
                        return new V3(1, 0, 0);
                    case 4:
                        return this.KSPC.Velocity.NormalizeStretch;
                    case 5:
                        return this.KSPC.Velocity.NormalizeStretch.Inverse;
                    default:
                        return V3.Zero;
                }
            }
        }

        public MainViewModel(MainView view)
        {
            this.View = view;
            this.KSPC = new KSPC();
            this.DirectionIndex = 1;
        }

        public void Run()
        {
            this.KSPC.Run();

            Task.Run(async () =>
            {
                while (this.KSPC.IsRunning)
                {
                    this.KSPC.Direction = this.Direction;
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            });
            Task.Run(async () =>
            {
                while (this.KSPC.IsRunning)
                {
                    this.View.Dispatcher.Invoke(() =>
                    {
                        this.UpdateComponents();
                    });
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            });
        }

        public void Dispose()
        {
            this.KSPC.Dispose();
        }

        public void UpdateComponents()
        {
            this.View.ProgressBarAutoPilot.Background = this.KSPC.AutoPilotEnabled ? this.BrushGreen : this.BrushRed;
            this.View.ProgressBarSAS.Background = this.KSPC.Control.SAS ? this.BrushGreen : this.BrushRed;
            this.View.ProgressBarThrottle.Background = this.KSPC.Control.Throttle == 1 ? this.BrushGreen : this.BrushRed;
            this.View.TextBlockThrottle.Text = $"{this.KSPC.Control.Throttle:N1}";
            this.View.TextBlockDirection1.Text = $"{this.KSPC.Direction}";
            this.View.TextBlockDirection2.Text = $"{this.Direction}";

            this.View.TextBlockStages.Text = $"{this.KSPC.Stages}";
            this.View.TextBlockCurrentStage.Text = $"{this.KSPC.CurrentStage}";

            this.View.ProgressBarApoapsis.Background = this.BrushRed;
            this.View.ProgressBarApoapsis.Foreground = this.BrushGreen;
            this.View.ProgressBarApoapsis.Value = this.KSPC.ApoapsisRelative * 100;
            this.View.TextBlockAltitude.Text = $"{this.KSPC.Altitude:N0}";
            this.View.TextBlockApoapsis1.Text = $"{this.KSPC.Apoapsis:N0}";
            this.View.ProgressBarPeriapsis.Background = this.BrushRed;
            this.View.ProgressBarPeriapsis.Foreground = this.BrushGreen;
            this.View.ProgressBarPeriapsis.Value = this.KSPC.PeriapsisRelative * 100;
            this.View.TextBlockApoapsis2.Text = $"{this.KSPC.Apoapsis:N0}";
            this.View.TextBlockPeriapsis.Text = $"{this.KSPC.Periapsis:N0}";

            this.View.ProgressBarFuel.Background = this.BrushRed;
            this.View.ProgressBarFuel.Foreground = this.BrushGreen;
            this.View.ProgressBarFuel.Value = this.KSPC.FuelRelative * 100;
            this.View.TextBlockFuel.Text = $"{this.KSPC.Fuel:N0}";
            this.View.TextBlockFuelMax.Text = $"{this.KSPC.FuelMax:N0}";
            this.View.ProgressBarCurrentStageFuel.Background = this.BrushRed;
            this.View.ProgressBarCurrentStageFuel.Foreground = this.BrushGreen;
            this.View.ProgressBarCurrentStageFuel.Value = this.KSPC.CurrentStageFuelRelative * 100;
            this.View.TextBlockCurrentStageFuel.Text = $"{this.KSPC.CurrentStageFuel:N0}";
            this.View.TextBlockCurrentStageFuelMax.Text = $"{this.KSPC.CurrentStageFuelMax:N0}";

            this.View.TextBlockPrograde.Text = $"{this.KSPC.Flight.Prograde.ToV3()}";
            this.View.TextBlockRetrograde.Text = $"{this.KSPC.Flight.Retrograde.ToV3()}";

            this.View.TextBlockPitch1.Foreground = this.KSPC.PitchError < .1 ? this.BrushGreen : this.BrushRed;
            this.View.TextBlockPitch1.Text = $"{this.KSPC.Pitch:N0}";
            this.View.TextBlockPitch2.Text = $"{this.KSPC.TargetPitch:N0}";
            this.View.TextBlockHeading1.Foreground = this.KSPC.HeadingError < .1 ? this.BrushGreen : this.BrushRed;
            this.View.TextBlockHeading1.Text = $"{this.KSPC.Heading:N0}";
            this.View.TextBlockHeading2.Text = $"{this.KSPC.TargetHeading:N0}";
            this.View.TextBlockRoll1.Foreground = this.KSPC.RollError < .1 ? this.BrushGreen : this.BrushRed;
            this.View.TextBlockRoll1.Text = $"{this.KSPC.Roll:N0}";
            this.View.TextBlockRoll2.Text = $"{this.KSPC.TargetRoll:N0}";

            this.View.TextBlockVelocity1.Text = $"{this.KSPC.Velocity}";
            this.View.TextBlockVelocity2.Text = $"{this.KSPC.Velocity.Normalize}";
            this.View.TextBlockVerticalSpeed.Text = $"{this.KSPC.VerticalSpeed:N0}";
        }

        public async Task HandleKeyAsync(Key key)
        {
            switch (key)
            {
                case Key.Escape:
                    this.View.Close();
                    break;
                case Key.F1:
                    this.KSPC.ToggleAutoPilot();
                    break;
                case Key.F2:
                    this.KSPC.ToggleSAS();
                    break;
                case Key.F3:
                    this.KSPC.ToggleThrottle();
                    break;
                case Key.D1:
                    this.DirectionIndex = 1;
                    break;
                case Key.D2:
                    this.DirectionIndex = 2;
                    break;
                case Key.D3:
                    this.DirectionIndex = 3;
                    break;
                case Key.D4:
                    this.DirectionIndex = 4;
                    break;
                case Key.D5:
                    this.DirectionIndex = 5;
                    break;
                case Key.Space:
                    await this.KSPC.ActivateNextStageAsync();
                    break;
            }
        }
    }
}