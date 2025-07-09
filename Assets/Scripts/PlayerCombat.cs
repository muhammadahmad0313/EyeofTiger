using UnityEngine;
using System.Collections;

/*
This script controls the player combat.
*/

public class PlayerCombat : MonoBehaviour
{
    // Damage for each attack type
    public float damageJab;
    public float damageCross;
    public float damageUpperLeft;
    public float damageUpperRight;

    public float attackRatio; // If you increment this, the player can hit more faster.

    // Audio Vars
    public AudioClip attackMissed;
    public AudioClip attackLeft;
    public AudioClip attackRight;

    // Some private vars.
    private float timeForNextAttack;
    private bool canAct = true;
    private bool isDead = false;
    private float damageCaused;
    private GameObject enemy;
    private PlayerStatus playerStatusScript;
    private MonoBehaviour levelManagerScript;

    void Start()
    {
        playerStatusScript = GetComponent<PlayerStatus>();
        // Use string-based lookup to work with JS or C# scripts
        levelManagerScript = GameObject.FindGameObjectWithTag("LevelManager").GetComponent("LevelManager") as MonoBehaviour;
        enemy = playerStatusScript.enemy;
    }

    void Update()
    {
        if (!isDead)
        {
            if (timeForNextAttack > 0)
            {
                timeForNextAttack -= Time.deltaTime;
                canAct = false;
            }
            if (timeForNextAttack < 0)
            {
                timeForNextAttack = 0;
                canAct = true;
            }
        
            if (Input.GetKeyDown(KeyCode.R) && canAct)
            {
                gameObject.SendMessage("Attack", "jab");
            }
            if (Input.GetKeyDown(KeyCode.T) && canAct)
            {
                gameObject.SendMessage("Attack", "cross");
            }
            if (Input.GetKeyDown(KeyCode.F) && canAct)
            {
                gameObject.SendMessage("Attack", "uppercutleft");
            }
            if (Input.GetKeyDown(KeyCode.G) && canAct)
            {
                gameObject.SendMessage("Attack", "uppercutright");
            }
            
            if (Input.GetKey(KeyCode.E))
            {
                gameObject.SendMessage("Covered");
            }
            else
            {
                gameObject.SendMessage("Uncovered");
            }
        }
    }

    public void Attack(string attackType)
    {
        timeForNextAttack = attackRatio;
        playerStatusScript.LoseStamina(5);
        
        float dist = 0;
        if (enemy)
        {
            dist = Vector3.Distance(transform.position, enemy.transform.position);
        }
        
        if (attackType == "jab")
        {
            if (dist < 1.95f)
            {
                damageCaused = playerStatusScript.stamina * damageJab / 100;
                enemy.SendMessage("LoseLife", damageCaused);
                enemy.SendMessage("Impact", attackType);
                playerStatusScript.LoseStamina(2);
                PlaySound(attackLeft);
            }
            else
            {
                PlaySound(attackMissed);
            }
        }
        if (attackType == "cross")
        {
            if (dist < 1.95f)
            {
                damageCaused = playerStatusScript.stamina * damageCross / 100;
                enemy.SendMessage("LoseLife", damageCaused);
                enemy.SendMessage("Impact", attackType);
                playerStatusScript.LoseStamina(2);
                PlaySound(attackRight);
            }
            else
            {
                PlaySound(attackMissed);
            }
        }
        if (attackType == "uppercutleft")
        {
            if (dist < 1.85f)
            {
                damageCaused = playerStatusScript.stamina * damageUpperLeft / 100;
                enemy.SendMessage("LoseLife", damageCaused);
                enemy.SendMessage("Impact", attackType);
                playerStatusScript.LoseStamina(5);
                PlaySound(attackLeft);
            }
            else
            {
                PlaySound(attackMissed);
            }
        }
        if (attackType == "uppercutright")
        {
            if (dist < 1.85f)
            {
                damageCaused = playerStatusScript.stamina * damageUpperRight / 100;
                enemy.SendMessage("LoseLife", damageCaused);
                enemy.SendMessage("Impact", attackType);
                playerStatusScript.LoseStamina(5);
                PlaySound(attackRight);
            }
            else
            {
                PlaySound(attackMissed);
            }
        }
        
        GameObject.FindGameObjectWithTag("LevelManager").SendMessage("AddHit", "player", SendMessageOptions.DontRequireReceiver);
    }

    public void Dead()
    {
        isDead = true;
    }

    // If the player was impacted, need to wait the attackRatio seconds for the next attack.
    public void Impact()
    {
        timeForNextAttack = attackRatio;
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
