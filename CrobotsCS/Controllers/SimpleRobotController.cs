namespace CrobotsCS.Controllers;

using CrobotsCS.Models;
using System;
using System.Collections.Generic;

public class SimpleRobotController : IRobotController
{
    private readonly string name;
    private readonly Action<RobotApi> behavior;
    private RobotApi? api;

    public SimpleRobotController(string name, Action<RobotApi> behavior)
    {
        this.name = name;
        this.behavior = behavior;
    }

    public void Execute(Robot robot)
    {
        api ??= new RobotApi(robot);
        behavior(api);
    }
}

public class RobotApi
{
    private readonly Robot robot;
    private readonly Random random = new();

    public RobotApi(Robot robot)
    {
        this.robot = robot;
    }

    public double X => robot.Position.X;
    public double Y => robot.Position.Y;
    public double Heading => robot.Heading;
    public int Health => robot.Health;

    public void Drive(double heading, double speed) => robot.Drive(heading, speed);
    public int Scan(double direction, double resolution) => robot.Scan(direction, resolution);
    public void Cannon(double direction, double range) => robot.Cannon(direction, range);

    public int Random(int max) => random.Next(max);
}
