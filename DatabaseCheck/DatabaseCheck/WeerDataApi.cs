using System;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;

public class WeerDataApi
{
    private string _weerApiUrl = "https://weerlive.nl/api/weerlive_api_v2.php?key=655523df03&locatie=Akkrum";
    private string _connectionString = "Host=localhost;Username=postgres;Password=Ryancool123;Database=testbase";
    private string _timeApiUrl = "https://worldtimeapi.org/api/timezone/Europe/Amsterdam";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await HaalEnSlaDataOp();  
            Log.Information("Wachten voor de volgende API-oproep...");
            await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);  
        }
    }

    
    
    public async Task HaalEnSlaDataOp()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                // Haal gegevens op van de weer-API
                var weerApiAntwoord = await client.GetAsync(_weerApiUrl);
                weerApiAntwoord.EnsureSuccessStatusCode();
                var weerData = await weerApiAntwoord.Content.ReadAsStringAsync();

                // Haal de tijd op van de tijd-API
                var tijdApiAntwoord = await client.GetAsync(_timeApiUrl);
                tijdApiAntwoord.EnsureSuccessStatusCode();
                var tijdData = await tijdApiAntwoord.Content.ReadAsStringAsync();

                // Zet beide data in de database
                ZetDataInDatabase(weerData, tijdData);
            }
        }
        catch (Exception ex)
        {
            Log.Information($"Er is een fout opgetreden bij het ophalen van de API-gegevens: {ex.Message}");
        }
    }

    // Aangepaste methode om zowel de weer- als de tijdgegevens in de database op te slaan
    private void ZetDataInDatabase(string weerData, string tijdData)
    {
        using (var verbinding = new NpgsqlConnection(_connectionString))
        {
            try
            {
                verbinding.Open();
                JObject jsonWeerData = JObject.Parse(weerData); 
                var weerInfo = jsonWeerData["liveweer"][0];
                
                string plaats = (string)weerInfo["plaats"];      
                string temperatuurString = (string)weerInfo["temp"];   
                

                // Parse temperatuur naar een numeriek type
                if (decimal.TryParse(temperatuurString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal temperatuur))
                {
                    // Haal de tijd op uit de tijd-API
                    JObject jsonTijdData = JObject.Parse(tijdData);
                    string apiTijd = (string)jsonTijdData["datetime"];
                    Log.Information($"Data opgeslagen: temperatuur={temperatuur}, tijd={apiTijd}, locatie={plaats}");


                    string ZetDataInDatabaseQuery = "INSERT INTO buitentemperatuur (temperatuur, tijd, locatie) VALUES (@temperatuur, @tijd, @locatie)";

                    using (var opdracht = new NpgsqlCommand(ZetDataInDatabaseQuery, verbinding))
                    {
                        opdracht.Parameters.AddWithValue("@temperatuur", temperatuur);  // Voeg de temperatuur toe als numeriek type
                        opdracht.Parameters.AddWithValue("@tijd", apiTijd);  // Tijd van de tijd-API opslaan
                        opdracht.Parameters.AddWithValue("@locatie", plaats);

                        opdracht.ExecuteNonQuery();
                        Log.Information("Data succesvol opgeslagen in de database.");
                    }
                }
                else
                {
                    Log.Information("De temperatuurwaarde kon niet worden geconverteerd naar een numeriek type.");
                }
            }
            catch (Exception ex)
            {
                Log.Information($"Er is een fout opgetreden bij het opslaan van gegevens in de database: {ex.Message}");
            } 
        }
    }
}
