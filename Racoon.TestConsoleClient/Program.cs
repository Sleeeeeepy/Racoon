using Racoon.Core.Net;

Console.WriteLine("=== Server ===");
var client = new RacoonClientSocket("127.0.0.1", 5000);
Console.WriteLine("Press any key to continue...");
Console.ReadLine();
client.Connect();
