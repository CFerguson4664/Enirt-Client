using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

public class netComs : MonoBehaviour
{
    public bool enableNetworking;
    NetworkStream networkStream;
    float time;
    float counter;
    int socketId;

    // Start is called before the first frame update
    void Start()
    {
        if (enableNetworking)
        {
            networkStream = ConnectTCPClient("127.0.0.1", 8124);
            socketId = int.Parse(receiveMessage(networkStream));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(enableNetworking)
        {
            time += Time.deltaTime;

            if (time > 1)
            {
                counter += 1;
                sendMessage("This is a message " + counter, networkStream);
                time -= 1;
            }

            if (networkStream.DataAvailable)
            {
                Debug.Log(receiveMessage(networkStream));
            }
        }
    }

    NetworkStream ConnectTCPClient(String serverAddress, int port)
    {
        // Create a TcpClient.
        TcpClient client = new TcpClient(serverAddress, port);

        // Get a client stream for reading and writing.
        NetworkStream stream = client.GetStream();

        return stream;
    }

    void sendMessage(String message, NetworkStream stream)
    {
        // Translate the passed message into ASCII and store it as a Byte array.
        Byte[] data = System.Text.Encoding.ASCII.GetBytes(socketId + "," + message);

        // Send the message to the connected TcpServer.
        stream.Write(data, 0, data.Length);
    }

    String receiveMessage(NetworkStream stream)
    {
        // Buffer to store the response bytes.
        Byte[] data = new Byte[256];

        // String to store the response ASCII representation.
        String responseData = String.Empty;

        // Read the first batch of the TcpServer response bytes.
        Int32 bytes = stream.Read(data, 0, data.Length);

        // Translate the passed message into a string
        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

        return responseData;
    }
}
