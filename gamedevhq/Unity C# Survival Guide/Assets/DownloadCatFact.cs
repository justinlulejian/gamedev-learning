using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
public class Root    {
    public string fact { get; set; } 
    public int length { get; set; } 
}


public class DownloadCatFact : MonoBehaviour
{
    [SerializeField]
    private string _url;
    [SerializeField]
    private Text _catFactText;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GetRequest(_url));
        }
    }

    private IEnumerator GetRequest(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();
        
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log("Error: " + uwr.error);
        }
        else
        {
           string catFact = uwr.downloadHandler.text;
           Root myCatJson = JsonConvert.DeserializeObject<Root>(catFact);
           _catFactText.text = myCatJson.fact;
        }
    }
}
