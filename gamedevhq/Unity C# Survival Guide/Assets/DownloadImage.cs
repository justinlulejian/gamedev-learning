using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownloadImage : MonoBehaviour
{
    [SerializeField]
    private string _url;
    [SerializeField]
    private RawImage _rawImage;

    [SerializeField] private Text _catFactText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GetRequest(_url));
        }
    }

    private IEnumerator GetRequest(string url)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();
        
        if (uwr.isNetworkError)
        {
            Debug.Log("Error: " + uwr.error);
        }
        else
        {

            _rawImage.texture = DownloadHandlerTexture.GetContent(uwr);
        }
    }
}
