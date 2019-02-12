using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
	
	void Awake () {

	}

	PlayerController player;
	[SerializeField] Transform camera;
	[SerializeField] float translationSpeed = 4;
	[SerializeField] float velocityPredictionFactor = 1;
	[SerializeField] float velocityPredictionAcceleration = 1;
	//[SerializeField] float zoomOut = 0.5f;
	Vector3 lastPosition;
	Vector3 playerVelocity;
	Vector3 velocityPredition;
	Vector3 cameraBaseOffset;
	public float lookSensitivity = 50;
	Vector3 rotationEuler;
	Vector3 towardsCamera;
	float startCameraDist;
	float cameraZoomIn;
	[SerializeField] float zoomSpeed = 8;
	[SerializeField] float closestDist = 2;
	void Start () {
		cameraBaseOffset = camera.localPosition;
		rotationEuler = transform.rotation.eulerAngles;
		startCameraDist = Vector3.Distance (transform.position, camera.position);
	}
	
	// Update is called once per frame
    
	void Update () {
        //if (!PauseGame.getPaused())
        //{

            if (player)
            {
                //if (!PauseGame.getPaused())
                //{
                    playerVelocity = Vector3.MoveTowards(playerVelocity, ((player.transform.position - lastPosition) / Time.deltaTime), Time.deltaTime * velocityPredictionAcceleration);
					towardsCamera = (camera.position - player.transform.position).normalized;

					if (hit.transform)
						cameraZoomIn = Mathf.MoveTowards (cameraZoomIn, startCameraDist - hit.distance, 5 * Time.deltaTime);
					else
						cameraZoomIn = Mathf.MoveTowards (cameraZoomIn, 0, 5 * Time.deltaTime);
					cameraZoomIn = Mathf.Clamp (cameraZoomIn, 0, startCameraDist - closestDist);
                    //transform.position = Vector3.Lerp (transform.position, player.transform.position + playerVelocity * velocityPredictionFactor + towardsCamera * playerVelocity.magnitude * zoomOutwithSpeedFactor, Time.deltaTime * 4);
                    velocityPredition = playerVelocity * velocityPredictionFactor;
                    velocityPredition = Vector3.MoveTowards(velocityPredition, playerVelocity * velocityPredictionFactor, Time.deltaTime * velocityPredictionAcceleration); ;
					transform.position = Vector3.Lerp(transform.position, player.transform.position + velocityPredition - towardsCamera * cameraZoomIn, Time.deltaTime * 4);
                    lastPosition = player.transform.position;
                //}

            }
            else
            {
                player = FindObjectOfType<PlayerController>();

            }

            //transform.Rotate (Input.GetAxis ("Mouse Y") * Time.deltaTime * 100, Input.GetAxis ("Mouse X") * Time.deltaTime  * 100, 0);
            //transform.RotateAround(transform.right, Input.GetAxis ("Mouse Y") * Time.deltaTime * -lookSensitivity);


            //transform.rotation = Quaternion.Euler (Mathf.Clamp(transform.rotation.x, -30, 80), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            //transform.rotation = Quaternion.Euler (Mathf.Clamp(transform.rotation.x + Input.GetAxis ("Mouse Y") * Time.deltaTime * -lookSensitivity, -30, 80), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            //transform.RotateAround(Vector3.up, Input.GetAxis ("Mouse X") * Time.deltaTime * -lookSensitivity);
            rotationEuler.y += (Input.GetAxis("Mouse X") + Input.GetAxis("Stick 2 X")) * Time.deltaTime * lookSensitivity;
            rotationEuler.x += (Input.GetAxis("Mouse Y") + Input.GetAxis("Stick 2 Y")) * Time.deltaTime * -lookSensitivity;
            rotationEuler.x = Mathf.Clamp(rotationEuler.x, -20, 20);
            transform.rotation = Quaternion.Euler(rotationEuler);
        }
	//}

	RaycastHit hit;
	[SerializeField] LayerMask mask;
	void FixedUpdate(){
		if (player)
			//Physics.Raycast (player.transform.position, camera.position - player.transform.position, out hit, startCameraDist, mask);
			Physics.SphereCast (player.transform.position + Vector3.up, 0.1f, camera.position - player.transform.position, out hit, startCameraDist, mask);
//		Debug.DrawLine (player.transform.position, hit.point);
//		Debug.Log (hit.distance);s
//
	}
}
