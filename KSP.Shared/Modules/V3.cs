using System;
using System.Collections.Generic;
using System.Linq;

namespace KSP.Shared.Modules
{
    public class V3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static V3 Zero
        {
            get => new V3();
        }

        public static V3 One
        {
            get => new V3(1, 1, 1);
        }

        public static V3 Right
        {
            get => new V3(1, 0, 0);
        }

        public static V3 Up
        {
            get => new V3(0, 1, 0);
        }

        public static V3 Forward
        {
            get => new V3(0, 0, 1);
        }

        public static V3 Left
        {
            get => new V3(-1, 0, 0);
        }

        public static V3 Down
        {
            get => new V3(0, -1, 0);
        }

        public static V3 Back
        {
            get => new V3(0, 0, -1);
        }

        public double Dot
        {
            get => this.X + this.Y + this.Z;
        }

        public V3 Inverse
        {
            get => new V3(-this.X, -this.Y, -this.Z);
        }

        public double Min
        {
            get => new List<double>
            {
                this.X,
                this.Y,
                this.Z,
            }.Min();
        }

        public double Max
        {
            get => new List<double>
            {
                this.X,
                this.Y,
                this.Z,
            }.Max();
        }

        public V3 Abs
        {
            get => new V3(Math.Abs(this.X), Math.Abs(this.Y), Math.Abs(this.Z));
        }

        public double AbsMin
        {
            get => new List<double>
            {
                Math.Abs(this.X),
                Math.Abs(this.Y),
                Math.Abs(this.Z),
            }.Min();
        }

        public double AbsMax
        {
            get => new List<double>
            {
                Math.Abs(this.X),
                Math.Abs(this.Y),
                Math.Abs(this.Z),
            }.Max();
        }

        public double Magnitude
        {
            get => Math.Sqrt(V3.DotProduct(this, this));
        }

        public double MagnitudeStretched
        {
            get
            {
                var m1 = this.Magnitude;
                var m2 = this.NormalizeStretch.Magnitude;
                return m2 != 0 ? m1 / m2 : 0;
            }
        }

        public V3 Normalize
        {
            get
            {
                var length = this.Magnitude;
                var x = length != 0 ? this.X / length : 0;
                var y = length != 0 ? this.Y / length : 0;
                var z = length != 0 ? this.Z / length : 0;
                return new V3(x, y, z);
            }
        }

        public V3 NormalizeStretch
        {
            get
            {
                var u = this.AbsMax;
                var x = u != 0 ? this.X / u : 0;
                var y = u != 0 ? this.Y / u : 0;
                var z = u != 0 ? this.Z / u : 0;
                return new V3(x, y, z);
            }
        }

        public V3()
        {
            this.X = default;
            this.Y = default;
            this.Z = default;
        }

        public V3(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public V3 SetX(float value)
        {
            return new V3(value, this.Y, this.Z);
        }

        public V3 SetY(float value)
        {
            return new V3(this.X, value, this.Z);
        }

        public V3 SetZ(float value)
        {
            return new V3(this.X, this.Y, value);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"({this.X:N1}, {this.Y:N1}, {this.Z:N1})";
        }

        public static V3 operator +(V3 v1, V3 v2)
        {
            return new V3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static V3 operator -(V3 v1, V3 v2)
        {
            return new V3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static V3 operator *(V3 v1, V3 v2)
        {
            return new V3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static V3 operator /(V3 v1, V3 v2)
        {
            return new V3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static bool operator ==(V3 v1, V3 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(V3 v1, V3 v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public static V3 CrossProduct(V3 v1, V3 v2)
        {
            var x = v1.Y * v2.Z - v1.Z * v2.Y;
            var y = v1.Z * v2.X - v1.X * v2.Z;
            var z = v1.X * v2.Y - v1.Y * v2.X;
            return new V3(x, y, z);
        }

        public static double DotProduct(V3 v1, V3 v2)
        {
            return (v1 * v2).Dot;
        }

        public static double AngleBetweenVectors(V3 v1, V3 v2)
        {
            var f1 = V3.DotProduct(v1, v2);
            var f2 = v1.Magnitude * v2.Magnitude;
            var f = f2 != 0 ? f1 / f2 : 0;
            return Math.Acos(f) * (180 / Math.PI);
        }

        public static double AngleBetweenVectors(V3 v1, V3 v2, V3 axis)
        {
            var angle = V3.SignedAngleBetweenVectors(v1, v2, axis);

            while (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }

        public static double SignedAngleBetweenVectors(V3 v1, V3 v2, V3 axis)
        {
            var v1n = v1.Normalize;
            var v2n = v2.Normalize;

            var dot = V3.DotProduct(v1n, v2n);
            var cross = V3.CrossProduct(v1n, v2n);

            var angle = Math.Acos(dot) * (180 / Math.PI);
            var axisDot = V3.DotProduct(axis, cross);

            if (axisDot < 0)
            {
                angle *= -1;
            }
            return angle;
        }
    }
}