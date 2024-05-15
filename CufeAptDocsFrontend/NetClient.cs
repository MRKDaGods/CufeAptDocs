using LiteNetLib;
using LiteNetLib.Utils;
using System.Windows;

namespace MRK
{
    public class NetClient
    {
        private EventBasedNetListener listener;
        private NetManager client;
        private string documentId = "exampleDocument";
        private string userId = Guid.NewGuid().ToString();
        private Action<Operation> applyOperation;
        private List<Operation> pendingOperations = new List<Operation>();
        private Thread? _thread;

        public string UserId => userId;

        public NetClient(Action<Operation> applyOperation)
        {
            this.applyOperation = applyOperation;

            listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                string receivedDocumentId = reader.GetString();
                string receivedUserId = reader.GetString();
                Operation operation = DeserializeOperation(reader);

                if (receivedDocumentId == documentId && receivedUserId != userId)
                {
                    applyOperation(operation);
                }
            };

            listener.PeerConnectedEvent += peer =>
            {
                // Send all pending operations to the server
                foreach (var operation in pendingOperations)
                {
                    SendOperation(operation);
                }
                pendingOperations.Clear();
            };

            client = new NetManager(listener);
            client.Start();

            (_thread = new Thread(() =>
            {
                while (client != null)
                {
                    client.PollEvents();
                    Thread.Sleep(50);
                }
            })).Start();
        }

        public void Connect(string address, int port, string connectionKey)
        {
            client.Connect(address, port, connectionKey);
        }

        public bool IsConnected => client.FirstPeer != null && client.FirstPeer.ConnectionState == ConnectionState.Connected;

        public void SendOperation(Operation operation)
        {
            if (IsConnected)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(documentId);
                writer.Put(userId);
                SerializeOperation(writer, operation);
                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                pendingOperations.Add(operation);
            }
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