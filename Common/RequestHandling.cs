using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RequestHandle
    {
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

        public void RequestToClient(NetworkStream stream, SendCommandsEnum cmd, Exception? exception, byte[]? dataStored)
        {
            var response = new ResponseModel
            {
                Command = cmd,
                Exception = exception,
                Object = dataStored
            };
            string responseJson = JsonConvert.SerializeObject(response);
            byte[] dataBytes = Encoding.UTF8.GetBytes(responseJson);
            byte[] objectBufferForClient = AppendLength(dataBytes);
            stream.Write(objectBufferForClient, 0, objectBufferForClient.Length);
        }

        public string RequestReceived(NetworkStream stream)
        {
            // Read the first 4 bytes to get the size of the stream
            byte[] sizeBuffer = new byte[4];
            int sizeBytesRead = stream.Read(sizeBuffer, 0, sizeBuffer.Length);
            int streamSize = BitConverter.ToInt32(sizeBuffer, 0);
            // Read the remaining stream based on the size received
            byte[] buffer = new byte[streamSize];
            int bytesRead = stream.Read(buffer, 0, streamSize);
            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return data;
        }

    }
}