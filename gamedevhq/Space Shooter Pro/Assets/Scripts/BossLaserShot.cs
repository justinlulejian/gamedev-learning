using UnityEngine;

public class BossLaserShot : MonoBehaviour
{
    private Vector3 _scaleIncrement = new Vector3(-0.05f, -0.05f, 0);
    private AudioSource _shotAudio;


    private void Start()
    {
        _shotAudio = this.transform.GetComponent<AudioSource>();
        
        if (_shotAudio == null) {
            Debug.LogError("Shot audio source not found on boss laser shot.");
        }
        
        _shotAudio.Play();
    }

    void Update()
    {
        // Grow/shrink sprite as an animation.
        transform.localScale += _scaleIncrement;
        if (transform.localScale.x < 5.0f || transform.localScale.x > 5.5f)
        {
            _scaleIncrement = -_scaleIncrement;
        }
        
    }

    private void OnDestroy()
    {
        _shotAudio.Stop();
    }
}
