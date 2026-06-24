using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NetworkService.Models;
using NetworkService.ViewModels;

namespace NetworkService.Views
{
    /// <summary>
    /// Draws graph type G3 - circles of different radii arranged along the time
    /// axis. The radius of each circle is proportional to the measured value, the
    /// centres are aligned on a common horizontal line (shifted down by the
    /// largest radius), and the time of each measurement is labelled on the X-axis.
    /// Valid and invalid values use different colours. The graph is drawn purely
    /// programmatically (no ready-made chart control) and refreshes in real time.
    /// </summary>
    public partial class MeasurementGraphView : UserControl
    {
        private MeasurementGraphViewModel _viewModel;

        public MeasurementGraphView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += (s, e) => Hook(DataContext as MeasurementGraphViewModel);
            SizeChanged += (s, e) => Redraw();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Hook(DataContext as MeasurementGraphViewModel);
            Redraw();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unhook();
        }

        // Keeps the View subscribed to the ViewModel's change notifications,
        // regardless of the order in which DataContext / Loaded are set.
        private void Hook(MeasurementGraphViewModel viewModel)
        {
            if (_viewModel == viewModel)
            {
                return;
            }

            Unhook();
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.GraphChanged += Redraw;
                _viewModel.RecentMeasurements.CollectionChanged += OnMeasurementsChanged;
            }

            Redraw();
        }

        private void Unhook()
        {
            if (_viewModel != null)
            {
                _viewModel.GraphChanged -= Redraw;
                _viewModel.RecentMeasurements.CollectionChanged -= OnMeasurementsChanged;
                _viewModel = null;
            }
        }

        private void OnMeasurementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            if (!IsLoaded)
            {
                return;
            }

            GraphCanvas.Children.Clear();

            IReadOnlyList<MeasurementRecord> records = _viewModel?.RecentMeasurements;
            if (records == null || records.Count == 0)
            {
                EmptyHint.Visibility = Visibility.Visible;
                return;
            }

            EmptyHint.Visibility = Visibility.Collapsed;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                // The canvas is not laid out yet - redraw once layout is finished.
                Dispatcher.BeginInvoke(new Action(Redraw), DispatcherPriority.Loaded);
                return;
            }

            const double leftPad = 28;
            const double rightPad = 28;
            const double labelSpace = 24;
            const double topPad = 10;
            const double minRadius = 9;

            int count = records.Count;
            double maxRadius = Math.Min((height - labelSpace - topPad) / 2.0 - 2, 36);
            if (maxRadius < minRadius)
            {
                maxRadius = minRadius;
            }

            double maxValue = 0;
            foreach (MeasurementRecord r in records)
            {
                maxValue = Math.Max(maxValue, r.Value);
            }
            double reference = Math.Max(maxValue, Entity.MaxValidValue);

            double centerY = topPad + maxRadius;
            double spacing = count > 1 ? (width - leftPad - rightPad) / (count - 1) : 0;

            var validBrush = (Brush)(TryFindResource("ValidBrush") ?? Brushes.LimeGreen);
            var dangerBrush = (Brush)(TryFindResource("DangerBrush") ?? Brushes.Red);
            var mutedBrush = (Brush)(TryFindResource("MutedBrush") ?? Brushes.Gray);

            double axisY = centerY + maxRadius + 6;
            GraphCanvas.Children.Add(new Line
            {
                X1 = leftPad - 10,
                Y1 = axisY,
                X2 = width - rightPad + 10,
                Y2 = axisY,
                Stroke = mutedBrush,
                StrokeThickness = 1
            });

            for (int i = 0; i < count; i++)
            {
                MeasurementRecord record = records[i];
                double x = count > 1 ? leftPad + i * spacing : width / 2.0;
                double radius = minRadius + (record.Value / reference) * (maxRadius - minRadius);
                radius = Math.Max(minRadius, Math.Min(maxRadius, radius));

                GraphCanvas.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = centerY,
                    X2 = x,
                    Y2 = axisY,
                    Stroke = mutedBrush,
                    StrokeThickness = 0.6,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                });

                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Fill = record.IsValid ? validBrush : dangerBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(circle, x - radius);
                Canvas.SetTop(circle, centerY - radius);
                GraphCanvas.Children.Add(circle);

                var valueText = new TextBlock
                {
                    Text = record.Value.ToString("0"),
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                };
                valueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(valueText, x - valueText.DesiredSize.Width / 2.0);
                Canvas.SetTop(valueText, centerY - valueText.DesiredSize.Height / 2.0);
                GraphCanvas.Children.Add(valueText);

                var timeText = new TextBlock
                {
                    Text = record.TimeLabel,
                    Foreground = mutedBrush,
                    FontSize = 10
                };
                timeText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(timeText, x - timeText.DesiredSize.Width / 2.0);
                Canvas.SetTop(timeText, axisY + 4);
                GraphCanvas.Children.Add(timeText);
            }
        }
    }
}