using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class WallFader : MonoBehaviour
{
    public float fadeSpeed = 2f;
    public string tagName = "Wall";

    private List<GameObject> fadingWalls;

    // Start is called before the first frame update
    void Start()
    {
        fadingWalls = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
		// Define a raycast from our GameObject to the camera
        int layer = ~(1 << 8);
        Vector3 vec = (Camera.main.transform.position - transform.position);

        RaycastHit[] hs = Physics.RaycastAll(transform.position, vec.normalized, vec.magnitude, layer);
        List<GameObject> gs = new List<GameObject>(); // Keep track of who we've seen this frame

		// Look at every object the ray passed through
        foreach(RaycastHit hit in hs)
        {
            GameObject g = hit.collider.gameObject;
            gs.Add(g); // Add this object to our list

			// Make sure we've found a wall
            if (hit.collider.tag != tagName || fadingWalls.Contains(g)) continue;

			// Officially admit the wall to the Faded Walls Club and begin the fading
            fadingWalls.Add(g);
            StartCoroutine(FadeWall(g));
        }

		// Check up on our members
        foreach(GameObject g in fadingWalls)
        {
			// If we haven't seen them in the last 1/60th of a second, they are dead to us.
            if (!gs.Contains(g))
                fadingWalls.Remove(g);
        }
    }

	// Fade those walls
    IEnumerator FadeWall(GameObject wall)
    {
        Material m = wall.GetComponent<Renderer>().material;
        Color c = m.color;
        float fade = c.a;

		// For as long as this wall is a member of the club, fade it out.
        while(fadingWalls.Contains(wall))
        {
            c = m.color;
            m.color = new Color(c.r, c.g, c.b, fade);
            fade -= fadeSpeed * Time.deltaTime;
            fade = Mathf.Max(0f, fade);

            yield return null;
        }

		// Fade it back in once it's left
        while(fade < 1f)
        {
            c = m.color;
            m.color = new Color(c.r, c.g, c.b, fade);
            fade += fadeSpeed * Time.deltaTime;
            fade = Mathf.Min(1f, fade);

            yield return null;
        }
    }
}
