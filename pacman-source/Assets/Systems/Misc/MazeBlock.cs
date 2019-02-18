using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A utility class to go on maze blocks that should make constructing a maze a little easier.
/// Works in hand with an editor script to execute this stuff in editor.
/// </summary>
public class MazeBlock : MonoBehaviour {

    public enum ExtendDirection { LEFT, RIGHT, UP, DOWN}

    [Header("Construction Fields")]
    [Tooltip("Are we extending left/right or up/down")]
    public ExtendDirection extendDirection;
    [Tooltip("How many blocks to put out in that direction")]
    public float blockExtension;

    /// <summary>
    /// Clone this object, putting x of them in y direction
    /// </summary>
    public void Extend() {
        // See what direction we're putting them in
        Vector3 cloneDirection = Vector3.zero;
        switch(extendDirection) {
            case ExtendDirection.LEFT:
                cloneDirection = Vector3.left;
                break;
            case ExtendDirection.RIGHT:
                cloneDirection = Vector3.right;
                break;
            case ExtendDirection.UP:
                cloneDirection = Vector3.forward;
                break;
            case ExtendDirection.DOWN:
                cloneDirection = Vector3.back;
                break;
        }

        int numWholeBlocks = (int)blockExtension;
        float remainderBlocks = blockExtension % numWholeBlocks;
        
        // Take into account how big the thing is
        float boundScale;
        float scale;
        Vector3 newLocalScale;
        Vector3 currentLocalScale = transform.localScale;
        if (extendDirection == ExtendDirection.LEFT || extendDirection == ExtendDirection.RIGHT) {
            boundScale = GetComponent<Renderer>().bounds.size.x;
            scale = transform.localScale.x;
            newLocalScale = new Vector3(scale * remainderBlocks, currentLocalScale.y, currentLocalScale.z);
        } else {
            boundScale = GetComponent<Renderer>().bounds.size.z;
            scale = transform.localScale.z;
            newLocalScale = new Vector3(currentLocalScale.x, currentLocalScale.y, scale * remainderBlocks);
        }



        // Spawn x blocks in the chosen direction.
        int i;
        for (i = 1; i <= numWholeBlocks; i++) {
            GameObject block = Instantiate(gameObject, transform.position + cloneDirection * i * boundScale, Quaternion.identity, transform.parent);
            block.name = gameObject.name;
        }
        if (remainderBlocks > 0.05f) {
            // Spawn the remaining block with a reduced scale
            GameObject partialBlock = Instantiate(gameObject, transform.position + cloneDirection * i * boundScale - cloneDirection * (((1 - remainderBlocks) * scale / 2)), Quaternion.identity, transform.parent);
            partialBlock.transform.localScale = newLocalScale;
            partialBlock.name = gameObject.name;
        }
    }
	
}
