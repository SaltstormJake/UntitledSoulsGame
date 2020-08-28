using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class RockScript : MonoBehaviour
{
   // public AudioClip boomSound;
   // public AudioSource SoundSource;
    public GameObject smallRock;
    
    // Start is called before the first frame update
    void Start()
    {
        // -40 y velocity selected for maximum survivable scariness
        GetComponent<Rigidbody>().velocity = new Vector3(0f, -40f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < 0)
        {
            Shatter();
        }
    }

    // OnCollisionEnter is called after a collision takes place
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Rock")
        {
          //  SoundSource.clip = boomSound;
          //  SoundSource.Play();
            Shatter();
        }
    }

    void Shatter()
    {
        Transform originalTransform;
        GameObject tempRock;
        Vector3 position, velocity;

      GameObject.Find("Crater/Arena").GetComponent<ArenaScript>().RumbleSound();

        originalTransform = transform;

        // Destroy the rock
        Destroy(gameObject);

        // Spawn several smaller rocks that will last a few seconds
        for (int i = 0; i < 40; i++)
        {
            // Create a randomly placed small rock within the original sphere of the rock.
            position = originalTransform.position + Vector3.Scale(Random.insideUnitSphere, originalTransform.localScale) / 2;
            tempRock = Instantiate(smallRock, position, Quaternion.identity);

            // Give the small rock a velocity as a function of its placement
            velocity = 20f * (tempRock.transform.position - originalTransform.position);
            velocity.y = 0f;
            tempRock.GetComponent<Rigidbody>().velocity = velocity;

			ProjectileScript proj = tempRock.GetComponent<ProjectileScript>();
			if (proj != null) proj.parent = GetComponent<ProjectileScript>().parent;
            // Small rock must despawn as well
            Destroy(tempRock, 1.5f);
        }
    }
}
