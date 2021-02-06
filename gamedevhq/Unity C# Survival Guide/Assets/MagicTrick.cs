using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTrick : MonoBehaviour
{

    private MeshRenderer _meshRenderer;
    private WaitForSeconds _changeColorWaitSeconds = new WaitForSeconds(3.0f);
    
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = this.GetComponent<MeshRenderer>();
        StartCoroutine(UnHideOnKey());
        StartCoroutine(RandomChangeColorSeconds());

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator RandomChangeColorSeconds()
    {
        while (true)
        {
            Color randomColor = Random.ColorHSV();
            _meshRenderer.material.color = randomColor;
            Debug.Log($"Turned cube random color: {randomColor.ToString()}");
            yield return _changeColorWaitSeconds;
        }
    }

    private IEnumerator UnHideOnKey(KeyCode key = KeyCode.I)
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.I) && _meshRenderer.enabled)
            {
                Debug.Log("Hiding meshrender of cube with key.");
                _meshRenderer.enabled = false;
                yield return new WaitForSeconds(5.0f);
                Debug.Log("Re-enabling meshrender of cube.");
                _meshRenderer.enabled = true;
            }
            yield return null;
        }
    }
}
