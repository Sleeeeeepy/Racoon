using Racoon.Core.Net;

var server = new RacoonServerSocket("0.0.0.0", 5000);
Console.WriteLine("Press any key to continue...");
Console.ReadLine();
server.Listen();