using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
 * Authors:
 * Jordan Gilbreath
 * Graham Porter
 * Jake Smith
 */
public class PlayerScript : HealthScript
{
    public AudioClip fireballSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip dodge;
    public AudioSource SoundSource;
    public AudioSource MusicSource;

	public static int[] DodgeLevels = { 30, 20, 10, 5 };
	public float moveSpeed = 0.5f;
	public float dodgeDist = 5f;
	public float gravity = 9.8f;

	public GameObject playerObj;
	public Material normalMat;
	public Material invulnMat;
	public GameObject deathUI;

	public int dodgeLevel = 0;

	public float dodgeTime = 1.5f;
	public float dodgeLevelTimeout = 2.0f;

	public bool testRespawn = false;

	public float fireCooldown = 1f;

	public GameObject projectilePrefab;
	public float projectileForce = 1000f;
	public float projectileSpawnRadius = 1f;
	public Vector3 projectileSpawnOffset = new Vector3(0, 1f, 0);

	private Vector3 lastInput;
	private float lastDodge = float.MinValue;
	private float lastFire = float.MinValue;

	private float vSpeed = 0f;

	private bool dodgeDown = false;
    private bool canFireball;

	private Vector3 spawnPosition;

    private GameObject staff, robe, hat;

	public bool Dodging => Time.fixedTime - lastDodge < dodgeTime;
	public bool CanFire => Time.fixedTime - lastFire >= fireCooldown;
	public float DodgeSpeed => dodgeDist / dodgeTime;

	// Start is called before the first frame update
	void Start()
	{
		// Add spawn invincibility and set our spawn point
		AddInvincibility(100);
		spawnPosition = transform.position;

		// Set up references and related scene objects
		deathUI = GameObject.Find("Canvas/DeathPanel");
		if (deathUI != null) deathUI.SetActive(false);

		staff = GameObject.Find("Player/Player Body/Staff");
        robe = GameObject.Find("Player/Player Body/Robe");
        hat = GameObject.Find("Player/Player Body/Hat");
        staff.SetActive(false);
        robe.SetActive(false);
        hat.SetActive(false);
        canFireball = false;
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();

		Vector3 unscaledMovement;
		Vector3 scaledMovement;
		CharacterController cont = GetComponent<CharacterController>();

		// Normal movement
		if (!Dodging && !Dead)
		{
			// Reset rotation (in case our dodge roll kept us tilted)
			Vector3 rot = playerObj.transform.localEulerAngles;
			rot.x = 0;
			playerObj.transform.localEulerAngles = rot;

			// Set up movement vector based on input
			unscaledMovement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
			scaledMovement = unscaledMovement * moveSpeed * Time.deltaTime;
		}
		else if(!Dead) // Dodge movement
		{
			// Maintain our direction regardless of input
			unscaledMovement = lastInput;
			scaledMovement = lastInput * DodgeSpeed * Time.deltaTime;
		}
		else // Disable movement on death
		{
			unscaledMovement = Vector3.zero;
			scaledMovement = Vector3.zero;
		}

		// Kill vertical speed if we're grounded
		if (cont.isGrounded) vSpeed = 0f;
		// Add gravitational acceleration, and then apply velocity to our movement vector
		vSpeed -= gravity * Time.deltaTime;
		scaledMovement.y += vSpeed * Time.deltaTime;

		// Execute the determined movement
		cont.Move(scaledMovement);

		// Rotate the player model if we need to & keep track of last input
		if (unscaledMovement.magnitude > 0 && !Dodging)
			playerObj.transform.forward = unscaledMovement;
		lastInput = unscaledMovement;

		// Dodge
		if (Input.GetAxis("Jump") > 0)
		{
			// dodgeDown prevents the player from holding the dodge button to dodge
			if (!dodgeDown)
				Dodge();

			dodgeDown = true;
		}
		else
			dodgeDown = false;

		// testRespawn will immediately respawn the player, used for debugging
		if (testRespawn)
		{
			testRespawn = false;
			Respawn();
		}

        // Attack
        if (Input.GetMouseButtonDown(0) && canFireball)
            Fire();
         

		// Instantly kill player if they're falling into the void
        if (transform.position.y < -14)
            Kill();
	}

    void OnTriggerEnter(Collider other)
    {
		// Handle equipment pickups
        if (other.gameObject.tag == "Pickup")
        {
            if (other.gameObject.name == "Staff")
                staff.SetActive(true);
            else if (other.gameObject.name == "Robe")
                robe.SetActive(true);
            else if (other.gameObject.name == "Hat")
                hat.SetActive(true);
            Destroy(other.gameObject);
            if (staff.activeSelf && robe.activeSelf && hat.activeSelf)
                canFireball = true;
        }
    }

    protected override void OnInvincible()
	{
		base.OnInvincible();

		// Apply our invincibility material
		playerObj.GetComponent<Renderer>().material = invulnMat;
	}

	protected override void OnVulnerable()
	{
		base.OnVulnerable();

		// Apply our normal material
		playerObj.GetComponent<Renderer>().material = normalMat;
	}

