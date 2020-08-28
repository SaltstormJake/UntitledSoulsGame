using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class HealthBarScript : MonoBehaviour
{
	public HealthScript obj;
	public int widthMin = 0;
	public int widthMax = 500;

    // Update is called once per frame
    void Update()
    {
		RectTransform rt = GetComponent<RectTransform>();

		// Adjust health bar width based on object's health percentage
		Vector2 size = rt.sizeDelta;
		rt.sizeDelta = new Vector2(Mathf.Lerp((float)widthMin, (float)widthMax, obj.Health), size.y);
    }
}
