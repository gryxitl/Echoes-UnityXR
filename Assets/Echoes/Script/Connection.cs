using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Meta.Net.NativeWebSocket;
using LearnXR.Core.Utilities;
public class Connection : MonoBehaviour
{
    WebSocket websocket;
    [SerializeField] private Renderer targetRenderer;

        [SerializeField] private SpatialLogger logger;
    // Start is called before the first frame update
    async void Start()
    {
        // websocket = new WebSocket("ws://localhost:3000");
        websocket = new WebSocket("ws://192.168.137.1:9982");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

            websocket.OnMessage += (bytes, offset, count) =>
        {
            Debug.Log("OnMessage!");
            string message = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            Debug.Log("Message received: " + message);
            logger.LogInfo(message);
            // getting the message as a string
            // var message = System.Text.Encoding.UTF8.GetString(bytes);
            // Debug.Log("OnMessage! " + message);
            // ImageMSG msg = new ImageMSG(); 
            // msg = ImgMsgDecode(message);
            // JsonToImage(msg);
        };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendPrompt", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // websocket.DispatchMessageQueue();
#endif
    }

    // async void SendWebSocketMessage()
    // {
    //     if (websocket.State == WebSocketState.Open)
    //     {
    //         // Sending bytes
    //         await websocket.Send(new byte[] { 10, 20, 30 });

    //         // Sending plain text
    //         await websocket.SendText("plain text message");
    //     }
    // }

    public async void SendPrompt(string prompt)
    {
        // Create the JSON object
        var json = new
        {
            type = "prompt",
            content = prompt,
            timestamp = System.DateTime.UtcNow.ToString("o")
        };

        // Convert JSON object to string
        string jsonString = JsonUtility.ToJson(json);

        // Send JSON string
        await websocket.SendText(jsonString);

        Debug.Log($"Sent: {jsonString}");
    }

    public ImageMSG ImgMsgDecode(string msg)
    {
        ImageMSG myMsg = new ImageMSG();
        myMsg = JsonUtility.FromJson<ImageMSG>(msg);
        return myMsg;
    }

    public void JsonToImage(ImageMSG msg)
    {
        try {

    // Assuming base64 encoded image received*
    
    byte[] imageBytes = Convert.FromBase64String(msg.imageData);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                
                targetRenderer.material.mainTexture = texture;
            }
            catch (Exception ex) {
                Debug.Log("Image processing error: " + ex.Message);
            }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }



}
