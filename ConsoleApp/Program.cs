using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using testclass;
class ClientConsole
{
  // Event handler method in the console app
  private static void Client_EventOccurred(string message)
  {
    Console.WriteLine("Event received: " + message);
  }
  static void Main()
  {
    ClientFunctions currentInstance = new ClientFunctions();
    currentInstance.Initialize();
    Console.WriteLine("Subscription ('yes', 'no')");
    string subscription = Console.ReadLine();
    if (subscription == "yes")
    {
      currentInstance.SubscribeToServer();
    }
    currentInstance.EventOccurred += Client_EventOccurred;

    while (true)
    {
      try
      {
        Console.WriteLine("Enter a command ('add', 'get', 'remove', 'clear', or 'exit'): ");
        string command = Console.ReadLine();

        if (command == "exit")
        {
          currentInstance.Dispose();
          break;
        }

        Console.WriteLine("Enter a key: ");
        string key = Console.ReadLine();

        if (command == "add")
        {
          // Create a sample person object
          var person = new Person
          {
            Name = "xyz",
            Age = 22,
            Email = "xyz@gmail.com"
          };
          var home = new Home
          {
            Town = "Model Town",
            Block = "38C"
          };

          currentInstance.Add(key, person);

        }
        else if (command == "get")
        {
          var valueFromClient = currentInstance.Get(key);
          /*                    Console.WriteLine(valueFromClient.GetType());
          */
          Console.WriteLine(valueFromClient);

        }

        else if (command == "remove")
        {
          currentInstance.Remove(key);

        }

        else if (command == "clear")
        {
          currentInstance.Clear();

        }

        else
        {
          Console.WriteLine("Invalid command. Please try again.");
        }
      }

      catch (Exception ex)
      {
        Console.WriteLine("An error occurred: " + ex.Message);
      }
    }
    // Unsubscribe from the event before exiting
    currentInstance.EventOccurred -= Client_EventOccurred;
  }
}

class Person
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
