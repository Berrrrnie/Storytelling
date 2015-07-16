using UnityEngine;
using System.Collections;

public class TouchCharacterController : MonoBehaviour {
	
	private Vector3 destinationPosition;
	private Vector3 destinationHeading;
	private float destinationDistance;
	private float moveSpeed = 0.5f;
	private float rotateSpeed = 1.0f;

	private RaycastHit hitInfo = new RaycastHit();
	private Ray ray;
	Collider touchCollider;

	private Vector3 touchPosition;
	private bool dragging;
	private int draggingID;
	
	protected Animator anim;

	RaycastHit terrainHit;
	
	int isWalkingHash = Animator.StringToHash ("isWalking");
	
	public GameObject terrain;

	void Start () {

		destinationPosition = transform.position;
		anim = GetComponent<Animator> ();

		InputHandler.InputDown += OnInputDown;
		InputHandler.InputDown += OnInputUp;

		touchCollider = GetComponent<CapsuleCollider>();

	}

	void OnInputDown (int id)
	{
		Vector3 position;
		
		if (InputHandler.GetInputPositionByID(id, out position))
		{
			// touched this character
			ray = Camera.main.ScreenPointToRay(position);
			if (Physics.Raycast(ray, out hitInfo))
			//if (Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(position), 0.3f) == _touchCollider)
			{
				if (hitInfo.transform.GetComponent<CapsuleCollider>() == touchCollider){
					dragging = true;
					draggingID = id;
				}
			}
		}
	}

	void OnInputUp (int id)
	{
		if (dragging && draggingID == id)
			dragging = false;
	}
	
	void Update () {

		/*
		if (Input.GetMouseButtonDown(0))
		{
			//find position
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			//set destination to position
			if (terrain.collider.Raycast (ray, out terrainHit, Mathf.Infinity)) {
				destinationPosition = terrainHit.point;
			}			
		}
		*/

		if (dragging)
		{
			Vector3 position;
			if (InputHandler.GetInputPositionByID(draggingID, out position))
			{
				//Vector3 correctedPosition = Camera.main.ScreenToWorldPoint(position);
				//correctedPosition.z = 0;
				
				//_rigidBody2D.Accelerate((correctedPosition - transform.position) * _maxSpeed, _acceleration);

				destinationPosition = position;
			}
		}
		
		destinationDistance = Vector3.Distance (destinationPosition, transform.position);
		
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
