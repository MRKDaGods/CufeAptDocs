using MRK;
using static System.Console;

WriteLine("Starting..");

var server = new NetServer();
server.Start();

while (!Console.KeyAvailable)
{
    Thread.Sleep(15);
}

server.Stop();
WriteLine("Exiting");

ReadLine();