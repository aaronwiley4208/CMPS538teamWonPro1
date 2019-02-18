using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeckEffects : MonoBehaviour {

    float invulnStart;
    float invulnLength = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnCollisionEnter(Collision col)
    {
        GameObject temp = col.gameObject;

        //while there is a parent object
        while (temp.transform.parent != null)
        {
            //move up a parent
            temp = temp.transform.parent.gameObject;  //fuck me is this some conversion bullshit
        }

        if (temp.name.Contains("Ghost"))
        {
            //trigger ghost effect on chillern
            //search components for ghostAI script
            //given current behavior, trigger flee behavior
            //increment points
        }
        else if (temp.name.Contains("Mega"))
        {
            //trigger super pellet effect
            Destroy(temp);
            //increment points
            StartCoroutine("Invuln");
        }
        else
        {
            //do nothing, peck has no effect
        }
    }

    IEnumerator Invuln()
    {
        invulnStart = Time.time;
        //start invuln effect
        while (Time.time < invulnStart + invulnLength)
        {
            //wait
            yield return null;
        }
        //end invuln effect
    }
}
