using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MastermindGame
{
    public partial class MainWindow : Window
    {
        private List<string> colors = new List<string> { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private List<string> Geheime_code = new List<string>();
        private DispatcherTimer timer = new DispatcherTimer();
        private int remainingTime = 10;
        private int totalAttempts = 0;
        private const int MaxAttempts = 10;

        // Lijst om de historiek van pogingen bij te houden
        private List<string> attemptHistory = new List<string>();
        private int currentScore = 0;

        public MainWindow()
        {
            InitializeComponent();
            GenerateGeheime_code();
            FillComboBoxes();
            SetupTimer();
        }

        private void GenerateGeheime_code()
        {
            Random random = new Random();
            Geheime_code.Clear();

            for (int i = 0; i < 4; i++)
            {
                Geheime_code.Add(colors[random.Next(colors.Count)]);
            }

            this.Title = "Mastermind";
        }

        private void FillComboBoxes()
        {
            var comboBoxes = new List<ComboBox> { Kleurcode1, Kleurcode2, Kleurcode3, Kleurcode4 };

            foreach (var comboBox in comboBoxes)
            {
                comboBox.ItemsSource = colors;
                comboBox.SelectedIndex = -1;
            }
        }

        private void SetupTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            Counter.Text = remainingTime.ToString();

            if (remainingTime <= 0)
            {
                timer.Stop();
                MessageBox.Show("De tijd is om! Probeer opnieuw.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetGame();
            }
        }

        private void ResetGame()
        {
            totalAttempts = 0;
            GenerateGeheime_code();
            FillComboBoxes();
            remainingTime = 10;
            Counter.Text = remainingTime.ToString();
            attemptHistory.Clear();
            UpdateHistoryView();
            currentScore = 0;
            UpdateScoreLabel();
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (totalAttempts >= MaxAttempts)
            {
                MessageBox.Show("Je hebt het maximale aantal pogingen bereikt! Het spel wordt opnieuw gestart.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetGame();
                return;
            }

            var guess = new List<string>
            {
                Kleurcode1.SelectedItem?.ToString(),
                Kleurcode2.SelectedItem?.ToString(),
                Kleurcode3.SelectedItem?.ToString(),
                Kleurcode4.SelectedItem?.ToString()
            };

            if (guess.Contains(null))
            {
                MessageBox.Show("Selecteer alle kleuren voordat je verdergaat.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string feedback = EvaluateGuess(guess, out int score);
            attemptHistory.Add(string.Join(", ", guess) + " - Feedback: " + feedback);
            UpdateHistoryView();

            currentScore += score;
            UpdateScoreLabel();

            totalAttempts++;

            if (totalAttempts >= MaxAttempts)
            {
                MessageBox.Show($"Je hebt het maximale aantal pogingen bereikt! De geheime code was: {string.Join(", ", Geheime_code)}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetGame();
            }
        }

        private string EvaluateGuess(List<string> guess, out int score)
        {
            var feedbackLabels = new List<Label> { Kleur1, Kleur2, Kleur3, Kleur4 };
            int correctPosition = 0;
            int correctColor = 0;
            score = 0;

            for (int i = 0; i < 4; i++)
            {
                feedbackLabels[i].BorderBrush = Brushes.Gray;
            }

            for (int i = 0; i < 4; i++)
            {
                if (guess[i] == Geheime_code[i])
                {
                    feedbackLabels[i].BorderBrush = Brushes.DarkRed;
                    correctPosition++;
                    score += 0; // Geen strafpunten voor correcte positie
                    Geheime_code[i] = null;
                    guess[i] = null;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (guess[i] != null && Geheime_code.Contains(guess[i]))
                {
                    feedbackLabels[i].BorderBrush = Brushes.Wheat;
                    correctColor++;
                    score += 1; // 1 strafpunt voor juiste kleur op verkeerde plaats
                    Geheime_code[Geheime_code.IndexOf(guess[i])] = null;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (guess[i] != null)
                {
                    score += 2; // 2 strafpunten voor een kleur die niet voorkomt
                }
            }

            return $"{correctPosition} rood, {correctColor} wit";
        }

        private void UpdateHistoryView()
        {
            HistoryListBox.ItemsSource = null;
            HistoryListBox.ItemsSource = attemptHistory;
        }

        private void UpdateScoreLabel()
        {
            ScoreLabel.Content = $"Score: {currentScore}";
        }

        private void ToggleDebug(object sender, RoutedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && Keyboard.IsKeyDown(Key.F12))
            {
                MessageBox.Show($"Debug Mode: Geheime code: {string.Join(", ", Geheime_code)}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
