using UnityEngine;
using System.Collections;

public class ClickCharacterController : MonoBehaviour {

	private Transform myTransform;
	private Vector3 destinationPosition;
	private Vector3 destinationHeading;
	private float destinationDistance;
	private float moveSpeed;
	private float rotateSpeed;

	protected Animator anim;

	int isWalkingHash = Animator.StringToHash ("isWalking");

	public GameObject terrain;


	void Start () {
		myTransform = transform;
		destinationPosition = myTransform.position;
		moveSpeed = 0.5f;
		rotateSpeed = 1.0f;
		anim = GetComponent<Animator> ();
	}
	
	void Update () {

		if (Input.GetMouseButtonDown(0)) {
			
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (terrain.collider.Raycast (ray, out hit, Mathf.Infinity)) {
				destinationPosition = hit.point;
			}			
		}

		destinationDistance = Vector3.Distance (destinationPosition, myTransform.position);

		if (destinationDistance > 0.2f) {
			anim.SetBool (isWalkingHash, true);
			//Vector3.RotateTowards (transform.rotation, destinationPosition - transform.position, 0.0f, 0.0f);
			transform.rotation = Quaternion.LookRotation (destinationPosition - transform.position);
		} else {
			anim.SetBool (isWalkingHash, false);
		}

		transform.position = Vector3.MoveTowards (transform.position, destinationPosition, Time.deltaTime * moveSpeed);

		destinationHeading = destinationPosition - transform.position;

	}
}
