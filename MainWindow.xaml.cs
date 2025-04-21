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

        // Store calculated data
        private double[] timePoints;
        private double[] displacement;
        private double[] velocity;
        private double[] acceleration;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize plots with default styling
            InitializePlots();
        }

        private void InitializePlots()
        {
            // Set up the combined plot
            plotCombined.Plot.Title("Pendulum Motion");
            plotCombined.Plot.XLabel("Time (s)");
            plotCombined.Plot.YLabel("Displacement (rad)"); // Default left Y axis label
            plotCombined.Plot.Grid.IsVisible = true;

            // Set default styling for the plot
            plotCombined.Plot.Legend.Location = ScottPlot.Alignment.UpperRight;

            plotCombined.Refresh();
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
                timePoints = new double[numPoints];
                displacement = new double[numPoints];
                velocity = new double[numPoints];
                acceleration = new double[numPoints];

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

                // Update plot
                UpdatePlot();

                MessageBox.Show($"Pendulum period: {period:F2} seconds", "Calculation Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChartOption_Changed(object sender, RoutedEventArgs e)
        {
            // Only update the plot if we have data
            if (timePoints != null && displacement != null && velocity != null && acceleration != null)
            {
                UpdatePlot();
            }
        }

        private void UpdatePlot()
        {
            // Clear previous plot completely
            plotCombined.Reset();

            // Calculate the sample rate for signal plotting
            double sampleRate = 1.0 / (timePoints[1] - timePoints[0]);


            // Always show displacement on primary Y-axis (left)
            var dispAxis = plotCombined.Plot.Axes.AddLeftAxis();
            dispAxis.Label.Text = "Displacement [rad]";

            // Use the original data but with X positions from scaledTimePoints
            var dispLine = plotCombined.Plot.Add.Scatter(timePoints, displacement);
            dispLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
            dispLine.Axes.YAxis = dispAxis;
            dispLine.LegendText = "Displacement";
            dispLine.LineWidth = 2;

            // Velocity on secondary Y-axis (right) if selected
            if (chkVelocity.IsChecked == true)
            {
                var velAxis = plotCombined.Plot.Axes.AddRightAxis();
                velAxis.Label.Text = "Velocity [rad/s]";

                var velLine = plotCombined.Plot.Add.Scatter(timePoints, velocity);
                velLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                velLine.Axes.YAxis = velAxis;
                velLine.LegendText = "Velocity";
                velLine.LineWidth = 2;
            }

            // Acceleration on secondary Y-axis (right) if selected
            if (chkAcceleration.IsChecked == true)
            {
                var accelAxis = plotCombined.Plot.Axes.AddRightAxis();
                accelAxis.Label.Text = "Acceleration [rad/s²]";

                var accelLine = plotCombined.Plot.Add.Scatter(timePoints, acceleration);
                accelLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                accelLine.Axes.YAxis = accelAxis;
                accelLine.LegendText = "Acceleration";
                accelLine.LineWidth = 2;
            }

            // Configure plot appearance
            plotCombined.Plot.Title("Pendulum Motion");
            plotCombined.Plot.XLabel("Time [s]");  // Updated label to indicate scaling

            // Show legend only if more than one plot is visible
            plotCombined.Plot.Legend.IsVisible = chkVelocity.IsChecked == true || chkAcceleration.IsChecked == true;

            // Ensure appropriate scaling
            plotCombined.Plot.Axes.AutoScale();

            // Refresh to apply changes
            plotCombined.Refresh();
        }
    }
}