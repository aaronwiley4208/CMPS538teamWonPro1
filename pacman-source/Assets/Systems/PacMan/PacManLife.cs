using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PacManLife : MonoBehaviour {

    [Tooltip("UI Image showing how much life is left. Should be filled")]
    public Image lifeCircle;
    public int maxLife = 4;
    public int currentLife;
    [Header("Invincibility Fields")]
    public float invincibleTime = 0.5f;
    public int numBlinks;
    public Renderer pacRenderer;

    public bool isInvincible = false;

	public Animator anim;
	public PlayerController controller;

    public AudioSource auxOut;
    public AudioClip death;
    public CapsuleCollider capsule;
    public SphereCollider sphere;

	// Use this for initialization
	void Start () {
        currentLife = maxLife;
		if (!anim)
			anim = GetComponentInChildren<Animator>();
		controller = GetComponent<PlayerController> ();
	}
	
    // What happens when pacman takes a hit
    public void Hit() {
        if (!isInvincible) {
            currentLife--;
            // Update UI
            lifeCircle.fillAmount = (float)currentLife / maxLife;
            // Check if dead
			if (currentLife <= 0) {
                StartCoroutine("Die");
			}
            // Start invincibility
            StartCoroutine("BeInvincible");
        }
    }

	public void HPUp(int amount) {
		currentLife = Mathf.Clamp (currentLife + amount, 0, maxLife);
		lifeCircle.fillAmount = (float)currentLife / maxLife;
	}

    IEnumerator BeInvincible() {
        isInvincible = true;
        float blinkTime = invincibleTime / numBlinks;
        // Go through a number of blinks
        for (int i = 0; i < numBlinks; i++) {
            pacRenderer.enabled = !pacRenderer.enabled;
            yield return new WaitForSeconds(blinkTime);
        }
        pacRenderer.enabled = true;
        isInvincible = false;
    }

    IEnumerator Die()
    {
        Debug.Log("Die");
        anim.SetTrigger("Die");
        capsule.enabled = false;
        sphere.enabled = false;
        //auxOut.clip = death;  //TODO fit these into the existing audio crap 
        //auxOut.Play();

        controller.enabled = false;
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene("Game Over Scene");
    }
}
