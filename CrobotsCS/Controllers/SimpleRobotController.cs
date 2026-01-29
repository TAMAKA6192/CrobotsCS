namespace CrobotsCS.Controllers;

using CrobotsCS.Models;
using System;
using System.Collections.Generic;

public class SimpleRobotController : IRobotController
{
    private readonly string _name;
    private readonly Action<RobotApi> _behavior;
    private RobotApi? _api;

    public SimpleRobotController(string name, Action<RobotApi> behavior)
    {
        this._name = name;
        this._behavior = behavior;
    }

    public void Execute(Robot robot)
    {
        _api ??= new RobotApi(robot);
        _behavior(_api);
    }
}

public class RobotApi
{
    private readonly Robot _robot;
    private readonly Random _random = new();

    public RobotApi(Robot robot)
    {
        this._robot = robot;
    }

    public double X => _robot.Position.X;
    public double Y => _robot.Position.Y;
    public double Heading => _robot.Heading;
    public int Health => _robot.Health;

    public void Drive(double heading, double speed) => _robot.Drive(heading, speed);
    public int Scan(double direction, double resolution) => _robot.Scan(direction, resolution);
    public void Cannon(double direction, double range) => _robot.Cannon(direction, range);

    public int Random(int max) => _random.Next(max);
}
