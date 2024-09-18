using DatabaseCheck;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Database Check gestart.");


string host = "localhost";
string gebruiker = "postgres";
string wachtwoord = "Ryancool123";
string databaseNaam = "testbase";

var dbCheckTool = new DatabaseTool(host, gebruiker, wachtwoord, databaseNaam);
var apiDataTool = new WeerDataApi();




if (!dbCheckTool.DatabaseBestaat())
{
    Log.Information($"Database '{databaseNaam}' bestaat niet, wordt nu aangemaakt.");
    dbCheckTool.MaakDatabase();
    
    
    dbCheckTool.MaakTabel("buitentemperatuur", "id SERIAL PRIMARY KEY, temperatuur DECIMAL(5, 2) NOT NULL, tijd VARCHAR(255) NOT NULL, locatie VARCHAR(255) NOT NULL");




}
else
{
    Log.Information($"Database '{databaseNaam}' bestaat al.");

    if (!dbCheckTool.TabelBestaat("buitentemperatuur"))
    {
        Log.Information($"Tabel buitentemperatuur bestaat niet.");
        dbCheckTool.MaakTabel("buitentemperatuur", "id SERIAL PRIMARY KEY, temperatuur DECIMAL(5, 2) NOT NULL, tijd VARCHAR(255) NOT NULL, locatie VARCHAR(255) NOT NULL");

    


    }
    else
    {
        Log.Information($"Tabel buitentemperatuur bestaat.");
    }
            
}

var cancellationTokenSource = new CancellationTokenSource();
await apiDataTool.StartAsync(cancellationTokenSource.Token);



Log.Information("Check beëindigd.");