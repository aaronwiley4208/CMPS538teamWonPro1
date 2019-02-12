using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullParent : MonoBehaviour {

	void Awake () {
		transform.parent = null;
	}
}
