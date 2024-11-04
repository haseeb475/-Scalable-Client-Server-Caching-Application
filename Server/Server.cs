using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Drawing.Printing;
using Newtonsoft.Json;
using Common;
using testings;
using System.Security.AccessControl;
using System.Collections.Concurrent;

public class Host
{
  static void Main()
  {
    Server1 startingServer = new Server1();
        startingServer.StartServer();
  }
}

public class EventData
{
    public TcpClient Client { get; set; }
    public SendCommandsEnum EventType { get; set; }
}

public class Server1
{

    CacheServer serverStoreFunc = new CacheServer();
    EventHandle eventHandle = new EventHandle();
    RequestHandle requestHandle = new RequestHandle();
    TcpListener listener = null;


    public void StartServer()
    {
        try
        {

            int port = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"] ?? "8080");
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server listening on port {port}");
            this.serverStoreFunc.Initialize();
            TcpClient client;

            try
            {
                Thread eventThread = new Thread(eventHandle.EventHandlingThread);
                eventThread.IsBackground = true;
                eventThread.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            while (true)
            {
                try
                {
                    client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected to client");
                    Thread thread = new Thread(() => HandleClient(client));
                    thread.Start();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    
    public void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        bool Check = true;
        try
        {
            while (Check)
            {
                string data = requestHandle.RequestReceived(stream);
                // Deserialize received data
                var receivedData = JsonConvert.DeserializeObject<DataModel>(data);
                // Extract the command, key, and object from receivedData
                CommandsEnums command = receivedData.Command;
                string key = receivedData.Key;
                // Process the data and create the response
                if (command == CommandsEnums.subscribe)
                {
                    eventHandle.AddSubscribedClients(client);
                }
                else if (command == CommandsEnums.add)
                {
                    try
                    {
                        serverStoreFunc.Add(key, receivedData.Object);
                        requestHandle.RequestToClient(stream, SendCommandsEnum.ack, exception: null, null);
                        eventHandle.AddEvent(SendCommandsEnum.eventFromServerAdd, client);
                    }
                    catch (Exception ex)
                    {
                        requestHandle.RequestToClient(stream, SendCommandsEnum.exc, ex, null);
                    }
                }
                else if (command == CommandsEnums.get)
                {
                    byte[]? bytesArray = null;
                    try
                    {
                        bytesArray = serverStoreFunc.Get(key) as byte[];
                        byte[] objectBuffer = requestHandle.AppendLength(bytesArray);
                        // Convert the size to bytes and copy it to the buffer
                        byte[] sizeBytes = BitConverter.GetBytes(bytesArray.Length);
                        Array.Copy(bytesArray, 0, objectBuffer, sizeBytes.Length, bytesArray.Length);
                        requestHandle.RequestToClient(stream, SendCommandsEnum.get, exception: null, bytesArray);
                    }
                    catch (Exception ex)
                    {
                        requestHandle.RequestToClient(stream, SendCommandsEnum.exc, ex, null);
                    }
                }
                else if (command == CommandsEnums.remove)
                {
                    serverStoreFunc.Remove(key);
                    requestHandle.RequestToClient(stream, SendCommandsEnum.ack, exception: null, null);
                    eventHandle.AddEvent(SendCommandsEnum.eventFromServerRemove, client);
                }
                else if (command == CommandsEnums.clear)
                {
                    serverStoreFunc.Clear();
                    requestHandle.RequestToClient(stream, SendCommandsEnum.ack, exception: null, null);
                    eventHandle.AddEvent(SendCommandsEnum.eventFromServerClear, client);
                }
                else if (command == CommandsEnums.exit)
                {
                    eventHandle.RemoveSubscribedClients(client);
                    requestHandle.RequestToClient(stream, SendCommandsEnum.exit, exception: null, null);
                    break;
                }
            }
        }

        catch (Exception)
        {
            try
            {
                eventHandle.RemoveSubscribedClients(client);
                /*                requestHandling.RequestToClient(stream, SendCommandsEnum.exc, ex, null);
                */
            }
            catch (Exception)
            {
                Console.WriteLine("Connection Forcefully Closed");
            }
        }
        finally
        {
            try
            {
                client.Close();
                Console.WriteLine("Client disconnected");
                stream.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    public void StopServer()
    {
        try
        {
            // Stop accepting new clients
            listener.Stop();

        }
        catch (Exception)
        {
            Console.WriteLine("Shutdowm Fail");
        }
    }

}