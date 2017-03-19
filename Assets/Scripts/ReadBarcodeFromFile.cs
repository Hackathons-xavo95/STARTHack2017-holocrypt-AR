using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using CielaSpike.Unity.Barcode;


public class ReadBarcodeFromFile : MonoBehaviour
{
    public GameObject menu, messageOverlay;
    public Text messageText;
    public InputField usernameField, passwordField;
    public Button decrypt, exitMessage;

    private WebCamTexture webcam;
    private IBarcodeDecoder qr_decoder;
    private bool isRunning;
    private string id;

    

    void Start()
    {
        menu.GetComponent<Canvas>().enabled = false;
        messageOverlay.GetComponent<Canvas>().enabled = false;
        decrypt.GetComponent<Button>().onClick.AddListener(Submit);
        exitMessage.GetComponent<Button>().onClick.AddListener(RemoveMessageBox);

        qr_decoder = Barcode.GetDecoder();

        WebCamDevice[] deviceList = WebCamTexture.devices;
        if (deviceList.Length > 0)
        {
            webcam = new WebCamTexture(deviceList[0].name);
            GetComponent<Renderer>().material.mainTexture = webcam;
            webcam.Play();
        }
    }

    private void Update()
    {
        var barcodeBitmap = webcam.GetPixels32();
        var result = qr_decoder.Decode(barcodeBitmap, webcam.width, webcam.height);
        if (result.Success)
        {
            if((result.BarcodeType == BarcodeType.QrCode) && !menu.GetComponent<Canvas>().enabled)
            id = result.Text;
            menu.GetComponent<Canvas>().enabled = true;
        }
    }

    private void Submit()
    {
        if (isRunning == false)
        {
            StartCoroutine(GetRoutine());
        }
    }

    IEnumerator GetRoutine()
    {
        isRunning = true;
        String requestStr = string.Format("https://holocrypt.herokuapp.com/api/messages/get?id={0}&username={1}&password={2}", id, usernameField.text, passwordField.text);
        passwordField.text = "";
        WWW request = new WWW(requestStr);
        yield return request;

        menu.GetComponent<Canvas>().enabled = false;
        messageOverlay.GetComponent<Canvas>().enabled = true;
        string[] response = request.text.Replace("{", string.Empty).Replace("}", string.Empty).Split(':');
        if (response.Length > 0)
        {
            if (response[1].Contains("\""))
                messageText.text = response[1].Replace("\"", string.Empty).Replace(@"\r\n", Environment.NewLine);
            else
            {
                String error = "Your username or " + Environment.NewLine + "password doesn't match " + Environment.NewLine + "for that message";
                messageText.text = error;
            }
        }
        else
            messageText.text = "Got connectivity errors";

        isRunning = false;
    }

    private void RemoveMessageBox()
    {
        messageOverlay.GetComponent<Canvas>().enabled = false;
    }
}