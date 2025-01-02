using System.Collections.Generic;

using System.Collections.Generic;

public class Card
{
    public string name { get; set; } = "Onbekend";
    public string set { get; set; } = "Onbekend";
    public string collector_number { get; set; } = "Onbekend";
    public int Quantity { get; set; } = 0;
    public string Style { get; set; } = "Normaal"; // Alleen voor foil-informatie
    public string image_uris { get; set; } = "https://cards.scryfall.io/normal/front/0/0/001eb913-2afe-4d7d-89a1-7c35de92d702.jpg?1540162762";
    public string mana_cost { get; set; } = "Geen"; // Voor mana cost
    public string oracle_text { get; set; } = "Geen beschrijving beschikbaar.";
    public Dictionary<string, string> legalities { get; set; } = new Dictionary<string, string>();
    public string rulings_uri { get; set; } = string.Empty;
}


