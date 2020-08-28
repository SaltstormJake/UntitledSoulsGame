using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class TorchLightScript : MonoBehaviour
{
    public float minimumIntensity, maximumIntensity, flickerPeriod;

    private float amplitude, mean, sinPeriodManip;
    private Light torchLight;
    
    // Start is called before the first frame update
    void Start()
    {
        torchLight = gameObject.GetComponent<Light>();

        // Set useful values for intensity calculation
        amplitude = (maximumIntensity - minimumIntensity) / 2;
        mean = (minimumIntensity + maximumIntensity) / 2;
        sinPeriodManip = 2 * Mathf.PI / flickerPeriod;
    }

    // Update is called once per frame
    void Update()
    {
        // One-liner to chart current intensity based on time since start of game
        torchLight.intensity = amplitude * Mathf.Sin(sinPeriodManip * Time.time) + mean;
    }
}
