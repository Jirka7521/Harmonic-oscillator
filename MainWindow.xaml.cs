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
    /// Simulace matematického kyvadla v harmonickém pohybu
    /// Používá aproximaci malých úhlů pro výpočet pohybu kyvadla
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Konstanty a proměnné

        /// <summary>
        /// Gravitační zrychlení (m/s²)
        /// </summary>
        private const double g = 9.81;

        /// <summary>
        /// Pole pro ukládání časových bodů simulace
        /// </summary>
        private double[] timePoints;

        /// <summary>
        /// Pole pro ukládání úhlové výchylky v čase
        /// </summary>
        private double[] displacement;

        /// <summary>
        /// Pole pro ukládání úhlové rychlosti v čase
        /// </summary>
        private double[] velocity;

        /// <summary>
        /// Pole pro ukládání úhlového zrychlení v čase
        /// </summary>
        private double[] acceleration;

        #endregion

        /// <summary>
        /// Inicializace hlavního okna aplikace
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializePlots();  // Nastavení grafů s výchozím stylem
        }

        #region Metody pro nastavení a aktualizaci grafů

        /// <summary>
        /// Inicializace grafu s výchozím nastavením
        /// </summary>
        private void InitializePlots()
        {
            // Nastavení titulku a popisků os
            plotCombined.Plot.Title("Pohyb kyvadla");
            plotCombined.Plot.XLabel("Čas (s)");
            plotCombined.Plot.YLabel("Výchylka (rad)");

            // Zapnutí mřížky pro lepší čitelnost
            plotCombined.Plot.Grid.IsVisible = true;

            // Nastavení pozice legendy
            plotCombined.Plot.Legend.Location = ScottPlot.Alignment.UpperRight;

            // Aplikace nastavení
            plotCombined.Refresh();
        }

        /// <summary>
        /// Aktualizuje graf podle aktuálních dat a nastavení uživatele
        /// </summary>
        private void UpdatePlot()
        {
            // Kompletní reset grafu před vykreslením nových dat
            plotCombined.Reset();

            // Výpočet vzorkovací frekvence pro vykreslování
            double sampleRate = 1.0 / (timePoints[1] - timePoints[0]);

            // --- Vykreslení výchylky (vždy na primární ose Y vlevo) ---
            var dispAxis = plotCombined.Plot.Axes.AddLeftAxis();
            dispAxis.Label.Text = "Výchylka [rad]";

            var dispLine = plotCombined.Plot.Add.Scatter(timePoints, displacement);
            dispLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
            dispLine.Axes.YAxis = dispAxis;
            dispLine.LegendText = "Výchylka";
            dispLine.LineWidth = 2;
            dispLine.Color = System.Drawing.Color.FromArgb(0, 120, 215);  // Modrá barva pro výchylku

            // --- Vykreslení rychlosti na pravé ose Y, pokud je zvolena ---
            if (chkVelocity.IsChecked == true)
            {
                var velAxis = plotCombined.Plot.Axes.AddRightAxis();
                velAxis.Label.Text = "Rychlost [rad/s]";

                var velLine = plotCombined.Plot.Add.Scatter(timePoints, velocity);
                velLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                velLine.Axes.YAxis = velAxis;
                velLine.LegendText = "Rychlost";
                velLine.LineWidth = 2;
                velLine.Color = System.Drawing.Color.FromArgb(232, 17, 35);  // Červená barva pro rychlost
            }

            // --- Vykreslení zrychlení na pravé ose Y, pokud je zvoleno ---
            if (chkAcceleration.IsChecked == true)
            {
                var accelAxis = plotCombined.Plot.Axes.AddRightAxis();
                accelAxis.Label.Text = "Zrychlení [rad/s²]";

                var accelLine = plotCombined.Plot.Add.Scatter(timePoints, acceleration);
                accelLine.Axes.XAxis = plotCombined.Plot.Axes.Bottom;
                accelLine.Axes.YAxis = accelAxis;
                accelLine.LegendText = "Zrychlení";
                accelLine.LineWidth = 2;
                accelLine.Color = System.Drawing.Color.FromArgb(0, 153, 0);  // Zelená barva pro zrychlení
            }

            // --- Finální nastavení grafu ---
            plotCombined.Plot.Title("Pohyb kyvadla");
            plotCombined.Plot.XLabel("Čas [s]");

            // Zobrazení legendy pouze pokud je více než jedna křivka
            plotCombined.Plot.Legend.IsVisible = chkVelocity.IsChecked == true || chkAcceleration.IsChecked == true;

            // Automatické nastavení měřítka os
            plotCombined.Plot.Axes.AutoScale();

            // Aplikace změn
            plotCombined.Refresh();
        }

        #endregion

        #region Události

        /// <summary>
        /// Obsluha události kliknutí na tlačítko výpočtu
        /// Provádí výpočet pohybu kyvadla a aktualizuje graf
        /// </summary>
        /// <param name="sender">Objekt, který vyvolal událost</param>
        /// <param name="e">Argumenty události</param>
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- Validace a načtení vstupních hodnot ---

                // Hmotnost (kg)
                if (!double.TryParse(txtWeight.Text, out double mass))
                {
                    throw new ArgumentException("Neplatná hodnota hmotnosti.");
                }

                // Délka kyvadla (m)
                if (!double.TryParse(txtLength.Text, out double length) || length <= 0)
                {
                    throw new ArgumentException("Délka musí být kladné číslo.");
                }

                // Počáteční úhel ve stupních
                if (!double.TryParse(txtInitialAngle.Text, out double initialAngleDegrees))
                {
                    throw new ArgumentException("Neplatná hodnota počátečního úhlu.");
                }

                // Doba simulace (s)
                if (!double.TryParse(txtSimTime.Text, out double simulationTime) || simulationTime <= 0)
                {
                    throw new ArgumentException("Doba simulace musí být kladné číslo.");
                }

                // --- Převod jednotek a výpočet základních parametrů ---

                // Převod úhlu ze stupňů na radiány
                double initialAngle = initialAngleDegrees * Math.PI / 180.0;

                // Výpočet periody kyvadla z fyzikálního vztahu
                double period = 2 * Math.PI * Math.Sqrt(length / g);

                // Úhlová frekvence kmitání
                double angularFrequency = Math.Sqrt(g / length);

                // --- Generování průběhu pohybu kyvadla ---

                // Vysoké rozlišení bodů pro hladký graf
                int numPoints = 10000;

                // Alokace polí pro uložení dat
                timePoints = new double[numPoints];
                displacement = new double[numPoints];
                velocity = new double[numPoints];
                acceleration = new double[numPoints];

                // Časový krok mezi jednotlivými body (s)
                double timeStep = simulationTime / (numPoints - 1);

                // Výpočet hodnot v jednotlivých časových bodech
                for (int i = 0; i < numPoints; i++)
                {
                    double t = i * timeStep;
                    timePoints[i] = t;

                    // Pro malé úhly je pohyb přibližně harmonický
                    displacement[i] = initialAngle * Math.Cos(angularFrequency * t);
                    velocity[i] = -initialAngle * angularFrequency * Math.Sin(angularFrequency * t);
                    acceleration[i] = -initialAngle * angularFrequency * angularFrequency * Math.Cos(angularFrequency * t);
                }

                // --- Aktualizace grafu a zobrazení výsledků ---
                UpdatePlot();

                // Zobrazení vypočtených charakteristik kyvadla
                txtResults.Text = $"Perioda kyvadla: {period:F2} sekund | Maximální výchylka: {initialAngle:F2} rad | Úhlová frekvence: {angularFrequency:F2} rad/s";
                txtResults.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 67, 101));
            }
            catch (Exception ex)
            {
                // Zobrazení chybového dialogu při výskytu problému
                MessageBox.Show(
                    $"Chyba: {ex.Message}",
                    "Chyba výpočtu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Zobrazení chybové zprávy v panelu výsledků
                txtResults.Text = "Výpočet selhal. Zkontrolujte vstupní hodnoty.";
                txtResults.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        /// <summary>
        /// Obsluha události při změně volby zobrazení grafu
        /// </summary>
        /// <param name="sender">Objekt, který vyvolal událost</param>
        /// <param name="e">Argumenty události</param>
        private void ChartOption_Changed(object sender, RoutedEventArgs e)
        {
            // Aktualizace grafu jen pokud již existují data
            if (timePoints != null && displacement != null &&
                velocity != null && acceleration != null)
            {
                UpdatePlot();
            }
        }

        #endregion
    }
}