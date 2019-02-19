using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PeckEffects : MonoBehaviour {

    public Renderer pacRenderer;
    public Renderer ballRenderer;
    public PacManLife life;

    Color originalBallColor;
    float invulnStart;
    float invulnLength = 10;

	// Use this for initialization
	void Start () {
        originalBallColor = ballRenderer.material.color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnTriggerEnter(Collider col)
    {
        GameObject temp = col.gameObject;

        //while there is a parent object
        //while (temp.transform.parent != null)
        //{
            //move up a parent
            //temp = temp.transform.parent.gameObject;  //fuck me is this some conversion bullshit
        //}

        if (temp.name.Contains("Ghost"))
        {

            //EnemyPatrolBehavior script = temp.GetComponentInChildren<EnemyPatrolBehavior>();
            //script.Transition(script.currentState, EnemyPatrolBehavior.PatrolState.RETREAT);
            //trigger ghost effect on chillern
            //search components for ghostAI script
            //given current behavior, trigger flee behavior
            //increment points
        }
        else if (temp.name.Contains("Mega"))
        {
            Debug.Log("Mega " + temp.name);
            // Call the chomp script on the mega pellet
            temp.GetComponent<MegaPellet>().GetChomped();
            //trigger super pellet effect
            Destroy(temp);
            //increment points
            StartCoroutine("Invuln");
        }
        else
        {
            Debug.Log("boy howdy" + temp.name);
            //do nothing, peck has no effect
        }
    }

    IEnumerator Invuln()
    {
        pacRenderer.material.color = Color.red;
        ballRenderer.material.color = Color.red;
        life.isInvincible = true;
        invulnStart = Time.time;
        //start invuln effect
        while (Time.time < invulnStart + invulnLength)
        {
            //wait
            yield return null;
        }
        //end invuln effect
        pacRenderer.material.color = Color.white;
        ballRenderer.material.color = originalBallColor;
        life.isInvincible = false;
    }
}
