using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net.Http;
using MTG_Library2;
using Newtonsoft.Json.Linq;

public class CsvLoader
{
    private readonly string _jsonFilePath;
    private readonly Dictionary<string, Card> _scryfallCards;

    public CsvLoader(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
        _scryfallCards = LoadAllCardsFromJson(jsonFilePath);
    }
    
    private Card MapJsonToCard(ScryfallCard scryfallCard)
    {
        return new Card
        {
            name = scryfallCard.name,
            set = scryfallCard.set,
            collector_number = scryfallCard.collector_number,
            image_uris = scryfallCard.image_uris?.Normal ?? "NOT_FOUND",
            mana_cost = scryfallCard.mana_cost,
            oracle_text = scryfallCard.oracle_text,
            legalities = scryfallCard.legalities,
            rulings_uri = scryfallCard.rulings_uri
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

        if (string.IsNullOrEmpty(card.image_uris) || card.image_uris == "https://cards.scryfall.io/normal/front/0/0/001eb913-2afe-4d7d-89a1-7c35de92d702.jpg?1540162762")
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
    private IEnumerable<dynamic> StreamJsonFile(string jsonFilePath)
    {
        using (var reader = new StreamReader(jsonFilePath))
        using (var jsonReader = new JsonTextReader(reader))
        {
            var serializer = new JsonSerializer();

            if (jsonReader.TokenType == JsonToken.StartArray || jsonReader.Read())
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.EndArray)
                        break;

                    yield return serializer.Deserialize<dynamic>(jsonReader);
                }
            }
        }
    }

public void UpdateCsvWithImageUris(string csvFilePath, string jsonFilePath, LoadingWindow loadingWindow)
{
    Console.WriteLine("Start updating CSV file.");

    // Load JSON data
    IEnumerable<dynamic> jsonCards = StreamJsonFile(jsonFilePath);
    Console.WriteLine($"Loaded JSON file: {jsonCards.Count()} cards found.");

    if (!File.Exists(csvFilePath))
        throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

    var tempFilePath = $"{csvFilePath}.tmp";

    using (var reader = new StreamReader(csvFilePath))
    using (var writer = new StreamWriter(tempFilePath))
    using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        HeaderValidated = null,
        MissingFieldFound = null
    }))
    using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true
    }))
    {
        // Read records
        var records = csvReader.GetRecords<dynamic>().ToList();
        Console.WriteLine("Read {0} records from CSV file.", records.Count);

        // Write headers to the new file
        csvWriter.WriteHeader<dynamic>();
        csvWriter.NextRecord();

        // Batch size
        var batchSize = 100;
        for (int i = 0; i < records.Count; i += batchSize)
        {
            var batch = records.Skip(i).Take(batchSize).ToList();
            Console.WriteLine("Processing batch {0} of {1}.", i / batchSize + 1, Math.Ceiling((double)records.Count / batchSize));

            foreach (var record in batch)
            {
                try
                {
                    // Get the card name
                    var cardName = record.name;
                    Console.WriteLine("Processing card: {0}", cardName);

                    // Find the card in JSON data
                    var jsonCard = jsonCards.FirstOrDefault(c => c.name == cardName);
                    if (jsonCard != null)
                    {
                        if (!((IDictionary<string, object>)record).ContainsKey("mana_cost"))
                            record.mana_cost = jsonCard.mana_cost ?? "Geen";

                        if (!((IDictionary<string, object>)record).ContainsKey("oracle_text"))
                            record.oracle_text = jsonCard.oracle_text ?? "Geen beschrijving beschikbaar.";

                        if (jsonCard.legalities is JObject tempLegalities)
                        {
                            // Legal formats
                            record.legal_formats = string.Join(", ",
                                tempLegalities.Properties()
                                    .Where(p => p.Value.ToString() == "legal")
                                    .Select(p => p.Name));

                            // Illegal formats
                            record.illegal_formats = string.Join(", ",
                                tempLegalities.Properties()
                                    .Where(p => p.Value.ToString() != "legal")
                                    .Select(p => p.Name));
                        }
                        else
                        {
                            // Default values for missing legalities
                            record.legal_formats = "Niet beschikbaar";
                            record.illegal_formats = "Niet beschikbaar";
                        }

                        if (!((IDictionary<string, object>)record).ContainsKey("rulings_uri"))
                            record.rulings_uri = jsonCard.rulings_uri ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing card: {0}. Exception: {1}", record.name, ex.Message);
                }

                csvWriter.WriteRecord(record);
            }

            csvWriter.NextRecord();

            // Update progress in the loading window
            var progress = ((i + batchSize) / (float)records.Count) * 100;
            Console.WriteLine("Batch {0} completed. Progress: {1:F1}%.", i / batchSize + 1, progress);
            loadingWindow.UpdateProgress(progress, $"Batch {i / batchSize + 1} voltooid. Voortgang: {progress:F1}%");
        }
    }

    Console.WriteLine("All batches processed. Writing final CSV file.");

    File.Delete(csvFilePath);
    File.Move(tempFilePath, csvFilePath);

    Console.WriteLine("CSV update completed successfully.");
} 
}
