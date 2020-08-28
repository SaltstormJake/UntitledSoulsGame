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
public class BossScript : HealthScript
{
    public float aggroRange = 30f;
    public float screenShakeMagnitude = 0.5f;
    public float screenShakeDuration = 0.8f;
    public GameObject bossChunk, rock;

    private GameObject mainCamera, player, arena, hammer, healthBar, winUI;
    private Vector3 originalHammerPosition;
    private Quaternion originalHammerRotation;

    public AudioClip MusicClip;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip boomSound;
    public AudioSource MusicSource;
    public AudioSource SoundSource;
    public AudioSource DeathSource;
    bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Player/Main Camera");
        player = GameObject.Find("Player/Player Body");
        arena = GameObject.Find("Crater/Arena");
        hammer = GameObject.Find("Boss/Maul");
		healthBar = GameObject.Find("Canvas/BossHealthBarContainer");
		winUI = GameObject.Find("Canvas/WinPanel");

        originalHammerPosition = hammer.transform.localPosition;
        originalHammerRotation = hammer.transform.localRotation;

		winUI.SetActive(false);

        // Wait for the player before taking action
        StartCoroutine(AwaitPlayer());
    }

    // Select a new random routine for the boss to begin executing
    void StartNewRandomRoutine()
    {
        float randomSelection;

        randomSelection = Random.value * 3f;

        if (randomSelection < 1f)
        {
            StartCoroutine(HammerTime());
        }
        else if (randomSelection < 2f)
        {
            StartCoroutine(SkyFall());
        }
        else
        {
            StartCoroutine(SpinningRush());
        }
    }

    public void Reset()
    {
        CancelInvoke();
        StopAllCoroutines();
        hp = maxHP;
        StartCoroutine(ResetHammer(0f));
        transform.position = new Vector3(0f, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        StartCoroutine(AwaitPlayer());
    }
    
    protected override void OnDeath()
    {
        dead = true;
        CancelInvoke();
        StopAllCoroutines();
        DeathSource.clip = deathSound;
        DeathSource.Play();
        StartCoroutine(Die());
    }

    // ************************************************************************
    // Attack routines
    // ************************************************************************

    // A routine where the boss approaches the player then slams the ground
    // with his hammer
    IEnumerator HammerTime()
    {
        for(int i = 0; i < 3; i++)
        {
            StartCoroutine(RaiseHammer(1f));
            yield return StartCoroutine(TurnToObject(player, 1f));
            yield return StartCoroutine(StalkPlayer(40f, 10f));
            yield return StartCoroutine(HammerAttack(0.2f));
            StartCoroutine(ShakeScreen());
        }
        yield return StartCoroutine(ResetHammer(0.5f));
        Invoke("StartNewRandomRoutine", 3f);
    }

    // A routine where the boss jumps to the middle of the arena, then shakes
    // the arena, causing rocks to fall on the player.
    IEnumerator SkyFall()
    {
        // Jump to center, then jump a few times to dislodge rocks from the
        // ceiling
        yield return StartCoroutine(JumpToCenter());
        yield return StartCoroutine(JumpAngrily());

        // Boss spins to make the center of the arena unsafe
        StartCoroutine(Spin(10f));
        yield return StartCoroutine(RocksFall(10f));

        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        
        Invoke("StartNewRandomRoutine", 3f);
    }

    // A routine where the boss faces the player, then charges while spinning.
    // The charge ends when the boss runs into a wall
    IEnumerator SpinningRush()
    {
        Vector3 forward;
        
        // Turn to face player
        yield return StartCoroutine(TurnToObject(player, 1.5f));
        yield return new WaitForSeconds(0.5f);

        // Get the forward direction
        forward = transform.forward;

        // Charge until wall encountered
        while (Vector3.Distance(transform.position, arena.transform.position) < 45f)
        {
            yield return null;
            SpinCharge(forward);
        }

        // Leave some space between the boss and the wall in case this routine
        // is called again.
        SpinCharge(-forward);

        StartCoroutine(ShakeScreen());
        Invoke("StartNewRandomRoutine", 3f);
    }

    // ************************************************************************
    // Utility routines
    // ************************************************************************

    // Called at start; boss will not enter any other coroutines until the
    // player enters aggro range.
    IEnumerator AwaitPlayer()
    {
        float playerDistance;

		if (healthBar != null) healthBar.SetActive(false);

		do
        {
            yield return null;
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
            hp = maxHP;
        } while (playerDistance > aggroRange);

        MusicSource.clip = MusicClip;
        MusicSource.Play();
        MusicSource.loop = true;
        if (healthBar != null) healthBar.SetActive(true);
        // Once the boss breaks out of the wait loop, it will enter a new
        // random routine.
        StartNewRandomRoutine();
    }

    // A scenic death scene
    IEnumerator Die()
    {
        // Don't want to accidentally damage the player
        Destroy(hammer.GetComponent<ProjectileScript>());
        StartCoroutine(RaiseHammer(1f));

        // Don't want to get hit again
        Destroy(GetComponent<SphereCollider>());

        // Just in case we're not on the ground
        while(transform.position.y > 0)
        {
            transform.Translate(0f, -10f * Time.deltaTime, 0f);
            yield return null;
        }
        transform.Translate(0f, -transform.position.y, 0f);

        // And now we die
        StartCoroutine(Spin(6f));
        yield return StartCoroutine(JumpForSeconds(1f));
        StartCoroutine(ShakeScreen());
        yield return StartCoroutine(JumpForSeconds(2f));
        StartCoroutine(ShakeScreen());
        yield return StartCoroutine(JumpForSeconds(3f));
        Explode();

		healthBar.SetActive(false);
		winUI.SetActive(true);
		GameObject.Find("Player").GetComponent<PlayerScript>().WinGame();
    }

    // Boss moves to (x, z) location in moveTime seconds
    IEnumerator GoToLocation(Vector3 location, float moveTime)
    {
        Vector3 newPosition;

        // Loop to smoothly move to the location
        for (float timeRemaining = moveTime; timeRemaining > Time.deltaTime; timeRemaining -= Time.deltaTime)
        {
            newPosition = Vector3.Lerp(transform.position, location, Time.deltaTime / timeRemaining);
            newPosition.y = transform.position.y;
            transform.position = newPosition;
            yield return null;
        }
        transform.position = location;
        yield return null;
    }

    // Boss smashes the puny human
    IEnumerator HammerAttack(float time)
    {
        Vector3 pos;
        Quaternion rot;

        pos = new Vector3(5f, 7f, 6f);
        rot = Quaternion.Euler(30f, -60f, 90f);

        yield return StartCoroutine(MoveHammer(pos, rot, time));
    }

    // Boss jumps a few times in succession
    IEnumerator JumpAngrily()
    {
        yield return StartCoroutine(JumpForSeconds(1f));
        StartCoroutine(ShakeScreen(0.5f, screenShakeMagnitude));
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(JumpForSeconds(0.75f));
            StartCoroutine(ShakeScreen(0.75f, screenShakeMagnitude));
        }
    }

    // Boss jumps in the air, remaining airborne for (jumpTime) seconds
    // Note: opted not to use rigidbody due to general issues where assigning
    // constraints did not appear to entirely prevent extraneous movement.
    IEnumerator JumpForSeconds(float jumpTime)
    {
        float velocity, newVelocity, delta;

        // Begin jump execution
        velocity = -Physics.gravity.y * jumpTime / 2;
        for (float t = jumpTime; t > Time.deltaTime; t -= Time.deltaTime)
        {
            // Calculate the new velocity after deltaTime has passed
            newVelocity = velocity + Physics.gravity.y * Time.deltaTime;

            // Use conservation of energy equation, i.e. 1/2 m v^2 + m * -g * h = constant
            // Equation was selected to avoid having to interpolate velocity values
            delta = ((newVelocity * newVelocity) - (velocity * velocity)) / (2 * Physics.gravity.y);
            transform.Translate(0f, delta, 0f);
            velocity = newVelocity;
            yield return null;
        }
        // Resets position to ground position
        transform.Translate(0f, -transform.position.y, 0f);
        yield return null;
    }

    // Jumps to center of arena and pauses
    IEnumerator JumpToCenter()
    {
        StartCoroutine(GoToLocation(arena.transform.position, 2f));
        StartCoroutine(TurnToDirection(180f, 2f));
        yield return StartCoroutine(JumpForSeconds(2f));
        StartCoroutine(ShakeScreen());
        yield return new WaitForSeconds(1f);
    }

    // Moves hammer into a generic local position and rotation
    IEnumerator MoveHammer(Vector3 position, Quaternion rotation, float time)
    {
        for (float timeRemaining = time; timeRemaining > Time.deltaTime; timeRemaining -= Time.deltaTime)
        {
            hammer.transform.localPosition = Vector3.Lerp(hammer.transform.localPosition, position, Time.deltaTime / timeRemaining);
            hammer.transform.localRotation = Quaternion.Slerp(hammer.transform.localRotation, rotation, Time.deltaTime / timeRemaining);
            yield return null;
        }
        hammer.transform.localPosition = position;
        hammer.transform.localRotation = rotation;
        yield return null;
    }
    
    // Raises a hammer to a smashing position
    IEnumerator RaiseHammer(float time)
    {
        Vector3 pos;
        Quaternion rot;

        pos = new Vector3(5f, 7f, 2f);
        rot = Quaternion.Euler(-60f, 0f, 90f);

        yield return StartCoroutine(MoveHammer(pos, rot, time));
    }

    // Resets hammer to its original position and rotation
    IEnumerator ResetHammer(float time)
    {
        yield return StartCoroutine(MoveHammer(originalHammerPosition, originalHammerRotation, time));
    }

    // Rocks fall from the ceiling
    IEnumerator RocksFall(float duration)
    {
        for (float t = duration; t > 0; t -= 1f)
        {
            SpawnRock();
            yield return new WaitForSeconds(1f);
        }
    }

    // Boss spins. Useful for a few things
    IEnumerator Spin(float duration)
    {
        for (float t = duration; t > 0; t -= Time.deltaTime)
        {
            transform.Rotate(0f, -4f * 360f * Time.deltaTime, 0f);
            yield return null;
        }
    }

    // Basic call to shake screen, uses members for duration and magnitude
    IEnumerator ShakeScreen()
    {
        yield return StartCoroutine(ShakeScreen(screenShakeDuration, screenShakeMagnitude));
    }

    // Shakes camera within radius around its original local position.
    IEnumerator ShakeScreen(float shakeDuration, float shakeMagnitude)
    {
        Vector3 cameraPos;

        cameraPos = mainCamera.transform.localPosition;
        if (dead != true)
        {
            SoundSource.clip = boomSound;
            SoundSource.Play();
        }
        for (float t = shakeDuration; t > 0; t -= Time.deltaTime)
        {
            mainCamera.transform.localPosition = cameraPos + Random.insideUnitSphere * shakeMagnitude;
            yield return null;
        }

        // Resets camera to original position.
        mainCamera.transform.localPosition = cameraPos;
    }

    // Boss will move to player at stalkSpeed units/second, until either close
    // enough or maxTime is reached
    IEnumerator StalkPlayer(float stalkSpeed, float maxTime)
    {
        float timeRemaining, finalDistance;
        bool reachedPlayer;

        finalDistance = 10f;
        timeRemaining = maxTime;
        reachedPlayer = false;

        while (timeRemaining > 0f && !reachedPlayer)
        {
            // Turn towards player and move forwards at stalking speed
            transform.rotation = FindRotationToObject(player);
            transform.Translate(transform.forward * Time.deltaTime * stalkSpeed, Space.World);

            // Make sure that we're not too close
            if (Vector3.Distance(transform.position, player.transform.position) < finalDistance)
            {
                reachedPlayer = true;
                transform.position = (player.transform.position - transform.forward * finalDistance);
            }
            yield return null;
            timeRemaining -= Time.deltaTime;
        }
    }

    // Boss turns to face a direction in turnTime seconds
    IEnumerator TurnToDirection(float angle, float turnTime)
    {
        Quaternion finalRotation;

        finalRotation = Quaternion.Euler(0f, angle, 0f);

        // Loop to smoothly turn towards the direction
        for (float timeRemaining = turnTime; timeRemaining > Time.deltaTime; timeRemaining -= Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime / timeRemaining);
            yield return null;
        }
        transform.rotation = finalRotation;
    }

    // Boss turns to face an object in turnTime seconds
    IEnumerator TurnToObject(GameObject gameObject, float turnTime)
    {
        // Loop to smoothly turn towards the object, tracking its current position.
        for (float timeRemaining = turnTime; timeRemaining > Time.deltaTime; timeRemaining -= Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, FindRotationToObject(gameObject), Time.deltaTime / timeRemaining);
            yield return null;
        }
        transform.rotation = FindRotationToObject(gameObject);
    }

    // ************************************************************************
    // Various utility functions
    // ************************************************************************

    // Explodes
    void Explode()
    {
        Transform originalTransform;
        GameObject tempChunk;
        Vector3 position, velocity;

        originalTransform = GameObject.Find("Boss/BossBody").transform;

        // Some hammer housekeeping
        hammer.AddComponent<Rigidbody>();

        // Destroy the body
        Destroy(GameObject.Find("Boss/BossBody"));

        // Spawn several chunks that will last a few seconds
        for (int i = 0; i < 320; i++)
        {
            // Create a randomly placed small rock within the original sphere of the rock.
            position = originalTransform.position + Vector3.Scale(Random.insideUnitSphere, originalTransform.localScale) / 2;
            tempChunk = Instantiate(bossChunk, position, Quaternion.identity);

            // Give the chunk a velocity as a function of its placement
            velocity = 2f * (tempChunk.transform.position - originalTransform.position);
            velocity.y = 0f;
            tempChunk.GetComponent<Rigidbody>().velocity = velocity;
            
            Destroy(tempChunk, 4f);
        }
        Destroy(gameObject, 4f);
    }

    // Finds the rotation necessary to face the player
    Quaternion FindRotationToObject(GameObject gameObject)
    {
        Vector3 deltaPos;

        deltaPos = gameObject.transform.position - transform.position;
        deltaPos.y = 0f;
        return Quaternion.LookRotation(deltaPos);
    }

    // Materializes a rock in the air above the player
    void SpawnRock()
    {
        Vector3 rockPos;

        rockPos = player.transform.position;
        rockPos.y = 20f;

        GameObject rockObj = Instantiate(rock, rockPos, Quaternion.identity);
        ProjectileScript proj = rockObj.GetComponent<ProjectileScript>();
        if (proj != null) proj.parent = gameObject;
    }

    // Utility to spin and advance for a frame
    void SpinCharge(Vector3 direction)
    {
        transform.Translate(40 * direction * Time.deltaTime, Space.World);
        transform.Rotate(0, -4 * 360 * Time.deltaTime, 0);
    }

    protected override void OnDamage(int dmg)
    {
        SoundSource.clip = hurtSound;
        SoundSource.Play();
    }
}