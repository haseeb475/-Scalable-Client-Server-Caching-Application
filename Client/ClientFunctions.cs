using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace testclass
{
  public class ClientFunctions : ICache
  {
    static string serverIP = "127.0.0.1";
    static int serverPort = Int32.Parse(ConfigurationManager.AppSettings["PortNumber"]);

    static TcpClient client;
    static NetworkStream stream;
    private Thread responseThread;
    private object responseLock = new object();
    private bool responseReceived = false;
    private ResponseModel responseData;
    private static bool globalCheck = true;
    // Define a delegate for the event handler
    public delegate void EventHandler(string message);

    // Define an event based on the delegate
    public event EventHandler EventOccurred;
    //        private object responseLock = new object();
    public void Initialize()
    {
      try
      {
        client = new TcpClient(serverIP, serverPort);
        stream = client.GetStream();
        // Start a new thread to handle the response
        responseThread = new Thread(() => ReadResponse(client));
        responseThread.IsBackground = true;
        responseThread.Start();

      }
      catch (Exception e)
      {
        throw new Exception("Could Not Connect to Server");
      }
    }

    // Method to raise the event
    protected virtual void OnEventOccurred(string message)
    {
      // Check if there are any subscribers to the event
      EventOccurred?.Invoke(message);
    }
    private void WriteData(string jsonData)
    {
      // Get the size of the serialized data
      int dataSize = jsonData.Length;

      // Create a byte array with the size and data
      byte[] buffer = new byte[dataSize + 4];

      // Convert the size to bytes and copy it to the buffer
      byte[] sizeBytes = BitConverter.GetBytes(dataSize);
      Array.Copy(sizeBytes, buffer, sizeBytes.Length);

      // Copy the serialized data to the buffer
      byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
      Array.Copy(dataBytes, 0, buffer, sizeBytes.Length, dataBytes.Length);

      // Send the data to the server
      stream.Write(buffer, 0, buffer.Length);
      Console.WriteLine("Request sent to server.");
    }

    private void ReadResponse(TcpClient client)
    {
      while (true)
      {
        // Receive the response from the server

        // Read the first 4 bytes to get the size of the stream
        byte[] sizeBuffer = new byte[4];
        int sizeBytesRead = stream.Read(sizeBuffer, 0, sizeBuffer.Length);
        int streamSize = BitConverter.ToInt32(sizeBuffer, 0);
        // Read the remaining stream based on the size received
        byte[] buffer = new byte[streamSize];
        int bytesRead = stream.Read(buffer, 0, streamSize);

        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        var response = JsonConvert.DeserializeObject<ResponseModel>(data);
        Console.WriteLine("Waiting");
        //Console.WriteLine(response.Command);
        if (response.Command == "exit")
        {
          responseThread.Interrupt();
          // Close the client connection
          stream.Close();
          client.Close();

        }
        if (response.Command == "get" || response.Command == "excO")
        {
          lock (responseLock)
          {
            // Store the response data
            responseData = response;
            // Set the flag to indicate that the response has been received
            responseReceived = true;

            // Signal that the response has been processed
            Monitor.Pulse(responseLock);
          }

        }
        if (response.Command == "ack" || response.Command == "exc")
        {
          lock (responseLock)
          {
            // Store the response data
            responseData = response;
            /*                        // Set the flag to indicate that the response has been received
                                    responseReceived = true;
            */
            // Signal that the response has been processed
            Monitor.Pulse(responseLock);
          }

        }
        if (response.Command == "event")
        {
          OnEventOccurred("Something happened!");
        }
      }



    }
    public void SubscribeToServer()
    {
      // Create a data model to hold the command, key, and value
      var data = new DataModel { Command = "sub" };
      // Serialize the data
      string jsonData = JsonConvert.SerializeObject(data);

      // Write the data to the server
      WriteData(jsonData);
    }
    public void Add(string key, object value)
    {

      string newT = Serialize(value);
      Console.WriteLine(newT);
      /*            string newT = JsonConvert.SerializeObject(value,
      *//*            Newtonsoft.Json.Formatting.None,
      *//*            new JsonSerializerSettings
      *//*            {
                      TypeNameHandling = TypeNameHandling.All
                  });
      */
      byte[] serializedObject = Encoding.UTF8.GetBytes(newT);
      // Deserialize the Home instance without explicitly mentioning the type

      /*            object deserializedObject = JsonConvert.DeserializeObject(newT);
                  // Handle the deserialized object on the server
                  Type objectType = deserializedObject.GetType();
                  Console.WriteLine("Received object of type: " + objectType.FullName);
      */            /*            var options = new JsonSerializerOptions
                              {
                                  WriteIndented = true
                              };

                  */
      // Serialize the person object to byte array
      // byte[] objectByte = ;
      // Create a data model to hold the command, key, and value
      var data = new DataModel { Command = "add", Key = key, Object = serializedObject };

      /*            String stringData = Encoding.UTF8.GetString(serializedObject);
                  Console.WriteLine(stringData);
                  var deserializedObject = Deserialize(stringData);
                  Console.WriteLine("It is correct", deserializedObject);
                  // Get the type of the deserialized object
                  Type objectType = deserializedObject.GetType();

                  // Print the type of the deserialized object
                  Console.WriteLine("Deserialized Object Type: " + objectType.FullName);
                  // Serialize the data
                  */
      //string jsonData = JsonConvert.SerializeObject(data);
      string jsonData = Serialize(data);
      // Write the data to the server
      WriteData(jsonData);

      lock (responseLock)
      {
        // Wait for the response
        Monitor.Wait(responseLock);

        // Access the response data
        string command = responseData.Command;
        if (command == "exc")
        {
          // Reset the flag for the next operation
          responseReceived = false;

          // Signal that the response has been processed
          Monitor.Pulse(responseLock);

          string tempExec = responseData.Exception.Message;
          throw new Exception(tempExec);
        }

        /*                // Reset the flag for the next operation
                        responseReceived = false;
        */
        // Signal that the response has been processed
        Monitor.Pulse(responseLock);
      }
    }

    public object Get(string key)
    {

      // Create a data model to hold the command and key
      var data = new DataModel { Command = "get", Key = key };

      // Serialize the data
      string jsonData = JsonConvert.SerializeObject(data);

      WriteData(jsonData);

      lock (responseLock)
      {
        Console.WriteLine("Control here");
        // Wait for the response
        while (!responseReceived)
        {
          Monitor.Wait(responseLock);
        }


        // Access the response data
        string command = responseData.Command;
        if (command == "exc")
        {
          responseReceived = false;
          // Signal that the response has been processed
          Monitor.Pulse(responseLock);

          string tempExec = responseData.Exception.Message;
          throw new Exception(tempExec);
        }
        Console.WriteLine(command);
        Exception exception = responseData.Exception;
        byte[] dataFor = responseData.Object;


        // var receivedObject = DeserializeObject<object>(dataFor);

        string json = Encoding.UTF8.GetString(dataFor);
        // var receivedObject = JsonConvert.DeserializeObject(json);


        // Deserialize the object with type information
        object receivedObject = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All
        });
        /*                Home deserializedHome = receivedObject as Home;

                        Console.WriteLine(deserializedHome.Town);*/

        /*                Console.WriteLine(receivedObject.GetType().Name);
                        Home rety = (Home)receivedObject;
                        Console.WriteLine("jhg", rety.Town);
        */
        /*                Type deserializedHome = (receivedObject.GetType().Name)deserializedObject;
        */
        // Reset the flag for the next operation
        responseReceived = false;
        // Signal that the response has been processed
        Monitor.Pulse(responseLock);
        return receivedObject;
      }

    }

    public void Remove(string key)
    {
      // Create a data model to hold the command and key
      var data = new DataModel { Command = "remove", Key = key };

      // Serialize the data
      string jsonData = JsonConvert.SerializeObject(data);
      WriteData(jsonData);

      /*            lock (responseLock)
                  {
                      // Wait for the response
                      while (!responseReceived)
                      {
                          Monitor.Wait(responseLock);
                      }


                      // Access the response data
                      string command = responseData.Command;
                      // Reset the flag for the next operation
                      responseReceived = false;

                      // Signal that the response has been processed
                      Monitor.Pulse(responseLock);

                  }
      */
    }

    public void Clear()
    {
      // Send the exit command to the server
      var data = new DataModel { Command = "clear" };
      string jsonData = JsonConvert.SerializeObject(data);
      WriteData(jsonData);

      lock (responseLock)
      {
        // Wait for the response
        while (!responseReceived)
        {
          Monitor.Wait(responseLock);
        }


        // Access the response data
        string command = responseData.Command;

        // Reset the flag for the next operation
        responseReceived = false;

        // Signal that the response has been processed
        Monitor.Pulse(responseLock);

      }
    }



    public void Dispose()
    {

      var data = new DataModel { Command = "exit" };
      string jsonData = JsonConvert.SerializeObject(data);
      WriteData(jsonData);

    }
    public static string Serialize<T>(T obj)
    {
      return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.All
      });
    }

    public static object Deserialize(string serializedData)
    {
      return JsonConvert.DeserializeObject(serializedData, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.All
      });
    }
  }
  public class Person
  {
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
  }

  public class Home
  {
    public string Town { get; set; }
    public string Block { get; set; }
  }
}