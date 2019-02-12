using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the size of pacman, with functions for adding pellets and stuff.
/// Size goes up in small increments every so many pellets. 
/// </summary>
public class PacManSize : MonoBehaviour {

    [Tooltip("Current radius of pacman, don't adjust")]
    public float currentSize;
    [Tooltip("What size total of pellets he should consume til next sizeup")]
    public float pelletsTilSizeUp;

    [Header("UI Fields")]
    public Image sizeUpProgBar;
    public Image sizeUpProgBarBack;

    private float currentConsumedPelletSize = 0;
    public static PacManSize instance;

	// Use this for initialization
	void Awake () {
        if (instance == null) instance = this;
        else Destroy(this);

        currentSize = transform.localScale.x;
        UpdateUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Consume a pellet of given size.
    /// Add one to our pellet count and check if we need to size up.
    /// </summary>
    /// <param name="size"></param>
    public void Chomp(float size) {
        currentConsumedPelletSize += size;
        if (currentConsumedPelletSize >= pelletsTilSizeUp)
            SizeUp();
        UpdateUI();
    }

    // Increase PacMan Size
    private void SizeUp() {
        currentSize += .2f;
        transform.localScale = new Vector3(currentSize, currentSize, currentSize);
        pelletsTilSizeUp += 1;
        currentConsumedPelletSize = 0;
    }

    // Fill progress bars and say things like +size and stuff.
    private void UpdateUI() {
        sizeUpProgBar.rectTransform.sizeDelta = new Vector2(pelletsTilSizeUp * 100, 45);
        sizeUpProgBarBack.rectTransform.sizeDelta = new Vector2(pelletsTilSizeUp * 100, 45);
        float sizeUpFill = currentConsumedPelletSize / pelletsTilSizeUp;
        sizeUpProgBar.fillAmount = sizeUpFill;
    }
}
