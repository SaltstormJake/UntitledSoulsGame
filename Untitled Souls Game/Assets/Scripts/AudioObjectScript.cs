using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class AudioObjectScript : MonoBehaviour
{
    public AudioClip MusicClip;

    public AudioSource MusicSource;

    // Start is called before the first frame update
    void Start()
    {
        MusicSource.clip = MusicClip;
        MusicSource.Play();
        MusicSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
