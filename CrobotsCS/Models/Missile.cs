namespace CrobotsCS.Models;

using System;
using System.Windows;

public class Missile(Robot owner, Point position, double heading) {
    public Point Position { get; set; } = position;
    public double Heading { get; set; } = heading;
    public double Speed { get; } = 10.0;
    public Robot Owner { get; } = owner;
    public int Damage { get; } = 10;
    public bool IsActive { get; set; } = true;

    public void Update() {
        if (!IsActive) {
            return;
        }

        var radians = Heading * Math.PI / 180.0;
        Position = new Point(
            Position.X + (Speed * Math.Cos(radians)),
            Position.Y + (Speed * Math.Sin(radians))
        );
    }

    public bool CheckCollision(Robot target, double collisionDistance = 10.0) {
        if (target == Owner || !target.IsAlive) {
            return false;
        }

        var dx = Position.X - target.Position.X;
        var dy = Position.Y - target.Position.Y;
        var distance = Math.Sqrt((dx * dx) + (dy * dy));

        return distance < collisionDistance;
    }
}
