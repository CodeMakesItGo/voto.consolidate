using file.consolidate;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Voto.Consolidate
{
    /// <summary>
    /// Interaction logic for PageConsolidate.xaml
    /// </summary>
    public partial class PageConsolidate : Page
    {
        private int _lastChildIndex = 0;
        private readonly List<double> bpsList;
        private const int _maxColumns = 100;
        private double _maxBps = Double.MinValue;
        private double _movingAverage = 0;
        private readonly ProgressReport progressReport;
        private delegate void UpdateProgessDelegate();

        public PageConsolidate()
        {
            InitializeComponent();

            progressReport = new ProgressReport(UpdateProgress);

            bpsList = new List<double>();
        }

        public void UpdateProgress()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new UpdateProgessDelegate(UpdateProgress));
                return;
            }

            labelCount.Content = progressReport.Completed.ToString();
            labelSuccess.Content = progressReport.Successful.ToString();
            labelFailed.Content = progressReport.Failed.ToString();
            labelSkipped.Content = progressReport.Skipped.ToString();
            labelSpeed.Content = progressReport.Bps.ToString("N1");
            labelTotalBytes.Content = progressReport.TotalBytes.ToString("N");
            labelComplete.Content = progressReport.PercentComplete.ToString("P");

            textBlockText2.Text = progressReport.Report;
            textBlockText.Text = progressReport.FileName;
            ProgressBarComplete.Maximum = progressReport.Total;
            ProgressBarComplete.Minimum = 0;
            ProgressBarComplete.Value = progressReport.Completed;

            ProgressBarRunning.IsIndeterminate = true;

            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            var height = CanvasMain.ActualHeight;
            var width = CanvasMain.ActualWidth;
            var mbps = progressReport.Bps / 1000000.0;

            if (double.IsPositiveInfinity(progressReport.Bps) || double.IsNegativeInfinity(progressReport.Bps) ||
                double.IsNaN(progressReport.Bps) || Math.Abs(progressReport.Bps) < 0.001)
            {
                return;
            }

            _movingAverage = .1 * mbps + (1.0 - .1) * _movingAverage;

            bpsList.Add(mbps);

            while (bpsList.Count > _maxColumns)
            {
                bpsList.RemoveAt(0);
            }

            var stepX = width / bpsList.Count;
            var startX = 0.0;

            //Remove the previous bps values but not the background lines
            CanvasMain.Children.RemoveRange(_lastChildIndex, CanvasMain.Children.Count - _lastChildIndex);


            foreach (var bps in bpsList)
            {
                _maxBps = Math.Max(bps, _maxBps);

                var percent = 1.0 - bps / _maxBps;

                var startY = height * percent;

                AddLine(new Point(startX + (stepX / 2.0), height), new Point(startX + (stepX / 2.0), startY),
                    Brushes.MediumPurple, stepX, .33);

                startX += stepX;
            }

            var p = 1.0 - _movingAverage / _maxBps;

            var y = height * p;

            AddLine(new Point(0, y), new Point(width, y), Brushes.Purple, 1.0, .75);

            AddText(width / 2.0, y, $"{_movingAverage:N1}Mb\\s", Colors.DimGray);
        }

        private void CreateCanvas()
        {
            const int countX = 10;
            const int countY = 10;

            CanvasMain.Children.Clear();

            var height = CanvasMain.ActualHeight;
            var width = CanvasMain.ActualWidth;

            var stepX = width / countX;
            var stepY = height / countY;

            var startX = 0.0;
            var startY = 0.0;


            for (var i = 0; i <= countX; ++i)
            {
                AddLine(new Point(startX, 0), new Point(startX, height), Brushes.LightGray, 1.0);
                startX += stepX;
            }

            for (var i = 0; i <= countY; ++i)
            {
                AddLine(new Point(0, startY), new Point(width, startY), Brushes.LightGray, 1.0);
                startY += stepY;
            }

            _lastChildIndex = CanvasMain.Children.Count;
        }

        private void AddLine(Point p1, Point p2, Brush Fill, double thickness = 1, double opacity = 1.0,
            PenLineCap cap = PenLineCap.Flat)
        {
            CanvasMain.Children.Add(new Line()
            {
                X1 = p1.X,
                X2 = p2.X,
                Y1 = p1.Y,
                Y2 = p2.Y,
                StrokeThickness = thickness,
                StrokeDashCap = cap,
                StrokeEndLineCap = cap,
                StrokeStartLineCap = cap,
                Stroke = Fill,
                StrokeLineJoin = PenLineJoin.Round,
                Opacity = opacity
            });
        }

        private void AddText(double x, double y, string text, Color color)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(color)
            };

            Canvas.SetLeft(textBlock, x);

            Canvas.SetTop(textBlock, y);

            CanvasMain.Children.Add(textBlock);
        }


        public void Complete()
        {
            textBlockText2.Text = "Finished!";
            textBlockText.Text = "";
            ProgressBarRunning.IsIndeterminate = false;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateCanvas();
        }
    }
}