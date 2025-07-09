using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour
{
    private bool isCovered = false;
    private bool isDead = false;
    
    // Audio effects
    public AudioClip impactSound;
    public AudioClip deathSound;
    public AudioClip coverSound;
    public AudioClip uncoverSound;
    
    void Start()
    {
        // Set up animation layers and blend modes
        GetComponent<Animation>()["jab"].layer = 1;
        GetComponent<Animation>()["jab"].blendMode = AnimationBlendMode.Additive;

        GetComponent<Animation>()["cross"].layer = 1;
        GetComponent<Animation>()["cross"].blendMode = AnimationBlendMode.Additive;

        GetComponent<Animation>()["uppercutleft"].layer = 1;
        GetComponent<Animation>()["uppercutleft"].blendMode = AnimationBlendMode.Additive;

        GetComponent<Animation>()["uppercutright"].layer = 1;
        GetComponent<Animation>()["uppercutright"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["cubrirse"].layer = 2;
        GetComponent<Animation>()["cubrirse"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["descubrirse"].layer = 2;
        GetComponent<Animation>()["descubrirse"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["impactoDerecho"].layer = 1;
        GetComponent<Animation>()["impactoDerecho"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["impactoIzquierdo"].layer = 1;
        GetComponent<Animation>()["impactoIzquierdo"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["impactoBajoDerecho"].layer = 1;
        GetComponent<Animation>()["impactoBajoDerecho"].blendMode = AnimationBlendMode.Additive;
        
        GetComponent<Animation>()["impactoBajoIzquierdo"].layer = 1;
        GetComponent<Animation>()["impactoBajoIzquierdo"].blendMode = AnimationBlendMode.Additive;
    }

    void Update()
    {
        if (isDead) return;
        
        CharacterController controller = GetComponent<CharacterController>();
        COMController comController = GetComponent<COMController>();
        
        if (controller.velocity.magnitude > 0.1f)
        {
            // Check direction of movement relative to facing
            float forwardDot = Vector3.Dot(controller.velocity.normalized, transform.forward);
            float rightDot = Vector3.Dot(controller.velocity.normalized, transform.right);
            
            if (forwardDot > 0.7f)
            {
                GetComponent<Animation>().CrossFade("avanzar", 0.2f);
            }
            else if (forwardDot < -0.7f)
            {
                GetComponent<Animation>().CrossFade("caminar_atras", 0.2f);
            }
            else if (rightDot > 0.7f)
            {
                GetComponent<Animation>().CrossFade("caminar_derecha", 0.2f);
            }
            else if (rightDot < -0.7f)
            {
                GetComponent<Animation>().CrossFade("caminar_izquierda", 0.2f);
            }
            else
            {
                GetComponent<Animation>().CrossFade("avanzar", 0.2f);
            }
        }
        else
        {
            GetComponent<Animation>().CrossFade("idle", 0.2f);
        }
    }

    public void Attack(string attackType)
    {
        if (isDead) return;
        
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().Blend(attackType, 1, 0.1f);
    }

    public void Impact(string hitType)
    {
        if (isDead) return;
        
        // Play impact sound
        PlaySound(impactSound);
        
        if (hitType == "jab")
        {
            GetComponent<Animation>().CrossFadeQueued("impactoIzquierdo", 0.1f, QueueMode.PlayNow);
        }
        else if (hitType == "cross")
        {
            GetComponent<Animation>().CrossFadeQueued("impactoDerecho", 0.1f, QueueMode.PlayNow);
        }
        else if (hitType == "uppercutleft")
        {
            GetComponent<Animation>().CrossFadeQueued("impactoBajoIzquierdo", 0.1f, QueueMode.PlayNow);
        }
        else if (hitType == "uppercutright")
        {
            GetComponent<Animation>().CrossFadeQueued("impactoBajoDerecho", 0.1f, QueueMode.PlayNow);
        }
    }

    public void Cover()
    {
        if (isDead || isCovered) return;
        
        GetComponent<Animation>().Blend("cubrirse", 1, 0.1f);
        PlaySound(coverSound);
        isCovered = true;
    }

    public void Uncover()
    {
        if (isDead || !isCovered) return;
        
        GetComponent<Animation>().CrossFade("descubrirse", 0.2f);
        PlaySound(uncoverSound);
        isCovered = false;
    }

    public void Dead()
    {
        isDead = true;
        GetComponent<Animation>().CrossFade("muerte", 0.2f);
        PlaySound(deathSound);
        this.enabled = false;
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
