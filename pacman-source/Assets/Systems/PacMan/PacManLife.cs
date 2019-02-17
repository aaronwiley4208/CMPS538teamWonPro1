using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PacManLife : MonoBehaviour {

    [Tooltip("UI Image showing how much life is left. Should be filled")]
    public Image lifeCircle;
    public int maxLife = 4;
    public int currentLife;
    public float invincibleTime = 0.5f;

    private bool isInvincible = false;

	// Use this for initialization
	void Start () {
        currentLife = maxLife;
	}
	
    // What happens when pacman takes a hit
    public void Hit() {
        if (!isInvincible) {
            currentLife--;
            // Update UI
            lifeCircle.fillAmount = (float)currentLife / maxLife;
            // Check if dead
            if (currentLife == 0)
                Debug.Log("Die");

            // Start invincibility
            StartCoroutine("BeInvincible");
        }
    }

    IEnumerator BeInvincible() {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
}
