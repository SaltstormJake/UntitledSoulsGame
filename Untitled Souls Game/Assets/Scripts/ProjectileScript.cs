using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class ProjectileScript : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 2f;
	public bool destroyOnGenericHit = true;
	public bool noDestroy = false;

    public GameObject parent;

    private void Start()
    {
		// Start our lifetime counter to kill the projectile after a set amount of time
		if (lifeTime >= 0f)
			Destroy(gameObject, lifeTime);
    }

	private void Update()
	{
		//transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity.normalized, transform.up);
	}

    private void OnCollisionEnter(Collision collision)
    {
		bool collided = false;

		// Look through all the current contacts
        foreach(ContactPoint contact in collision.contacts)
        {
			// If this contact is our parent, we don't want to damage it!
            if (parent != null && contact.otherCollider.gameObject == parent) continue;

			// We've found a valid collision
			collided = true;

			// Grab a HealthScript; if it's not there, we can't damage it.
			HealthScript dmg = contact.otherCollider.gameObject.GetComponent<HealthScript>();
            if (dmg == null) continue;

			// cause pain
			dmg.Damage(damage);

			// Destroy the projectile unless destruction is being overridden.
			if(!noDestroy)
				Destroy(gameObject, 0f);
			break;
        }

		// Destroy the projectile if we hit something, unless destruction is being overridden.
		if (collided && destroyOnGenericHit && !noDestroy)
			Destroy(gameObject, 0f);
    }
}
