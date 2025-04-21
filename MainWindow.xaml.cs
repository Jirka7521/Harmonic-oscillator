using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScottPlot;

namespace HarmonicOscillator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants
        private const double g = 9.81; // Acceleration due to gravity (m/s²)

        public MainWindow()
        {
            InitializeComponent();

            // Initialize plots with default styling
            InitializePlots();
        }

        private void InitializePlots()
        {
            // Set up plots with common styling
            var plots = new[] { plotDisplacement, plotVelocity, plotAcceleration, plotCombined };

            foreach (var plot in plots)
            {
                plot.Plot.Title(null);
                plot.Plot.XLabel("Time (s)");
                plot.Plot.Grid.IsVisible = true;
            }

            // Set specific Y labels
            plotDisplacement.Plot.YLabel("Angular Displacement (rad)");
            plotVelocity.Plot.YLabel("Angular Velocity (rad/s)");
            plotAcceleration.Plot.YLabel("Angular Acceleration (rad/s²)");
            plotCombined.Plot.YLabel("Value");

            // Refresh all plots
            foreach (var plot in plots)
            {
                plot.Refresh();
            }
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse input values
                if (!double.TryParse(txtWeight.Text, out double mass))
                    throw new ArgumentException("Invalid weight value.");

                if (!double.TryParse(txtLength.Text, out double length) || length <= 0)
                    throw new ArgumentException("Length must be a positive number.");

                if (!double.TryParse(txtInitialAngle.Text, out double initialAngleDegrees))
                    throw new ArgumentException("Invalid initial angle value.");

                if (!double.TryParse(txtSimTime.Text, out double simulationTime) || simulationTime <= 0)
                    throw new ArgumentException("Simulation time must be a positive number.");

                // Convert angle from degrees to radians
                double initialAngle = initialAngleDegrees * Math.PI / 180.0;

                // Calculate pendulum period
                double period = 2 * Math.PI * Math.Sqrt(length / g);

                // For small angles, the motion is approximately simple harmonic
                // Generate time points and solution data
                int numPoints = 1000;
                double[] timePoints = new double[numPoints];
                double[] displacement = new double[numPoints];
                double[] velocity = new double[numPoints];
                double[] acceleration = new double[numPoints];

                double timeStep = simulationTime / (numPoints - 1);
                double angularFrequency = Math.Sqrt(g / length);

                for (int i = 0; i < numPoints; i++)
                {
                    double t = i * timeStep;
                    timePoints[i] = t;

                    // For small angles, simple harmonic motion is a good approximation
                    displacement[i] = initialAngle * Math.Cos(angularFrequency * t);
                    velocity[i] = -initialAngle * angularFrequency * Math.Sin(angularFrequency * t);
                    acceleration[i] = -initialAngle * angularFrequency * angularFrequency * Math.Cos(angularFrequency * t);
                }

                // Update plots
                UpdatePlots(timePoints, displacement, velocity, acceleration);

                MessageBox.Show($"Pendulum period: {period:F2} seconds", "Calculation Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePlots(double[] timePoints, double[] displacement, double[] velocity, double[] acceleration)
        {
            // Clear previous plots
            plotDisplacement.Plot.Clear();
            plotVelocity.Plot.Clear();
            plotAcceleration.Plot.Clear();
            plotCombined.Plot.Clear();

            // Add data to individual plots with explicit ScottPlot.Color usage
            plotDisplacement.Plot.Add.Scatter(timePoints, displacement, color: ScottPlot.Color.FromColor(System.Drawing.Color.Red));
            plotVelocity.Plot.Add.Scatter(timePoints, velocity, color: ScottPlot.Color.FromColor(System.Drawing.Color.Blue));
            plotAcceleration.Plot.Add.Scatter(timePoints, acceleration, color: ScottPlot.Color.FromColor(System.Drawing.Color.Green));

            // Add all data to combined plot with labels
            var dispScatter = plotCombined.Plot.Add.Scatter(timePoints, displacement, color: ScottPlot.Color.FromColor(System.Drawing.Color.Red));
            var velScatter = plotCombined.Plot.Add.Scatter(timePoints, velocity, color: ScottPlot.Color.FromColor(System.Drawing.Color.Blue));
            var accScatter = plotCombined.Plot.Add.Scatter(timePoints, acceleration, color: ScottPlot.Color.FromColor(System.Drawing.Color.Green));

            dispScatter.Label = "Displacement";
            velScatter.Label = "Velocity";
            accScatter.Label = "Acceleration";

            plotCombined.Plot.ShowLegend();

            // Auto-scale axis limits
            plotDisplacement.Plot.Axes.AutoScale();
            plotVelocity.Plot.Axes.AutoScale();
            plotAcceleration.Plot.Axes.AutoScale();
            plotCombined.Plot.Axes.AutoScale();

            // Refresh all plots
            plotDisplacement.Refresh();
            plotVelocity.Refresh();
            plotAcceleration.Refresh();
            plotCombined.Refresh();
        }
    }
}