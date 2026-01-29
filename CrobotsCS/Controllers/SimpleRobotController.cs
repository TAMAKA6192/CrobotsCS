namespace CrobotsCS.Controllers;

using System;

using CrobotsCS.Models;

public class SimpleRobotController(string name, Action<RobotApi> behavior) : IRobotController {
    private readonly string _name = name;
    private RobotApi? _api;

    public void Execute(Robot robot) {
        _api ??= new RobotApi(robot);
        behavior(_api);
    }
}

public class RobotApi(Robot robot) {
    private readonly Random _random = new();

    public double X => robot.Position.X;
    public double Y => robot.Position.Y;
    public double Heading => robot.Heading;
    public int Health => robot.Health;

    public void Drive(double heading, double speed) => robot.Drive(heading, speed);
    public int Scan(double direction, double resolution) => robot.Scan(direction, resolution);
    public void Cannon(double direction, double range) => robot.Cannon(direction, range);

    public int Random(int max) => _random.Next(max);
}
