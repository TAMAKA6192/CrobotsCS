namespace CrobotsCS.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using CrobotsCS.Models;

public class BattleField {
    public const int Width = 1000;
    public const int Height = 1000;

    private readonly List<Robot> _robots = [];
    private readonly List<Missile> _missiles = [];

    public IReadOnlyList<Robot> Robots => _robots.AsReadOnly();
    public IReadOnlyList<Missile> Missiles => _missiles.AsReadOnly();
    public int CycleCount { get; private set; }

    public event EventHandler<Robot>? RobotDestroyed;
    public event EventHandler<Robot>? BattleWon;

    public void AddRobot(Robot robot) {
        ArgumentNullException.ThrowIfNull(robot);
        _robots.Add(robot);
    }

    public void Clear() {
        _robots.Clear();
        _missiles.Clear();
        CycleCount = 0;
    }

    public void Update() {
        CycleCount++;

        foreach (var robot in _robots) {
            robot.Update();
            ConstrainRobotPosition(robot);
        }

        ProcessMissiles();
        CheckForWinner();
    }

    private static void ConstrainRobotPosition(Robot robot) {
        var x = Math.Clamp(robot.Position.X, 0, Width);
        var y = Math.Clamp(robot.Position.Y, 0, Height);
        robot.Position = new Point(x, y);
    }

    private void ProcessMissiles() {
        foreach (var missile in _missiles.ToList()) {
            if (!missile.IsActive) {
                continue;
            }

            missile.Update();

            if (missile.Position.X < 0 || missile.Position.X > Width ||
                missile.Position.Y < 0 || missile.Position.Y > Height) {
                missile.IsActive = false;
                continue;
            }

            foreach (var robot in _robots) {
                if (missile.CheckCollision(robot)) {
                    robot.TakeDamage(missile.Damage);
                    missile.IsActive = false;

                    if (!robot.IsAlive) {
                        RobotDestroyed?.Invoke(this, robot);
                    }

                    break;
                }
            }
        }

        _ = _missiles.RemoveAll(m => !m.IsActive);
    }

    private void CheckForWinner() {
        var aliveRobots = _robots.Where(r => r.IsAlive).ToList();
        if (aliveRobots.Count == 1) {
            BattleWon?.Invoke(this, aliveRobots[0]);
        }
    }

    public void FireMissile(Robot robot) {
        if (!robot.IsAlive) {
            return;
        }

        var missile = new Missile(robot, robot.Position, robot.TurretHeading);
        _missiles.Add(missile);
    }

    public Robot? ScanForTarget(Robot scanner, double direction, double resolution) {
        var scanAngleStart = direction - (resolution / 2);
        var scanAngleEnd = direction + (resolution / 2);

        return _robots
            .Where(r => r != scanner && r.IsAlive)
            .Select(r => new { Robot = r, Angle = scanner.AngleTo(r), Distance = scanner.DistanceTo(r) })
            .Where(x => x.Distance < 700 && IsAngleInRange(x.Angle, scanAngleStart, scanAngleEnd))
            .OrderBy(x => x.Distance)
            .Select(x => x.Robot)
            .FirstOrDefault();
    }

    private static bool IsAngleInRange(double angle, double start, double end) {
        angle = NormalizeAngle(angle);
        start = NormalizeAngle(start);
        end = NormalizeAngle(end);

        return start <= end ? angle >= start && angle <= end : angle >= start || angle <= end;
    }

    private static double NormalizeAngle(double angle) {
        angle %= 360;
        if (angle < 0) {
            angle += 360;
        }

        return angle;
    }
}
