using UnityEngine;
using System.Collections;

public class COMController : MonoBehaviour
{
    // Target player
    public GameObject player;
    
    // Combat stats
    public float vida = 100f;      // Health
    public float stamina = 100f;   // Stamina
    public int staminaRecoveryRate = 8;
    
    // Hit tracking
    public int totalGolpes = 0;    // Total hits
    public int golpesEfetivos = 0; // Effective hits
    
    // Combat parameters
    public float attackDistance = 1.9f;
    public float retreatDistance = 1.5f;
    public float attackCooldown = 1.5f;
    public float damageMultiplier = 1.0f;
    
    // Attack damage values
    public float damageJab = 5f;
    public float damageCross = 8f;
    public float damageUppercutLeft = 12f;
    public float damageUppercutRight = 15f;
    
    // Ring boundary settings (can be adjusted in inspector)
    public float ringXLimit = 7.0f;  // Half width of the ring
    public float ringZLimit = 7.0f;  // Half length of the ring
    
    // Audio
    public AudioClip hitSound;
    public AudioClip missSound;
    
    // Private variables
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool isCovered = false;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float moveSpeed = 2.0f;
    private float rotationSpeed = 5.0f;
    private float distanceToPlayer;
    
    // AI behavior states
    private enum AIState { Approach, Retreat, Attack, Defend, Circling }
    private AIState currentState = AIState.Circling;
    private float stateChangeTime = 0f;
    private float stateDuration = 2f;
    
