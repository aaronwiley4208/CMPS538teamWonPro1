using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PacManLife : MonoBehaviour {

    [Tooltip("UI Image showing how much life is left. Should be filled")]
    public Image lifeCircle;
    public int maxLife = 4;
    public int currentLife;
    [Header("Invincibility Fields")]
    public float invincibleTime = 0.5f;
    public int numBlinks;
    public Renderer pacRenderer;

    private bool isInvincible = false;

	public Animator anim;
	public PlayerController controller;

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
				Debug.Log ("Die");
				anim.SetTrigger ("Die");
				controller.enabled = false;
				float timer = Time.time + 2;
				if(timer <= Time.time){
					print ("load death scene");
				}
				//loads scene to respawn
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
}
