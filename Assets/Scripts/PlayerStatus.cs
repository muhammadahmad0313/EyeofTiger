using UnityEngine;
using System.Collections;

/*
This script controls the player status.
*/

public class PlayerStatus : MonoBehaviour
{
    // The enemy target for this player
    public GameObject enemy;

    public float life = 100; // Max life
    public float stamina = 100; // Max stamina
    public int staminaRecuperationFactor = 10; // If you increment this, it will gain stamina faster
    
    // Audio clips for different events
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip lowStaminaSound;

    // Some private variables
    private bool isDead = false;
    private bool canRegenerateStamina = true;
    private bool isCovered = false;

    // Variables to access to others scripts
    private Component playerAnimationScript;
    private Component playerCombatScript;
    private Component playerMovementScript;
    private Component levelManagerScript;

    void Start()
    {
        // Set external scripts using string-based lookup to work with both JS and C#
        playerAnimationScript = GetComponent("playerAnimation");
        if (playerAnimationScript == null) playerAnimationScript = GetComponent("PlayerAnimation");
        
        playerCombatScript = GetComponent("playerCombat");
        if (playerCombatScript == null) playerCombatScript = GetComponent("PlayerCombat");
        
        playerMovementScript = GetComponent("playerMovement");
        if (playerMovementScript == null) playerMovementScript = GetComponent("PlayerMovement");
        
        levelManagerScript = GameObject.FindGameObjectWithTag("LevelManager").GetComponent("LevelManager");

        if (!enemy)
        {
            Debug.LogWarning("WARNING: You must set one enemy (the PC character) for this script in the Inspector View!");
        }
    }

    void Update()
    {
        // If player is not dead, regenerate stamina
        if (!isDead)
        {
            RegenerateStamina();
        }
    }

    // This function applies damage to this player
    public void Damage(float amount)
    {
        float totalAmount;

        if (isCovered)
        {
            totalAmount = amount / 5;
        }
        else
        {
            totalAmount = amount;
            // If the player was damaged, add one successful hit to the enemy in LevelManager
           // levelManagerScript.SendMessage("AddSuccessfulHit", "enemy");
        }

        life -= totalAmount;
        
        // Play damage sound
        PlaySound(damageSound);

        if (life <= 0)
        {
            life = 0;
            isDead = true;
            PlaySound(deathSound);
            gameObject.SendMessage("Dead");
        }
    }

    // This function reduces the stamina
    // Is called externally from PlayerCombat
    public void LoseStamina(float cantidad)
    {
        stamina -= cantidad;
        if (stamina < 0)
        {
            stamina = 0;
            canRegenerateStamina = false;
            PlaySound(lowStaminaSound);
            StartCoroutine(RegainStaminaAfterDelay());
        }
    }

    private IEnumerator RegainStaminaAfterDelay()
    {
        yield return new WaitForSeconds(3);
        canRegenerateStamina = true;
    }

    // This function regenerates stamina every frame
    // Is called in Update function
    private void RegenerateStamina()
    {
        if (canRegenerateStamina)
        {
            stamina += Time.deltaTime * staminaRecuperationFactor;
            stamina = Mathf.Clamp(stamina, 0, 100);
        }
    }

    public void Covered()
    {
        isCovered = true;
    }

    public void Uncovered()
    {
        isCovered = false;
    }

    // Player Dead
    // Informs LevelManager that the player was Knockout
    public void Dead()
    {
        isDead = true;
        levelManagerScript.SendMessage("KO", "player");
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
}
