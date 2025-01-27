using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Meta.Net.NativeWebSocket;
using LearnXR.Core.Utilities;
using OVRSimpleJSON;
using System.Linq;
using System.Text;
public class Connection : MonoBehaviour
{
    WebSocket websocket;
    [SerializeField] private Renderer targetRenderer;

        [SerializeField] private SpatialLogger logger;

        [SerializeField] private TextAsset jsonObj;
        private List<string> messages = new List<string>();
        private string lastMessage;

        private StringBuilder messageBuffer = new StringBuilder();


    // Start is called before the first frame update
    async void Start()
    {

        // TextToImage(jsonObj);

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
            string messagePart = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            Debug.Log("Message received: " + messagePart);

            messageBuffer.Append(messagePart);

            if (IsMessageComplete(messageBuffer.ToString()))
            {
              string completeMessage = messageBuffer.ToString();

             Debug.Log("Complete message received: " + completeMessage); 

                     messageBuffer.Clear();

            messages.Add(completeMessage);
            }

            // logger.LogInfo(message);
            // getting the message as a string
            // var message = System.Text.Encoding.UTF8.GetString(bytes);
            // Debug.Log("OnMessage! " + message);
            // ImgMsgDecode(message);
        };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendPrompt", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();

    }

private bool IsMessageComplete(string message)
{
    return message.EndsWith("}");
}
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // websocket.DispatchMessageQueue();
#endif
    if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetUp(OVRInput.RawButton.A))
    {
        StartCoroutine(DecodeImage());   
    }

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

    public void ImgMsgDecode(string msg)
    {
        ImageMSG myMsg = JsonUtility.FromJson<ImageMSG>(msg);
        StringToImage(myMsg);
    }

    public void StringToImage(ImageMSG msg)
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

    public void TextToImage(TextAsset text)
    {
        ImageMSG mymsg = JsonUtility.FromJson<ImageMSG>(text.text);
        StringToImage(mymsg);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    IEnumerator DecodeImage()
    {
        //TextToImage(jsonObj);
            ImgMsgDecode(messages[messages.Count - 1]);
        yield return 0;
    }


}
