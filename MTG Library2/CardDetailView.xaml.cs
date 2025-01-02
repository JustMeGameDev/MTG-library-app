using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace MTG_Library2;

public partial class CardDetailView : Window
{
    public CardDetailView(Card scryfallCard)
    {
        InitializeComponent();
        LoadCardDetails(scryfallCard);
    }



private async void LoadCardDetails(Card card)
{
    if (card == null)
    {
        MessageBox.Show("De geselecteerde kaart is ongeldig.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    // Controleer of de afbeelding URI geldig is, anders gebruik een plaatsvervangende URI
    string imageUri = !string.IsNullOrEmpty(card.image_uris) && Uri.IsWellFormedUriString(card.image_uris, UriKind.Absolute)
        ? card.image_uris
        : "https://via.placeholder.com/300"; // Plaatsvervangende afbeelding

    CardImage.Source = new BitmapImage(new Uri(imageUri));

    // Basisgegevens
    CardName.Text = card.name ?? "Naam niet beschikbaar";
    ManaCost.Text = $"Mana Cost: {(string.IsNullOrEmpty(card.mana_cost) ? "Geen" : card.mana_cost)}"; // Gebruik mana_cost in plaats van Style
    CardText.Text = !string.IsNullOrEmpty(card.oracle_text) ? card.oracle_text : "Geen tekst beschikbaar.";

    // Legaliteiten
    if (card.legalities != null)
    {
        var legalFormats = card.legalities.Where(l => l.Value == "legal").Select(l => l.Key);
        var illegalFormats = card.legalities.Where(l => l.Value != "legal").Select(l => l.Key);

        Legality.Text = $"Legal Formats: {string.Join(", ", legalFormats)}";
        IllegalFormats.Text = $"Illegal Formats: {string.Join(", ", illegalFormats)}";
    }
    else
    {
        Legality.Text = "Legal Formats: Niet beschikbaar";
        IllegalFormats.Text = "Illegal Formats: Niet beschikbaar";
    }

    // Rulings
    Rulings.Text = "Rulings ophalen...";
    if (!string.IsNullOrEmpty(card.rulings_uri))
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync(card.rulings_uri);
            var rulings = JObject.Parse(response)["data"]?.ToObject<List<dynamic>>();
            Rulings.Text = rulings != null && rulings.Count > 0
                ? string.Join("\n", rulings.Select(r => $"{r["published_at"]}: {r["comment"]}"))
                : "Geen rulings beschikbaar.";
        }
        catch
        {
            Rulings.Text = "Fout bij ophalen van rulings.";
        }
    }
    else
    {
        Rulings.Text = "Geen rulings URI beschikbaar.";
    }

    // Foil-status bepalen op basis van Style
    IsFoil.Text = card.Style.Contains("foil", StringComparison.OrdinalIgnoreCase) ? "✨" : "";
}

}

