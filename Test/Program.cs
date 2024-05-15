using MRK.Networking;
using MRK.Networking.Internal;

var listener = new EventBasedNetListener();
NetManager client = new(listener);

client.Start();

client.Connect("localhost", 23466, "yo");

while (!Console.KeyAvailable)
{
    client.PollEvents();
    Thread.Sleep(15);
}

client.Stop();