    void Start()
    {
        if (!player)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (!player)
            {
                Debug.LogError("Enemy AI: Player reference not set and couldn't be found!");
            }
        }
        
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("Enemy AI: CharacterController component not found!");
        }
        
        // Randomize first state
        ChangeState();
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Recover stamina
        if (stamina < 100)
        {
            stamina += Time.deltaTime * staminaRecoveryRate;
            stamina = Mathf.Clamp(stamina, 0, 100);
        }
        
        if (!player) return;
        
        // Calculate distance to player
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // Look at player
        RotateTowardsPlayer();
        
        // Check if it's time to change state
        if (Time.time > stateChangeTime)
        {
            ChangeState();
        }
        
        // Execute current state behavior
        ExecuteStateBehavior();
    }
    
    void ChangeState()
    {
        // Set duration for this state
        stateDuration = Random.Range(1.5f, 4f);
        stateChangeTime = Time.time + stateDuration;
        
        // Choose a state based on distance and other factors
        float randomValue = Random.value;
        
        if (distanceToPlayer > attackDistance + 0.5f)
        {
            // Too far to attack, approach or circle
            currentState = (randomValue < 0.7f) ? AIState.Approach : AIState.Circling;
        }
        else if (distanceToPlayer < retreatDistance)
        {
            // Too close, back up
            currentState = AIState.Retreat;
        }
        else
        {
            // In combat range, decide what to do
            if (randomValue < 0.5f)
                currentState = AIState.Attack;
            else if (randomValue < 0.8f)
                currentState = AIState.Circling;
            else
                currentState = AIState.Defend;
        }
        
        // If low on stamina, increase chance of defending
        if (stamina < 30 && currentState == AIState.Attack)
        {
            if (Random.value < 0.7f)
                currentState = AIState.Defend;
        }
        
        // Debug.Log("Enemy AI: Changed state to " + currentState);
    }
    
    void ExecuteStateBehavior()
    {
        switch (currentState)
        {
            case AIState.Approach:
                ApproachPlayer();
                break;
                
            case AIState.Retreat:
                RetreatFromPlayer();
                break;
                
            case AIState.Attack:
                AttackPlayer();
                break;
                
            case AIState.Defend:
                Defend();
                break;
                
            case AIState.Circling:
                CircleAroundPlayer();
                break;
        }
    }
    
    void ApproachPlayer()
    {
        // Move toward player
        Vector3 direction = (player.transform.position - transform.position).normalized;
        moveDirection = direction * moveSpeed;
        
        // Check if the enemy would move outside the ring boundary
        Vector3 nextPosition = transform.position + moveDirection * Time.deltaTime;
        
        // Constrain the position within ring boundaries
        nextPosition = ConstrainToRingBoundaries(nextPosition);
        
        // Calculate adjusted movement vector
        Vector3 adjustedMovement = nextPosition - transform.position;
        controller.Move(adjustedMovement);
        
        // Send animation message
        SendMessage("Uncover", SendMessageOptions.DontRequireReceiver);
        isCovered = false;
        
        // Chance to attack if we get close enough
        if (distanceToPlayer <= attackDistance && Time.time > nextAttackTime)
        {
            if (Random.value < 0.3f)
            {
                PerformAttack();
            }
        }
    }
    
    void RetreatFromPlayer()
    {
        // Move away from player
        Vector3 direction = (transform.position - player.transform.position).normalized;
        moveDirection = direction * moveSpeed * 0.8f;
        
        // Check if the enemy would move outside the ring boundary
        Vector3 nextPosition = transform.position + moveDirection * Time.deltaTime;
        
        // Constrain the position within ring boundaries
        nextPosition = ConstrainToRingBoundaries(nextPosition);
        
        // Calculate adjusted movement vector
        Vector3 adjustedMovement = nextPosition - transform.position;
        controller.Move(adjustedMovement);
        
        // Sometimes cover while retreating
        if (Random.value < 0.3f * Time.deltaTime)
        {
            SendMessage("Cover", SendMessageOptions.DontRequireReceiver);
            isCovered = true;
        }
        else
        {
            SendMessage("Uncover", SendMessageOptions.DontRequireReceiver);
            isCovered = false;
        }
    }
    
    void AttackPlayer()
    {
        if (Time.time > nextAttackTime && distanceToPlayer <= attackDistance)
        {
            PerformAttack();
        }
        else if (distanceToPlayer > attackDistance)
        {
            // Get a bit closer to attack
            Vector3 direction = (player.transform.position - transform.position).normalized;
            moveDirection = direction * moveSpeed * 0.6f;
            
            // Check if the enemy would move outside the ring boundary
            Vector3 nextPosition = transform.position + moveDirection * Time.deltaTime;
            
            // Constrain the position within ring boundaries
            nextPosition = ConstrainToRingBoundaries(nextPosition);
            
            // Calculate adjusted movement vector
            Vector3 adjustedMovement = nextPosition - transform.position;
            controller.Move(adjustedMovement);
        }
    }
    
    void Defend()
    {
        // Stand ground and defend
        SendMessage("Cover", SendMessageOptions.DontRequireReceiver);
        isCovered = true;
        
        // Slight movement to avoid being stationary
        if (Random.value < 0.2f)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            Vector3 moveVector = randomDir * moveSpeed * 0.3f * Time.deltaTime;
            
            // Check if the enemy would move outside the ring boundary
            Vector3 nextPosition = transform.position + moveVector;
            
            // Constrain the position within ring boundaries
            nextPosition = ConstrainToRingBoundaries(nextPosition);
            
            // Calculate adjusted movement vector
            Vector3 adjustedMovement = nextPosition - transform.position;
            controller.Move(adjustedMovement);
        }
    }
    
    void CircleAroundPlayer()
    {
        // Move perpendicular to the direction to the player
        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(dirToPlayer.z, 0, -dirToPlayer.x);
        
        // Occasionally switch direction
        if (Random.value < 0.01f)
        {
            perpendicular = -perpendicular;
        }
        
        moveDirection = perpendicular * moveSpeed * 0.7f;
        
        // Check if the enemy would move outside the ring boundary
        Vector3 nextPosition = transform.position + moveDirection * Time.deltaTime;
        
        // Constrain the position within ring boundaries
        nextPosition = ConstrainToRingBoundaries(nextPosition);
        
        // Calculate adjusted movement vector
        Vector3 adjustedMovement = nextPosition - transform.position;
        controller.Move(adjustedMovement);
        
        // Occasionally attack while circling
        if (distanceToPlayer <= attackDistance && Time.time > nextAttackTime && Random.value < 0.02f)
        {
            PerformAttack();
        }
        
        // Sometimes cover while circling
        if (Random.value < 0.1f * Time.deltaTime)
        {
            SendMessage("Cover", SendMessageOptions.DontRequireReceiver);
            isCovered = true;
        }
        else if (Random.value < 0.2f * Time.deltaTime)
        {
            SendMessage("Uncover", SendMessageOptions.DontRequireReceiver);
            isCovered = false;
        }
    }
    
    void PerformAttack()
    {
        // Choose an attack
        string[] attacks = new string[] { "jab", "cross", "uppercutleft", "uppercutright" };
        float[] weights = new float[] { 0.4f, 0.3f, 0.15f, 0.15f };
        
        // Weighted random selection
        float totalWeight = 0;
        for (int i = 0; i < weights.Length; i++) totalWeight += weights[i];
        
        float randomValue = Random.Range(0, totalWeight);
        float weightSum = 0;
        string selectedAttack = attacks[0];
        
        for (int i = 0; i < attacks.Length; i++)
        {
            weightSum += weights[i];
            if (randomValue <= weightSum)
            {
                selectedAttack = attacks[i];
                break;
            }
        }
        
        // Perform the attack
        SendMessage("Attack", selectedAttack, SendMessageOptions.DontRequireReceiver);
        
        // Check if we hit the player
        bool hit = false;
        if (distanceToPlayer <= attackDistance)
        {
            float hitChance = 0.7f; // Base hit chance
            
            // Check if player is covered
            MonoBehaviour playerStatus = player.GetComponent("playerStatus") as MonoBehaviour;
            if (playerStatus != null)
            {
                // Try to determine if player is covered using reflection (since it might be JavaScript)
                System.Reflection.FieldInfo isCoveredField = playerStatus.GetType().GetField("isCovered", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (isCoveredField != null)
                {
                    bool playerIsCovered = (bool)isCoveredField.GetValue(playerStatus);
                    if (playerIsCovered)
                    {
                        hitChance *= 0.5f; // Harder to hit a covered player
                    }
                }
            }
            
            // Apply the hit
            if (Random.value < hitChance)
            {
                hit = true;
                float damage = 0;
                
                // Calculate damage based on attack type
                switch (selectedAttack)
                {
                    case "jab":
                        damage = damageJab * damageMultiplier;
                        break;
                    case "cross":
                        damage = damageCross * damageMultiplier;
                        break;
                    case "uppercutleft":
                        damage = damageUppercutLeft * damageMultiplier;
                        break;
                    case "uppercutright":
                        damage = damageUppercutRight * damageMultiplier;
                        break;
                }
                
                // Apply damage to player
                player.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
                
                // Tell player what hit them
                player.SendMessage("Impact", selectedAttack, SendMessageOptions.DontRequireReceiver);
                
                // Track hits
                totalGolpes++;
                golpesEfetivos++;
                
                // Inform LevelManager
                GameObject.FindGameObjectWithTag("LevelManager").SendMessage("AddSuccessfulHit", "enemy", SendMessageOptions.DontRequireReceiver);
                GameObject.FindGameObjectWithTag("LevelManager").SendMessage("AddHit", "enemy", SendMessageOptions.DontRequireReceiver);
                
                // Play hit sound
                PlaySound(hitSound);
            }
            else
            {
                // Miss
                totalGolpes++;
                GameObject.FindGameObjectWithTag("LevelManager").SendMessage("AddHit", "enemy", SendMessageOptions.DontRequireReceiver);
                
                // Play miss sound
                PlaySound(missSound);
            }
        }
        else
        {
            // Out of range miss
            totalGolpes++;
            GameObject.FindGameObjectWithTag("LevelManager").SendMessage("AddHit", "enemy", SendMessageOptions.DontRequireReceiver);
            
            // Play miss sound
            PlaySound(missSound);
        }
        
        // Set cooldown
        nextAttackTime = Time.time + attackCooldown;
        
        // Reduce stamina
        stamina -= (hit ? 10f : 5f);
        stamina = Mathf.Clamp(stamina, 0, 100);
    }
    
    void RotateTowardsPlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void LoseLife(float amount)
    {
        vida -= amount;
        
        if (vida <= 0)
        {
            vida = 0;
            isDead = true;
            Dead();
        }
        else
        {
            // React to being hit
            if (Random.value < 0.4f)
            {
                currentState = AIState.Retreat;
                stateChangeTime = Time.time + Random.Range(0.5f, 1.5f);
            }
            else if (Random.value < 0.7f)
            {
                currentState = AIState.Defend;
                stateChangeTime = Time.time + Random.Range(0.5f, 1.0f);
            }
        }
    }
    
    public void Movimenta(string direction)
    {
        // Handle movement commands from external scripts
        if (direction == "Recua")
        {
            Vector3 moveDir = (transform.position - player.transform.position).normalized;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }
    
    public void Dead()
    {
        isDead = true;
        SendMessage("Dead", SendMessageOptions.DontRequireReceiver);
        GameObject.FindGameObjectWithTag("LevelManager").SendMessage("KO", "enemy", SendMessageOptions.DontRequireReceiver);
    }
    
    // Helper method to play sounds safely
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            // If no AudioSource component is found, play the clip at the position
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    // Helper method to constrain movement within ring boundaries
    private Vector3 ConstrainToRingBoundaries(Vector3 targetPosition)
    {
        // Use ring boundary limits from public variables (can be adjusted in inspector)
        // Clamp the position within ring boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, -ringXLimit, ringXLimit);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -ringZLimit, ringZLimit);
        
        return targetPosition;
    }
}
