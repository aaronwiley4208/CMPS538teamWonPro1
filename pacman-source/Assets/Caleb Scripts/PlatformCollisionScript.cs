using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCollisionScript : MonoBehaviour {

    //storage var to keep track of pacman if/when he collides
    public GameObject target = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision col)
    {
        //keeps track of any given collision
        GameObject temp = col.gameObject;

        //while there is a parent object
        while (temp.transform.parent != null)
        {
            //move up a parent
            temp = temp.transform.parent.gameObject;  //fuck me is this some conversion bullshit
        }

        if (temp.name.CompareTo("Old pac-man") == 0 /*this should be a condition to make sure it's pacman*/)
        {
            //this has to go on the moving platform itself, which is parented to the holding object
            //as such, we need to set pacman's parent as the parent of this object
            //temp.transform.SetParent(this.gameObject.transform.parent);
            temp.transform.parent = this.gameObject.transform.parent;
            target = temp;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        //preemptive check; if target is null, no point in continuing.  also probably avoids a
        //null error or something
        if (target != null)
        {
            if (this.gameObject.transform.parent.childCount > 1)  //edge case check to prevent total failure
                //if pacman's parent is changed somehow, this prevents an array out of bounds error
            {
                if (this.gameObject.transform.parent.GetChild(1).name.CompareTo("Old pac-man") == 0)  //AHAHA THANK YOU UNITY
                {
                    this.gameObject.transform.parent.GetChild(1).parent = null;
                    target = null;  //look, this is a little spaghetti, but target not being null denotes pacman being collided
                    //if pacman leaves collision and was the target, that means we can safely assume it's definitely him leaving
                }
            }
            else
            {
                target = null;
                //if an array out of bounds error would have occurred, clear target because something's clearly fucky
            }
            
            /*GameObject temp = col.gameObject;

            //while there is a parent object
            while (temp.transform.parent != null)
            {
                //move up a line parent
                temp = temp.transform.parent.gameObject;  //fuck me is this some conversion bullshit

                //this needs to be here because the top-level parent of pacaman's parts is no longer the
                //top-level pacman part, it's the moving platform holding object
                if (temp == target)
                {
                    //temp.transform.SetParent(null);
                    temp.transform.parent = this.gameObject.transform.parent;
                    target = null;
                    break;
                }
            }
            */
        }
    }
}
