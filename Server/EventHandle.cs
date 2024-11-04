using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace testings
{
    public class EventHandle
    {
        private ConcurrentQueue<EventData> eventQueue = new ConcurrentQueue<EventData>();
        public List<TcpClient> subscribedClients = new List<TcpClient>();
        object lockObject = new object();

        public byte[] AppendLength(byte[] bytesArray)
        {
            // Get the size of the serialized data
            int dataSize = bytesArray.Length;

            // Create a byte array with the size and data
            byte[] objectBuffer = new byte[dataSize + 4];

            // Convert the size to bytes and copy it to the buffer
            byte[] sizeBytes = BitConverter.GetBytes(dataSize);
            Array.Copy(sizeBytes, objectBuffer, sizeBytes.Length);

            Array.Copy(bytesArray, 0, objectBuffer, sizeBytes.Length, bytesArray.Length);

            return objectBuffer;
        }

        public void SendEventToClients(SendCommandsEnum typeOfEvent,TcpClient currentClient)
        {
            var response = new ResponseModel
            {
                Command = typeOfEvent
            };

            string eventData = JsonConvert.SerializeObject(response);

            byte[] eventDataBytes = Encoding.UTF8.GetBytes(eventData);

            byte[] objectBuffer = AppendLength(eventDataBytes);

            lock (lockObject)
            {
                foreach (var client in subscribedClients)
                {
                    if (client != currentClient)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(objectBuffer, 0, objectBuffer.Length);
                        stream.Flush();
                    }
                }
            }
        }

        public void DelegateEvent(SendCommandsEnum typeOfEvent,TcpClient client)
        {
            if (subscribedClients.Count != 0)
            {
                SendEventToClients(typeOfEvent,client);
            }
        }

        public void AddSubscribedClients(TcpClient client)
        {
            lock (lockObject)
            {
                subscribedClients.Add(client);
            }
        }

        public void RemoveSubscribedClients(TcpClient client)
        {
            lock (lockObject)
            {
                if (subscribedClients.Contains(client))
                {
                    subscribedClients.Remove(client);
                }
            }
        }

        public void EventHandlingThread()
        {
            while (true)
            {
                if (eventQueue.TryDequeue(out EventData eventData))
                {
                    TcpClient client = eventData.Client;
                    SendCommandsEnum eventType = eventData.EventType;
                    switch (eventType)
                    {
                        case SendCommandsEnum.eventFromServerAdd:
                            DelegateEvent(SendCommandsEnum.eventFromServerAdd, client);
                            break;
                        case SendCommandsEnum.eventFromServerRemove:
                            DelegateEvent(SendCommandsEnum.eventFromServerRemove, client);
                            break;
                        case SendCommandsEnum.eventFromServerClear:
                            DelegateEvent(SendCommandsEnum.eventFromServerClear, client);
                            break;
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void AddEvent(SendCommandsEnum eventType, TcpClient client)
        {
            EventData eventData = new EventData
            {
                Client = client,
                EventType = eventType
            };
            eventQueue.Enqueue(eventData);
        }



    }
}