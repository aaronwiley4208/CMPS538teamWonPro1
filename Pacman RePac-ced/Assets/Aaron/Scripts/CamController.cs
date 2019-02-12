using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour {

	public Transform target;
	public float smoothness = 0.5f;
	public Vector3 targetOffset;
	public float xAxisTilt;
	public float zoomSpeed = 1f;

	Vector3 destination = Vector3.zero;
	float rotateVelocity = 0;

	public float xSpeed = 2f;
	public float ySpeed = 2f;
	float xRotate;
	float yRotate;
	float orbitD = 5f;

	// Use this for initialization
	void Start () {
		SetTarget (target);
	}

	void SetTarget(Transform tar){
		target = tar;
	}
		
	void LateUpdate () {
		//MoveToTarget ();
		//LookAtTarget ();

			OrbitLook();
		
	}

	void MoveToTarget(){
		destination =  targetOffset;
		//destination =  target.transform.rotation * targetOffset;
		destination += target.position;
		transform.position = destination;
		if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
			targetOffset.z += Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
		}
	}

	void LookAtTarget(){
		//float eulerAngleY = Mathf.SmoothDampAngle (transform.eulerAngles.y, target.eulerAngles.y, ref rotateVelocity, smoothness);//follows target directly
		//transform.rotation = Quaternion.Euler (transform.eulerAngles.x, eulerAngleY, 0);
		if(Input.GetMouseButton(1)){
			xRotate += Input.GetAxis("Mouse X") * xSpeed;
			yRotate += Input.GetAxis ("Mouse Y") * ySpeed;
			transform.eulerAngles = new Vector3 (yRotate, xRotate, 0);
		}

	}

	void OrbitLook(){
		xRotate += Input.GetAxis("Mouse X");
		yRotate += Input.GetAxis("Mouse Y");
		Vector3 direction = new Vector3 (0,0, -orbitD);
		Quaternion rotation = Quaternion.Euler (yRotate, xRotate, 0);
		//if (Input.GetMouseButton (1)) {
			transform.position = target.position + rotation * direction;
			transform.LookAt (target.position);
		//}
		if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
			orbitD += Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed;
		}
	}
}
