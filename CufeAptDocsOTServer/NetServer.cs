using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace MRK
{
    public class NetServer
    {
        private EventBasedNetListener listener;
        private NetManager server;
        private Dictionary<string, List<Operation>> documentOperations = new();
        private OperationalTransformer transformer = new();

        public NetServer()
        {
            listener = new EventBasedNetListener();

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("Client connected " + peer.Address.ToString());
                // Send the operation history to the newly connected client
                foreach (var docId in documentOperations.Keys)
                {
                    foreach (var op in documentOperations[docId])
                    {
                        SendOperation(peer, docId, op);
                    }
                }
            };

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;

            listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                string documentId = reader.GetString();
                string userId = reader.GetString();
                Operation operation = DeserializeOperation(reader);

                if (!documentOperations.ContainsKey(documentId))
                {
                    documentOperations[documentId] = new List<Operation>();
                }

                List<Operation> history = documentOperations[documentId];
                foreach (var appliedOperation in history)
                {
                    operation = transformer.Transform(operation, appliedOperation);
                }

                history.Add(operation);

                // Broadcast the operation to all clients
                foreach (var connectedPeer in server.ConnectedPeerList)
                {
                    SendOperation(connectedPeer, documentId, operation);
                }
            };

            server = new NetManager(listener);
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            request.Accept();
        }

        public void Start()
        {
            server.Start(23466);
            Console.WriteLine("Server started.");

            new Thread(() =>
            {
                while (true)
                {
                    server.PollEvents();
                    Thread.Sleep(15);
                }
            }).Start();
        }

        public void Stop()
        {
            server.Stop();
            Console.WriteLine("Server stopped.");
        }

        private void SendOperation(NetPeer peer, string documentId, Operation operation)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(documentId);
            writer.Put(operation.UserId);
            SerializeOperation(writer, operation);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        private Operation DeserializeOperation(NetDataReader reader)
        {
            string type = reader.GetString();
            int position = reader.GetInt();
            string userId = reader.GetString();
            DateTime timestamp = new DateTime(reader.GetLong());

            if (type == nameof(InsertOperation))
            {
                string text = reader.GetString();
                return new InsertOperation(position, text, userId) { Timestamp = timestamp };
            }
            else if (type == nameof(DeleteOperation))
            {
                int length = reader.GetInt();
                return new DeleteOperation(position, length, userId) { Timestamp = timestamp };
            }

            throw new InvalidOperationException("Unknown operation type.");
        }

        private void SerializeOperation(NetDataWriter writer, Operation operation)
        {
            writer.Put(operation.GetType().Name);
            writer.Put(operation.Position);
            writer.Put(operation.UserId);
            writer.Put(operation.Timestamp.Ticks);

            if (operation is InsertOperation insert)
            {
                writer.Put(insert.Text);
            }
            else if (operation is DeleteOperation delete)
            {
                writer.Put(delete.Length);
            }
        }
    }
}