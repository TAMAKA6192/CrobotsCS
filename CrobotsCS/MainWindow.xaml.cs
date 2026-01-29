namespace CrobotsCS;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using CrobotsCS.Controllers;
using CrobotsCS.Game;
using CrobotsCS.Models;
using CrobotsCS.Resources;

using Microsoft.Win32;

using Path = System.IO.Path;

public partial class MainWindow : Window {
    private readonly BattleField _battleField = new();
    private readonly DispatcherTimer _gameTimer = new();
    private readonly ObservableCollection<RobotViewModel> _robotViewModels = [];
#pragma warning disable CS0414 // フィールドは割り当てられていますがその値は使用されていません
    private bool _isRunning;
#pragma warning restore CS0414 // フィールドは割り当てられていますがその値は使用されていません

    public MainWindow() {
        InitializeComponent();
        InitializeGame();
        LoadDefaultRobots();
    }

    private void InitializeGame() {
        Title = Strings.AppTitle;
        StartButton.Content = Strings.StartButton;
        StopButton.Content = Strings.StopButton;
        ResetButton.Content = Strings.ResetButton;
        LoadRobotButton.Content = Strings.LoadRobotButton;
        StatusText.Text = Strings.Ready;

        _gameTimer.Tick += GameTimer_Tick;
        _battleField.RobotDestroyed += BattleField_RobotDestroyed;
        _battleField.BattleWon += BattleField_BattleWon;

        RobotListView.ItemsSource = _robotViewModels;
    }

    private void LoadDefaultRobots() {
        AddRobot("Sniper", Colors.Red, api => {
            api.Drive(api.Heading + api.Random(20) - 10, 2);
            _ = api.Scan(api.Heading, 10);
            if (api.Random(10) < 3) {
                api.Cannon(api.Heading, 500);
            }
        });

        AddRobot("Rook", Colors.Blue, api => {
            api.Drive(api.Heading, 3);
            if (api.X < 100 || api.X > 900 || api.Y < 100 || api.Y > 900) {
                api.Drive(api.Heading + 90, 3);
            }

            api.Cannon(api.Heading + api.Random(360), 500);
        });

        AddRobot("Rabbit", Colors.Green, api => {
            api.Drive(api.Random(360), 4);
            if (api.Random(10) < 2) {
                api.Cannon(api.Heading, 700);
            }
        });

        AddRobot("Counter", Colors.Orange, api => {
            api.Drive(api.Heading + 5, 2);
            _ = api.Scan(api.Heading + 180, 5);
            api.Cannon(api.Heading + 180, 500);
        });
    }

    private void AddRobot(string name, Color color, Action<RobotApi> behavior) {
        var random = Random.Shared;
        var position = new Point(
            random.Next(100, BattleField.Width - 100),
            random.Next(100, BattleField.Height - 100)
        );

        var controller = new SimpleRobotController(name, behavior);
        var robot = new Robot(name, position, color, controller);
        _battleField.AddRobot(robot);

        var viewModel = new RobotViewModel(robot);
        _robotViewModels.Add(viewModel);
    }

    private void StartButton_Click(object sender, RoutedEventArgs e) {
        if (_battleField.Robots.Count < 2) {
            _ = MessageBox.Show(Strings.MinimumTwoRobots, Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _isRunning = true;
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        LoadRobotButton.IsEnabled = false;
        StatusText.Text = Strings.BattleInProgress;

        var speed = (int)SpeedSlider.Value;
        _gameTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / speed);
        _gameTimer.Start();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e) => StopGame();

    private void StopGame() {
        _isRunning = false;
        _gameTimer.Stop();
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        LoadRobotButton.IsEnabled = true;
        StatusText.Text = Strings.Ready;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e) {
        StopGame();
        _battleField.Clear();
        _robotViewModels.Clear();
        BattleFieldCanvas.Children.Clear();
        LoadDefaultRobots();
        CycleText.Text = string.Format(Strings.Cycle, 0);
    }

    private void LoadRobotButton_Click(object sender, RoutedEventArgs e) {
        var dialog = new OpenFileDialog
        {
            Title = Strings.LoadRobotDialog,
            Filter = Strings.RobotScriptFilter
        };

        if (dialog.ShowDialog() == true) {
            try {
                var name = Path.GetFileNameWithoutExtension(dialog.FileName);
                AddRobot(name, GetRandomColor(), api => {
                    api.Drive(api.Random(360), 3);
                    if (api.Random(10) < 3) {
                        api.Cannon(api.Heading, 500);
                    }
                });
            } catch (Exception ex) {
                MessageBox.Show(string.Format(Strings.ErrorLoadingRobot, ex.Message), Strings.AppTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void GameTimer_Tick(object? sender, EventArgs e) {
        _battleField.Update();
        RenderBattleField();
        UpdateRobotViewModels();
        CycleText.Text = string.Format(Strings.Cycle, _battleField.CycleCount);

        foreach (var robot in _battleField.Robots.Where(r => r.IsAlive)) {
            if (Random.Shared.Next(100) < 5) {
                _battleField.FireMissile(robot);
            }
        }
    }

    private void RenderBattleField() {
        BattleFieldCanvas.Children.Clear();

        var scaleX = BattleFieldCanvas.ActualWidth / BattleField.Width;
        var scaleY = BattleFieldCanvas.ActualHeight / BattleField.Height;

        foreach (var missile in _battleField.Missiles) {
            var ellipse = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = Brushes.Black
            };
            System.Windows.Controls.Canvas.SetLeft(ellipse, (missile.Position.X * scaleX) - 2);
            System.Windows.Controls.Canvas.SetTop(ellipse, (missile.Position.Y * scaleY) - 2);
            _ = BattleFieldCanvas.Children.Add(ellipse);
        }

        foreach (var robot in _battleField.Robots.Where(r => r.IsAlive)) {
            var robotEllipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(robot.Color),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            System.Windows.Controls.Canvas.SetLeft(robotEllipse, (robot.Position.X * scaleX) - 10);
            System.Windows.Controls.Canvas.SetTop(robotEllipse, (robot.Position.Y * scaleY) - 10);
            _ = BattleFieldCanvas.Children.Add(robotEllipse);

            var turretLine = new Line
            {
                X1 = robot.Position.X * scaleX,
                Y1 = robot.Position.Y * scaleY,
                X2 = (robot.Position.X * scaleX) + (15 * Math.Cos(robot.TurretHeading * Math.PI / 180)),
                Y2 = (robot.Position.Y * scaleY) + (15 * Math.Sin(robot.TurretHeading * Math.PI / 180)),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            _ = BattleFieldCanvas.Children.Add(turretLine);
        }
    }

    private void UpdateRobotViewModels() {
        foreach (var vm in _robotViewModels) {
            vm.Refresh();
        }
    }

    private void BattleField_RobotDestroyed(object? sender, Robot robot) {
    }

    private void BattleField_BattleWon(object? sender, Robot winner) {
        StopGame();
        StatusText.Text = string.Format(Strings.Winner, winner.Name);
        MessageBox.Show(string.Format(Strings.Winner, winner.Name), Strings.BattleComplete,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static Color GetRandomColor() {
        var random = Random.Shared;
        return Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
    }
}

public class RobotViewModel(Robot robot) : INotifyPropertyChanged {
    public string Name => robot.Name;
    public int Health => robot.Health;
    public Brush ColorBrush => new SolidColorBrush(robot.Color);
    public string StatusText => robot.IsAlive ? Strings.Alive : Strings.Dead;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Health)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
    }
}
