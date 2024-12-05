using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class UDPVideoReceiver : MonoBehaviour
{
    public int port = 3000; // Match the Python UDP port
    private UdpClient udpClient;
    private Texture2D texture;
    private List<byte> imageBuffer = new List<byte>();

    void Start()
    {
        udpClient = new UdpClient(AddressFamily.InterNetworkV6); // Use IPv6
        udpClient.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false); // Allow IPv4 fallback
        udpClient.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, port));

        texture = new Texture2D(2, 2); // Placeholder size; will update later
        Application.runInBackground = true;

        // Start receiving video
        ReceiveVideo();
    }

    async void ReceiveVideo()
    {
        while (true)
        {
            var result = await udpClient.ReceiveAsync();
            byte[] data = result.Buffer;

            if (data.Length == 1) // End-of-frame marker
            {
                // Assemble the full frame
                byte[] imageData = imageBuffer.ToArray();
                imageBuffer.Clear();

                // Decode the image and apply it to the texture
                texture.LoadImage(imageData);
                GetComponent<Renderer>().material.mainTexture = texture;
            }
            else
            {
                // Accumulate chunks into the buffer
                imageBuffer.AddRange(data);
            }
        }
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}

