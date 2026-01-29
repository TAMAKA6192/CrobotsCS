namespace CrobotsCS.Game;

using CrobotsCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

public class BattleField
{
    public const int Width = 1000;
    public const int Height = 1000;

    private readonly List<Robot> robots = [];
    private readonly List<Missile> missiles = [];
    private int cycleCount;

    public IReadOnlyList<Robot> Robots => robots.AsReadOnly();
    public IReadOnlyList<Missile> Missiles => missiles.AsReadOnly();
    public int CycleCount => cycleCount;

    public event EventHandler<Robot>? RobotDestroyed;
    public event EventHandler<Robot>? BattleWon;

    public void AddRobot(Robot robot)
    {
        ArgumentNullException.ThrowIfNull(robot);
        robots.Add(robot);
    }

    public void Clear()
    {
        robots.Clear();
        missiles.Clear();
        cycleCount = 0;
    }

    public void Update()
    {
        cycleCount++;

        foreach (var robot in robots)
        {
            robot.Update();
            ConstrainRobotPosition(robot);
        }

        ProcessMissiles();
        CheckForWinner();
    }

    private void ConstrainRobotPosition(Robot robot)
    {
        var x = Math.Clamp(robot.Position.X, 0, Width);
        var y = Math.Clamp(robot.Position.Y, 0, Height);
        robot.Position = new Point(x, y);
    }

    private void ProcessMissiles()
    {
        foreach (var missile in missiles.ToList())
        {
            if (!missile.IsActive)
                continue;

            missile.Update();

            if (missile.Position.X < 0 || missile.Position.X > Width ||
                missile.Position.Y < 0 || missile.Position.Y > Height)
            {
                missile.IsActive = false;
                continue;
            }

            foreach (var robot in robots)
            {
                if (missile.CheckCollision(robot))
                {
                    robot.TakeDamage(missile.Damage);
                    missile.IsActive = false;

                    if (!robot.IsAlive)
                    {
                        RobotDestroyed?.Invoke(this, robot);
                    }
                    break;
                }
            }
        }

        missiles.RemoveAll(m => !m.IsActive);
    }

    private void CheckForWinner()
    {
        var aliveRobots = robots.Where(r => r.IsAlive).ToList();
        if (aliveRobots.Count == 1)
        {
            BattleWon?.Invoke(this, aliveRobots[0]);
        }
    }

    public void FireMissile(Robot robot)
    {
        if (!robot.IsAlive)
            return;

        var missile = new Missile(robot, robot.Position, robot.TurretHeading);
        missiles.Add(missile);
    }

    public Robot? ScanForTarget(Robot scanner, double direction, double resolution)
    {
        var scanAngleStart = direction - resolution / 2;
        var scanAngleEnd = direction + resolution / 2;

        return robots
            .Where(r => r != scanner && r.IsAlive)
            .Select(r => new { Robot = r, Angle = scanner.AngleTo(r), Distance = scanner.DistanceTo(r) })
            .Where(x => x.Distance < 700 && IsAngleInRange(x.Angle, scanAngleStart, scanAngleEnd))
            .OrderBy(x => x.Distance)
            .Select(x => x.Robot)
            .FirstOrDefault();
    }

    private static bool IsAngleInRange(double angle, double start, double end)
    {
        angle = NormalizeAngle(angle);
        start = NormalizeAngle(start);
        end = NormalizeAngle(end);

        if (start <= end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }

    private static double NormalizeAngle(double angle)
    {
        angle %= 360;
        if (angle < 0)
            angle += 360;
        return angle;
    }
}
