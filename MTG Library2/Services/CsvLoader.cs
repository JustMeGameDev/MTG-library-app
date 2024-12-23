using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MTG_Library2;

public class CsvLoader
{
    private readonly string _jsonFilePath;
    private readonly Dictionary<string, Card> _scryfallCards;

    public CsvLoader(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
        _scryfallCards = LoadAllCardsFromJson(jsonFilePath);
    }
    private Card MapJsonToCard(ScryfallCard card)
    {
        return new Card
        {
            name = card.name,
            set = card.set,
            collector_number = card.collector_number,
            Quantity = 0, // Zet standaard op 0, tenzij je dit ergens anders definieert
            Style = string.Empty, // Leeg, tenzij je een waarde hebt
            image_uris = card.image_uris != null ? card.image_uris.Normal : "NOT_FOUND"
        };
    }
private Dictionary<string, Card> LoadAllCardsFromJson(string jsonFilePath)
{
    var cardDictionary = new Dictionary<string, Card>();

    foreach (var scryfallCard in StreamAllCardsFromJson(jsonFilePath))
    {
        if (scryfallCard == null)
        {
            Console.WriteLine("Null card encountered, skipping...");
            continue;
        }

        if (string.IsNullOrEmpty(scryfallCard.name) || string.IsNullOrEmpty(scryfallCard.set) || string.IsNullOrEmpty(scryfallCard.collector_number))
        {
            Console.WriteLine($"Invalid card: Name='{scryfallCard.name}', Set='{scryfallCard.set}', CollectorNumber='{scryfallCard.collector_number}'");
            continue;
        }

        var sanitizedCardName = scryfallCard.name.Trim().Replace("\u200B", "").ToLowerInvariant(); // Verwijder onzichtbare karakters
        var key = $"{sanitizedCardName}_{scryfallCard.set.ToLowerInvariant()}_{scryfallCard.collector_number.ToLowerInvariant()}";
        Console.WriteLine($"Generated key: {key} for card: {scryfallCard.name}");

        if (scryfallCard.image_uris?.Normal == null)
        {
            Console.WriteLine($"No image found for card: {scryfallCard.name} (Set: {scryfallCard.set}, CollectorNumber: {scryfallCard.collector_number})");
        }
        else
        {
            Console.WriteLine($"Loaded image for card: {scryfallCard.name} -> {scryfallCard.image_uris.Normal}");
        }

        var card = MapJsonToCard(scryfallCard);

        if (scryfallCard.name.Contains("//"))
        {
            var parts = scryfallCard.name.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var part1Key = $"{parts[0].Trim().ToLowerInvariant()}_{scryfallCard.set.ToLowerInvariant()}_{scryfallCard.collector_number.ToLowerInvariant()}";
                var part2Key = $"{parts[1].Trim().ToLowerInvariant()}_{scryfallCard.set.ToLowerInvariant()}_{scryfallCard.collector_number.ToLowerInvariant()}";

                Console.WriteLine($"Checking split card parts: {parts[0].Trim()} ({part1Key}) and {parts[1].Trim()} ({part2Key})");

                if (cardDictionary.TryGetValue(part1Key, out var part1Card))
                {
                    Console.WriteLine($"Found image for first part: {parts[0].Trim()} -> {part1Card.image_uris}");
                    card.image_uris = part1Card.image_uris ?? "NOT_FOUND";
                }
                else if (cardDictionary.TryGetValue(part2Key, out var part2Card))
                {
                    Console.WriteLine($"Found image for second part: {parts[1].Trim()} -> {part2Card.image_uris}");
                    card.image_uris = part2Card.image_uris ?? "NOT_FOUND";
                }
                else
                {
                    Console.WriteLine($"No image URI found for parts: {parts[0].Trim()} or {parts[1].Trim()}");
                }
            }
        }

        cardDictionary[key] = card;

        if (string.IsNullOrEmpty(card.image_uris) || card.image_uris == "NOT_FOUND")
        {
            Console.WriteLine($"No image URI found for card: {scryfallCard.name} (Set: {scryfallCard.set}, CollectorNumber: {scryfallCard.collector_number})");
        }
    }

    return cardDictionary;
}



    private IEnumerable<ScryfallCard> StreamAllCardsFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"JSON file not found at {jsonFilePath}");

        using (var stream = File.OpenRead(jsonFilePath))
        using (var reader = new StreamReader(stream))
        using (var jsonReader = new JsonTextReader(reader))
        {
            var serializer = new JsonSerializer();

            // Begin lezen van de JSON-array
            if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
            {
                while (jsonReader.Read()) // Lees door de array heen
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        // Deserializeer één kaart en yield deze
                        var card = serializer.Deserialize<ScryfallCard>(jsonReader);
                        if (card != null)
                            yield return card;
                    }
                }
            }
        }
    }
    
    public IEnumerable<List<Card>> LoadCardsInBatches(string csvFilePath, int batchSize)
    {
        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException($"The CSV file {csvFilePath} was not found.");
        }

        using (var reader = new StreamReader(csvFilePath))
        {
            // Lees de eerste regel (headers)
            var header = reader.ReadLine();
            if (header == null)
            {
                throw new InvalidOperationException("CSV file is empty.");
            }

            var batch = new List<Card>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;

                var values = line.Split(',');
                if (values.Length < 5) continue;

                var card = new Card
                {
                    name = values[0].Trim(),
                    set = values[1].Trim(),
                    collector_number = values[2].Trim(),
                    Quantity = int.TryParse(values[3], out var quantity) ? quantity : 0,
                    Style = values[4].Trim(),
                    image_uris = values.Length > 5 ? values[5].Trim() : null
                };

                batch.Add(card);

                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                yield return batch;
            }
        }
    }

    public void UpdateCsvWithImageUris(string csvFilePath, LoadingWindow loadingWindow)
    {
        if (!File.Exists(csvFilePath))
            throw new FileNotFoundException($"CSV file not found at {csvFilePath}");

        var tempFilePath = $"{csvFilePath}.tmp";

        using (var reader = new StreamReader(new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        using (var writer = new StreamWriter(tempFilePath))
        using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
        using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
        {
            var records = csvReader.GetRecords<Card>().ToList();
            int totalRecords = records.Count;
            int processedRecords = 0;

            foreach (var record in records)
            {
                processedRecords++;
                var progress = (double)processedRecords / totalRecords * 100;

                loadingWindow.Dispatcher.Invoke(() =>
                {
                    loadingWindow.UpdateProgress(progress, $"Processing card {processedRecords}/{totalRecords}");
                });

                var cardKey = $"{record.name.Trim().ToLowerInvariant()}_{record.set.Trim().ToLowerInvariant()}_{record.collector_number.Trim().ToLowerInvariant()}";
                if (_scryfallCards.TryGetValue(cardKey, out var card))
                {
                    record.image_uris = card.image_uris ?? "NOT_FOUND";
                }
            }

            csvWriter.WriteRecords(records);
        }

        File.Delete(csvFilePath);
        File.Move(tempFilePath, csvFilePath);
    }


}
