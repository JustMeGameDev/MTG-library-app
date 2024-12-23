using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MTG_Library2.Services;

namespace MTG_Library2
{
    public partial class MainWindow : Window
    {
        private List<Card> _cards;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Laad de kaarten uit de CSV
        private async Task LoadCardsAsync()
        {
            var jsonPath = "Data/all_cards.json";
            var csvPath = "Data/MTG_DB.csv";

            var loadingWindow = new LoadingWindow();
            loadingWindow.Show();

            try
            {
                await Task.Run(() =>
                {
                    var csvLoader = new CsvLoader(jsonPath);
                    csvLoader.UpdateCsvWithImageUris(csvPath, loadingWindow);

                    // Laad kaarten na de update
                    _cards = csvLoader.LoadCardsInBatches(csvPath, 500).SelectMany(batch => batch).ToList();
                });

                // Update de UI na het laden van kaarten
                Dispatcher.Invoke(() =>
                {
                    ResultsListView.ItemsSource = _cards;
                });
            }
            finally
            {
                loadingWindow.Dispatcher.Invoke(() => loadingWindow.Close());
            }
        }


        // Zoekfunctie (bijgewerkt bij elke toetsaanslag)
        private void SearchBar_TextChanged(object sender, KeyEventArgs e)
        {
            var searchText = SearchBar.Text.ToLower();
            var results = _cards.Where(c => c.name.ToLower().Contains(searchText)).ToList();
            ResultsListView.ItemsSource = results; // Update ListBox met gefilterde resultaten
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCardsAsync(); // Roep de asynchrone laadfunctie aan
        }




        // Update-knop (roept de API aan om data te updaten)
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var scryfallApi = new ScryfallApi();
                var loadingWindow = new LoadingWindow();
                loadingWindow.Show();

                await scryfallApi.FetchAndSaveScryfallDataAsync(loadingWindow);

                loadingWindow.Close();
                MessageBox.Show("Data updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Herlaad kaarten na de update
                await LoadCardsAsync(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}