	// Execute a dodge
	void Dodge()
	{
		if (Dodging || Dead) return;

		// Increment our dodge level to reduce dodge effectiveness
		dodgeLevel++;
		if (dodgeLevel >= DodgeLevels.Length) dodgeLevel = DodgeLevels.Length - 1;

		// Add dodge invincibility
		AddInvincibility(DodgeLevels[dodgeLevel]);

		lastDodge = Time.fixedTime;

		// Start timer to give our sweet iframes back
		CancelInvoke("ResetDodge");
		Invoke("ResetDodge", dodgeLevelTimeout + dodgeTime);
        //SoundSource.clip = dodge;
        //SoundSource.volume = 0.2f;
        //SoundSource.Play();

		// Play dodge animation
		StartCoroutine(DodgeRoll());
	}

	// Reset dodge level to get full iframe advantage
	void ResetDodge()
	{
		dodgeLevel = 0;
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		// Stop music & play death sound
        MusicSource.Stop();
        SoundSource.clip = deathSound;
        SoundSource.Play();

		// Show death message
		if (deathUI != null)
			deathUI.SetActive(true);

		// Respawn player
		if(!IsInvoking("Respawn"))
			Invoke("Respawn", 3f);
	}

	// Respawn player
	void Respawn()
	{
		// character controller is garbage and has to be toggled for this to work sometimes for some reason
		CharacterController cont = GetComponent<CharacterController>();
		cont.enabled = false;
		transform.position = spawnPosition;
		cont.enabled = true;

		// Add spawn invincibility & reset health
		AddInvincibility(100);
		hp = maxHP;
		
		// Hide death message
		if (deathUI != null)
			deathUI.SetActive(false);

		// Reset boss state
        GameObject.Find("Boss").GetComponent<BossScript>().Reset();
    }

	// Shoot a fireball
	GameObject Fire()
	{
		if (Dodging || !CanFire || Dead) return null;

		// Cast a ray from the camera
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
			// Where the ray hit a GameObject
            Vector3 hitPos = hit.point;

			// If the player clicked on a damagable object, then we want to target it directly.
			HealthScript hs = hit.collider.gameObject.GetComponent<HealthScript>();
			if (hs != null && hs != this) hitPos = hit.collider.gameObject.transform.position;

			// Find vector from player to desired location
			Vector3 direction = hitPos - transform.position;
			direction.y = 0;
			direction.Normalize();

			// Figure out where to spawn the fireball
			Vector3 spawnPos = transform.position + direction * projectileSpawnRadius + projectileSpawnOffset;

			// Create the fireball!
			GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction, transform.up));

			// Apply force to move the projectile forward
			Rigidbody projBody = proj.GetComponent<Rigidbody>();
			if (projBody != null)
			{
				//projBody.velocity = GetComponent<CharacterController>().velocity;
				projBody.AddForce(direction * projectileForce);
			}

			// Set up the projectile's parent so it doesn't hurt us
			ProjectileScript ps = proj.GetComponent<ProjectileScript>();
			if (ps != null)
				ps.parent = gameObject;

			lastFire = Time.fixedTime;

			// Start the staff animation
			if (staff != null)
			{
				StopCoroutine("WaveStaff");
				StartCoroutine(WaveStaff());
			}
			
			// Play attack sound
            SoundSource.clip = fireballSound;
            SoundSource.Play();
			return proj;
		}

		return null;
	}

	// Staff animation
	IEnumerator WaveStaff()
	{
		// Initial rotation is 0 0 0
		Vector3 rot = Vector3.zero;

		// Move staff down
		do
		{
			rot.x = Mathf.LerpAngle(rot.x, 90f, 0.4f);
			staff.transform.localEulerAngles = rot;

			yield return null;
		} while (rot.x < 89.9f);

		// Wait a bit
		yield return new WaitForSeconds(0.2f);

		rot = staff.transform.localEulerAngles;

		// Move staff back up
		do
		{
			rot.x = Mathf.LerpAngle(rot.x, 0f, 0.4f);
			staff.transform.localEulerAngles = rot;

			yield return null;
		} while (rot.x > 0.01f);

		// Make sure our rotation is correct
		staff.transform.localEulerAngles = Vector3.zero;
	}


	// Dodge animation
	IEnumerator DodgeRoll()
	{
		Vector3 rot = playerObj.transform.localEulerAngles;

		// Rotate 360 degrees around the X axis over the dodge period
		float stepSize = 360f / dodgeTime;

		do
		{
			rot.x += stepSize * Time.deltaTime;
			playerObj.transform.localEulerAngles = rot;

			yield return null;
		} while (Dodging && rot.x < 359.9f);

		rot.x = 0f;
		playerObj.transform.localEulerAngles = rot;
	}

    protected override void OnDamage(int dmg)
    {
		// Play damage sound
        SoundSource.clip = hurtSound;
        SoundSource.Play();
    }

	// Callback for winning the game
	public void WinGame()
	{
		Invoke("RestartGame", 5f);
	}

	// Reload the scene
	void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
