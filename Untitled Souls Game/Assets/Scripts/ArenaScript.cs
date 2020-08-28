using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class ArenaScript : MonoBehaviour
{
    public AudioClip boomSound;
    public AudioSource SoundSource;

    public void RumbleSound()
    {
        SoundSource.clip = boomSound;
        SoundSource.Play();
    }
}
