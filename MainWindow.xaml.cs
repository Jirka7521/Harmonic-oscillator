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
    /// Simulation of a mathematical pendulum in harmonic motion
    /// Uses the small-angle approximation for calculating pendulum motion
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Variables

        /// <summary>
        /// Gravitational acceleration (m/s²)
        /// </summary>
        private const double g = 9.81;

        /// <summary>
        /// Array for storing simulation time points
        /// </summary>
        private double[] timePoints;

        /// <summary>
        /// Array for storing angular displacement over time
        /// </summary>
        private double[] displacement;

        /// <summary>
        /// Array for storing angular velocity over time
        /// </summary>
        private double[] velocity;

        /// <summary>
        /// Array for storing angular acceleration over time
        /// </summary>
        private double[] acceleration;

        #endregion

        /// <summary>
        /// Initialization of the main application window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializePlots();  // Setting up plots with default style
        }

        #region Methods for Setting and Updating Plots

        /// <summary>
        /// Initializes the plot with default settings
        /// </summary>
        private void InitializePlots()
        {
            // Setting the title and axis labels
            plotCombined.Plot.Title("Pohyb kyvadla");
            plotCombined.Plot.XLabel("Čas (s)");
            plotCombined.Plot.YLabel("Výchylka (rad)");

            // Enabling grid for better readability
            plotCombined.Plot.Grid.IsVisible = true;

            // Setting the position of the legend
            plotCombined.Plot.Legend.Location = ScottPlot.Alignment.UpperRight;

            // Applying settings
            plotCombined.Refresh();
        }

        /// <summary>
        /// Updates the plot based on current data and user settings
        /// </summary>
        private void UpdatePlot()
        {
            // Complete reset of the plot before rendering new data
            plotCombined.Reset();

            // Calculating the sampling rate for rendering
            double sampleRate = 1.0 / (timePoints[1] - timePoints[0]);

            // --- Plotting displacement (always on the primary Y-axis on the left) ---
            var dispAxis = plotCombined.Plot.Axes.AddLeftAxis();
            dispAxis.Label.Text = "Výchylka [rad]";

            var dispLine = plotCombined.Plot.Add.Scatter(timePoints, displacement);
            dispLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
            dispLine.Axes.YAxis = dispAxis;
            dispLine.LegendText = "Výchylka";
            dispLine.LineWidth = 2;
            dispLine.Color = ScottPlot.Color.FromHex("000fff");  // Blue color for displacement

            // --- Plotting velocity on the right Y-axis, if selected ---
            if (chkVelocity.IsChecked == true)
            {
                var velAxis = plotCombined.Plot.Axes.AddRightAxis();
                velAxis.Label.Text = "Rychlost [rad/s]";

                var velLine = plotCombined.Plot.Add.Scatter(timePoints, velocity);
                velLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                velLine.Axes.YAxis = velAxis;
                velLine.LegendText = "Rychlost";
                velLine.LineWidth = 2;
                velLine.Color = ScottPlot.Color.FromHex("E81123");   // Red color for velocity
            }

            // --- Plotting acceleration on the right Y-axis, if selected ---
            if (chkAcceleration.IsChecked == true)
            {
                var accelAxis = plotCombined.Plot.Axes.AddRightAxis();
                accelAxis.Label.Text = "Zrychlení [rad/s²]";

                var accelLine = plotCombined.Plot.Add.Scatter(timePoints, acceleration);
                accelLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                accelLine.Axes.YAxis = accelAxis;
                accelLine.LegendText = "Zrychlení";
                accelLine.LineWidth = 2;
                accelLine.Color = ScottPlot.Color.FromHex("009900");  // Green color for acceleration
            }

            // --- Final plot settings ---
            plotCombined.Plot.Title("Pohyb kyvadla");
            plotCombined.Plot.XLabel("Čas [s]");

            // Displaying the legend only if there is more than one curve
            plotCombined.Plot.Legend.IsVisible = chkVelocity.IsChecked == true || chkAcceleration.IsChecked == true;

            // Automatic scaling of axes
            plotCombined.Plot.Axes.AutoScale();

            // Applying changes
            plotCombined.Refresh();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event handler for the calculate button click
        /// Performs the pendulum motion calculation and updates the plot
        /// </summary>
        /// <param name="sender">Object that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- Validation and loading of input values ---

                // Mass (kg)
                if (!double.TryParse(txtWeight.Text, out double mass))
                {
                    throw new ArgumentException("Neplatná hodnota hmotnosti.");
                }

                // Pendulum length (m)
                if (!double.TryParse(txtLength.Text, out double length) || length <= 0)
                {
                    throw new ArgumentException("Délka musí být kladné číslo.");
                }

                // Initial angle in degrees
                if (!double.TryParse(txtInitialAngle.Text, out double initialAngleDegrees))
                {
                    throw new ArgumentException("Neplatná hodnota počátečního úhlu.");
                }

                // Simulation time (s)
                if (!double.TryParse(txtSimTime.Text, out double simulationTime) || simulationTime <= 0)
                {
                    throw new ArgumentException("Doba simulace musí být kladné číslo.");
                }

                // --- Unit conversion and calculation of basic parameters ---

                // Converting angle from degrees to radians
                double initialAngle = initialAngleDegrees * Math.PI / 180.0;

                // Calculating the pendulum period from the physical formula
                double period = 2 * Math.PI * Math.Sqrt(length / g);

                // Angular frequency of oscillation
                double angularFrequency = Math.Sqrt(g / length);

                // --- Generating the pendulum motion data ---

                // High resolution of points for a smooth graph
                int numPoints = 10000;

                // Allocating arrays for storing data
                timePoints = new double[numPoints];
                displacement = new double[numPoints];
                velocity = new double[numPoints];
                acceleration = new double[numPoints];

                // Time step between individual points (s)
                double timeStep = simulationTime / (numPoints - 1);

                // Calculating values at individual time points
                for (int i = 0; i < numPoints; i++)
                {
                    double t = i * timeStep;
                    timePoints[i] = t;

                    // For small angles, the motion is approximately harmonic
                    displacement[i] = initialAngle * Math.Cos(angularFrequency * t);
                    velocity[i] = -initialAngle * angularFrequency * Math.Sin(angularFrequency * t);
                    acceleration[i] = -initialAngle * angularFrequency * angularFrequency * Math.Cos(angularFrequency * t);
                }

                // --- Updating the plot and displaying results ---
                UpdatePlot();

                // Displaying calculated pendulum characteristics
                txtResults.Text = $"Perioda kyvadla: {period:F2} sekund | Maximální výchylka: {initialAngle:F2} rad | Úhlová frekvence: {angularFrequency:F2} rad/s";
                txtResults.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 67, 101));
            }
            catch (Exception ex)
            {
                // Displaying an error dialog in case of a problem
                MessageBox.Show(
                    $"Chyba: {ex.Message}",
                    "Chyba výpočtu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);


                // Displaying the error message in the results panel
                txtResults.Text = "Výpočet selhal. Zkontrolujte vstupní hodnoty.";
                txtResults.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        /// <summary>
        /// Event handler for changes in chart display options
        /// </summary>
        /// <param name="sender">Object that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private void ChartOption_Changed(object sender, RoutedEventArgs e)
        {
            // Update the plot only if data already exists
            if (timePoints != null && displacement != null &&
                velocity != null && acceleration != null)
            {
                UpdatePlot();
            }
        }

        #endregion
    }
}