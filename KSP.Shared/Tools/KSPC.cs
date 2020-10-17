using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KSP.Shared.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KSP.Shared.Tools
{
    public class KSPC : IDisposable
    {
        public bool IsRunning { get; private set; }
        public bool Started { get; private set; }
        public bool AutoPilotEnabled { get; private set; }
        public double FuelMax { get; private set; }
        public double CurrentStageFuelMax { get; private set; }
        private Connection Connection { get; set; }

        public Service SpaceCenter
        {
            get => this.Connection.SpaceCenter();
        }

        public Vessel Vessel
        {
            get => this.SpaceCenter.ActiveVessel;
        }

        public AutoPilot AutoPilot
        {
            get => this.Vessel.AutoPilot;
        }

        public Control Control
        {
            get => this.Vessel.Control;
        }

        public Flight Flight
        {
            get => this.Vessel.Flight(this.Vessel.Orbit.Body.ReferenceFrame);
        }

        public Orbit Orbit
        {
            get => this.Vessel.Orbit;
        }

        private V3 DirectionRaw
        {
            // -1 to +1     navball
            // ---------------------------
            // +x = up      pitch/blue
            // -x = down    pitch/red
            // +y = north   heading/down
            // -y = south   heading/up
            // +z = west    heading/right
            // -z = east    heading/left
            get => this.Vessel.Direction(this.Vessel.SurfaceReferenceFrame).ToV3();
        }

        public V3 Direction
        {
            get
            {
                var direction = this.DirectionRaw;
                return new V3(direction.Z, -direction.Y, direction.X);
            }
            set
            {
                var targetPitch = (1 - value.SetZ(0).MagnitudeStretched) * 90 * (0 <= value.Z ? 1 : -1);
                var defaultHeading = new V3(0, -1, 0);
                var targetHeading = new V3(value.X, value.Y, 0);
                var targetRoll = 0;

                if (targetHeading == V3.Zero)
                {
                    targetHeading = defaultHeading;
                }

                this.TargetPitch = targetPitch;
                this.TargetHeading = V3.AngleBetweenVectors(defaultHeading, targetHeading, V3.Forward);
                this.TargetRoll = targetRoll;
            }
        }

        public double Pitch
        {
            get
            {
                var direction = this.DirectionRaw;
                var horizonDirection = direction.SetX(0);
                var pitch = V3.AngleBetweenVectors(direction, horizonDirection);

                if (direction.X < 0)
                {
                    pitch = -pitch;
                }
                return pitch;
            }
        }

        public double Heading
        {
            get
            {
                var direction = this.DirectionRaw;
                var horizonDirection = direction.SetX(0);
                var north = new V3(0, 1, 0);
                var heading = V3.AngleBetweenVectors(north, horizonDirection);

                if (horizonDirection.Z < 0)
                {
                    heading = 360 - heading;
                }
                return heading;
            }
        }

        public double Roll
        {
            get
            {
                var direction = this.DirectionRaw;
                var up = new V3(1, 0, 0);
                var east = new V3(0, 0, -1);
                var planeNormal = V3.CrossProduct(direction, up);
                var vesselUp = this.SpaceCenter.TransformDirection(east.ToTuple(), this.Vessel.ReferenceFrame, this.Vessel.SurfaceReferenceFrame).ToV3();
                var roll = V3.AngleBetweenVectors(vesselUp, planeNormal);

                if (vesselUp.X > 0)
                {
                    roll *= -1;
                }
                else if (roll < 0)
                {
                    roll += 180;
                }
                else
                {
                    roll -= 180;
                }
                return roll;
            }
        }

        public double TargetPitch
        {
            get => this.AutoPilot.TargetPitch;
            set => this.AutoPilot.TargetPitch = (float)value;
        }

        public double TargetHeading
        {
            get => this.AutoPilot.TargetHeading;
            set => this.AutoPilot.TargetHeading = (float)value;
        }

        public double TargetRoll
        {
            get => this.AutoPilot.TargetRoll;
            set => this.AutoPilot.TargetRoll = (float)value;
        }

        public double PitchError
        {
            get
            {
                var value = this.Pitch;
                var target = this.TargetPitch;
                var diff = Math.Abs(value - target);
                var max = 180;
                return diff / max;
            }
        }

        public double HeadingError
        {
            get
            {
                var value = this.Heading;
                var target = this.TargetHeading;
                var diff = Math.Abs(value - target);
                var max = 360;
                return diff / max;
            }
        }

        public double RollError
        {
            get
            {
                var value = this.Roll;
                var target = this.TargetRoll;
                var diff = Math.Abs(value - target);
                var max = 360;
                return diff / max;
            }
        }

        public double Altitude
        {
            get => this.Flight.SurfaceAltitude;
        }

        public double Apoapsis
        {
            get => Math.Max(this.Orbit.ApoapsisAltitude, default);
        }

        public double ApoapsisRelative
        {
            get => this.Apoapsis != 0 ? this.Altitude / this.Apoapsis : 0;
        }

        public double Periapsis
        {
            get => Math.Max(this.Orbit.PeriapsisAltitude, default);
        }

        public double PeriapsisRelative
        {
            get => Math.Max(this.Apoapsis, this.Periapsis) != 0 ? Math.Min(this.Apoapsis, this.Periapsis) / Math.Max(this.Apoapsis, this.Periapsis) : 0;
        }

        public double VerticalSpeed
        {
            get => this.Flight.VerticalSpeed;
        }

        private V3 VelocityRaw
        {
            get => this.Flight.Velocity.ToV3();
        }

        public V3 Velocity
        {
            get
            {
                var direction = this.VelocityRaw;
                return new V3(direction.X, -direction.Y, -direction.Z);
            }
        }

        public string Stages
        {
            get => string.Join(", ", this.Vessel.Parts.All.Select(x => x.Stage).Distinct().Where(x => x != -1).OrderBy(x => x));
        }

        public int CurrentStage
        {
            get => this.Control.CurrentStage;
        }

        public double LiquidFuel
        {
            get => this.Vessel.Resources.Amount("LiquidFuel");
        }

        public double SolidFuel
        {
            get => this.Vessel.Resources.Amount("SolidFuel");
        }

        public double Fuel
        {
            get => this.LiquidFuel + this.SolidFuel;
        }

        public double FuelRelative
        {
            get => this.FuelMax != 0 ? this.Fuel / this.FuelMax : 0;
        }

        public double CurrentStageFuel
        {
            get
            {
                var result = default(double);
                var stage = this.CurrentStage;

                foreach (var part in this.Vessel.Parts.All.Where(x => x.Stage == stage || x.DecoupleStage + 1 == stage))
                {
                    result += part.Resources.Amount("LiquidFuel");
                    result += part.Resources.Amount("SolidFuel");
                }

                return result;
            }
        }

        public double CurrentStageFuelRelative
        {
            get => this.CurrentStageFuelMax != 0 ? this.CurrentStageFuel / this.CurrentStageFuelMax : 0;
        }

        public KSPC()
        {
            this.IsRunning = false;
            this.Started = false;
            this.AutoPilotEnabled = false;
            this.FuelMax = default;
            this.CurrentStageFuelMax = default;
            this.Connection = null;
        }

        public void Run()
        {
            if (this.IsRunning == false)
            {
                this.IsRunning = true;
                this.Connection = new Connection();

                if (100 < this.Altitude)
                {
                    this.Started = true;
                    this.FuelMax = this.Fuel;
                    this.CurrentStageFuelMax = this.CurrentStageFuel;
                }
            }
        }

        public void Dispose()
        {
            if (this.IsRunning)
            {
                this.IsRunning = false;
                this.DisableAutoPilot();
                this.Connection.Dispose();
            }
        }

        public void EnableAutoPilot()
        {
            if (this.AutoPilotEnabled == false)
            {
                this.AutoPilotEnabled = true;
                this.AutoPilot.Engage();
            }
        }

        public void DisableAutoPilot()
        {
            if (this.AutoPilotEnabled)
            {
                this.AutoPilotEnabled = false;
                this.AutoPilot.Disengage();
            }
        }

        public void ToggleAutoPilot()
        {
            if (this.AutoPilotEnabled == false)
            {
                this.EnableAutoPilot();
            }
            else
            {
                this.DisableAutoPilot();
            }
        }

        public void ToggleSAS()
        {
            this.Control.SAS = !this.Control.SAS;
        }

        public void ToggleThrottle()
        {
            this.Control.Throttle = this.Control.Throttle != 1 ? 1 : 0;
        }

        public async Task ActivateNextStageAsync()
        {
            if (this.Started == false)
            {
                this.Started = true;
                this.FuelMax = this.Fuel;
                this.Control.ActivateNextStage();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            this.CurrentStageFuelMax = this.CurrentStageFuel;
            this.Control.ActivateNextStage();
        }
    }
}