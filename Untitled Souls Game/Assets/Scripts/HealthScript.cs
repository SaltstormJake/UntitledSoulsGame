using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class HealthScript : MonoBehaviour, IDamagable
{
    public int hp = 100;
    public int maxHP = 100;

	public int iframes = 0;

    public int HitPoints
    {
        get => hp;
        set
		{
			hp = value;
			// If HP is zero or less, trigger OnDeath so our death callbacks work even if you don't use Damage
			if (hp <= 0)
			{
				hp = 0;
				OnDeath();
			}
		}
    }
    public int MaxHitPoints => maxHP;
    public float Health => (float)hp / (float)maxHP;
	public bool Invincible => iframes > 0;
	public bool Dead => hp <= 0;

	// AddInvincibility(int frames)
	// Adds iframes to make us invincible
	public virtual int AddInvincibility(int frames)
	{
		iframes += frames;

		OnInvincible();

		return iframes;
	}

	// Called once per frame
	protected virtual void Update()
	{
		if (iframes > 0)
		{
			iframes -= 1;
			// Call our callback if we are out of iframes
			if (iframes <= 0)
			{
				iframes = 0;
				OnVulnerable();
			}
		}
	}

	// Damage(int dmg)
	// Inflict damage, die if necessary
	public virtual int Damage(int dmg)
	{
		if (Invincible) return HitPoints;

		HitPoints -= dmg;
		OnDamage(dmg);

		// Check if we should be dead
		if(HitPoints <= 0)
		{
			OnDeath();
			HitPoints = 0;
		}

		return HitPoints;
	}

	// Immediately inflict maximum damage
	public virtual void Kill()
	{
		Damage(MaxHitPoints);
	}

	// Called upon death
	protected virtual void OnDeath()
	{

	}

	// Called upon taking damage
	protected virtual void OnDamage(int dmg)
	{

	}

	// Called upon becoming invincible
	protected virtual void OnInvincible()
	{

	}

	// Called upon becoming vulnerable
	protected virtual void OnVulnerable()
	{

	}
}
