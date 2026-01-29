namespace CrobotsCS.Models;

using System;
using System.Windows;

public class Robot {
    private const int MaxHealth = 100;
    private const double MaxSpeed = 5.0;
    private const double TurretRotationSpeed = 10.0;
    private const int ScanRange = 700;
    private const int MissileRange = 700;
    private const int MissileDamage = 10;

    public string Name { get; set; } = string.Empty;
    public Point Position { get; set; }
    public double Heading { get; set; }
    public double TurretHeading { get; set; }
    public int Health { get; set; } = MaxHealth;
    public double Speed { get; set; }
    public bool IsAlive => Health > 0;
    public System.Windows.Media.Color Color { get; set; }

    private double _targetSpeed;
    private double _targetHeading;
    private double _targetTurretHeading;
    private readonly IRobotController? _controller;

    public Robot(string name, Point startPosition, System.Windows.Media.Color color, IRobotController? controller = null) {
        Name = name;
        Position = startPosition;
        Color = color;
        Heading = Random.Shared.Next(360);
        TurretHeading = Heading;
        this._controller = controller;
    }

    public void Update() {
        if (!IsAlive) {
            return;
        }

        _controller?.Execute(this);

        Speed = Math.Clamp(Speed + (Math.Sign(_targetSpeed - Speed) * 0.5), 0, MaxSpeed);
        Heading = NormalizeAngle(Heading + Math.Clamp(_targetHeading - Heading, -5, 5));
        TurretHeading = NormalizeAngle(TurretHeading + Math.Clamp(_targetTurretHeading - TurretHeading, -TurretRotationSpeed, TurretRotationSpeed));

        var radians = Heading * Math.PI / 180.0;
        Position = new Point(
            Position.X + (Speed * Math.Cos(radians)),
            Position.Y + (Speed * Math.Sin(radians))
        );
    }

    public void Drive(double heading, double speed) {
        _targetHeading = NormalizeAngle(heading);
        _targetSpeed = Math.Clamp(speed, 0, MaxSpeed);
    }

    public int Scan(double direction, double resolution) {
        _targetTurretHeading = NormalizeAngle(direction);
        return 0;
    }

    public bool Cannon(double direction, double range) {
        _targetTurretHeading = NormalizeAngle(direction);
        return true;
    }

    public void TakeDamage(int damage) => Health = Math.Max(0, Health - damage);

    public double DistanceTo(Robot other) {
        var dx = Position.X - other.Position.X;
        var dy = Position.Y - other.Position.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    public double AngleTo(Robot other) {
        var dx = other.Position.X - Position.X;
        var dy = other.Position.Y - Position.Y;
        return NormalizeAngle(Math.Atan2(dy, dx) * 180.0 / Math.PI);
    }

    private static double NormalizeAngle(double angle) {
        angle %= 360;
        if (angle < 0) {
            angle += 360;
        }

        return angle;
    }
}
