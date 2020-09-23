using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

public class netComs : MonoBehaviour
{
    public bool enableNetworking;
    NetworkStream networkStream;
    public String serverIpAddress;
    float time;
    float counter;
    int socketId;

    // Start is called before the first frame update
    void Start()
    {
        if (enableNetworking)
        {
            networkStream = ConnectTCPClient(serverIpAddress, 8124);
            socketId = int.Parse(receiveMessage(networkStream));
            clockSync(networkStream);
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
                sendMessage(0 + ",This is a message " + counter, networkStream);
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
        Byte[] data = new Byte[4096];

        // String to store the response ASCII representation.
        String responseData = String.Empty;

        // Read the first batch of the TcpServer response bytes.
        Int32 bytes = stream.Read(data, 0, data.Length);

        // Translate the passed message into a string
        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

        return responseData;
    }

    long clockSync(NetworkStream stream)
    {
        int numPings = 5;
        double sumOfOffsets = 0;

        for (int i = 0; i < numPings; i++)
        {
            long firstTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            sendMessage(1 + ",0", stream);
            String reply = receiveMessage(stream);

            long secondTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            long serverTime = long.Parse(reply);

            long sendTime = serverTime - firstTime;
            long receiveTime = secondTime - serverTime;

            long elapsedTime = secondTime - firstTime;
            long singleDirectionTime = elapsedTime / 2;

            //If offset time is less that 0 the server is ahead
            //If it is greater than 0 the server is behind
            long offsetTime1 = singleDirectionTime - sendTime;
            long offestTime2 = receiveTime - singleDirectionTime;

            Debug.Log("Offset time 1: " + offsetTime1);
            Debug.Log("Offset time 2: " + offestTime2);

            sumOfOffsets += offsetTime1;
            sumOfOffsets += offestTime2;
        }

        long averageOffset = (long)(sumOfOffsets / (numPings * 2));

        Debug.Log("The average Offset is: " + averageOffset + " ms");

        return averageOffset;
    }
}
