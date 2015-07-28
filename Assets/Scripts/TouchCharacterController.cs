using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	private Queue<Vector3> wayPoints = new Queue<Vector3>();
	private Vector3 [] pathNodes;
	private Vector3 lastPosition;
	private LineRenderer pathLine;

	RaycastHit terrainHit;
	
	int isWalkingHash = Animator.StringToHash ("isWalking");
	
	public GameObject terrain;

	void Start () {

		destinationPosition = transform.position;
		anim = GetComponent<Animator> ();

		pathLine = GetComponent<LineRenderer> ();

		InputHandler.InputDown += OnInputDown;
		InputHandler.InputUp += OnInputUp;

		touchCollider = GetComponent<CapsuleCollider>();
		lastPosition = transform.position;

	}

	void OnInputDown (int id)
	{
		Vector3 position;
		
		if (InputHandler.GetInputPositionByID(id, out position))
		{

			// touched this character
			ray = Camera.main.ScreenPointToRay(position);

			/*
			if (terrain.collider.Raycast (ray, out terrainHit, Mathf.Infinity)) {
				destinationPosition = terrainHit.point;
			}
			*/

			if (Physics.Raycast(ray, out hitInfo))
			//if (Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(position), 0.3f) == _touchCollider)
			{
				if (hitInfo.transform.GetComponent<CapsuleCollider>() == touchCollider){
					dragging = true;
					draggingID = id;

					wayPoints.Clear();
				}
			}
		}
	}

	void OnInputUp (int id)
	{
		if (dragging && draggingID == id)
			dragging = false;
	}

	Vector3 InputPositionToWorld (Vector3 inputPosition){

		Vector3 worldPosition = Vector3.zero;

		Ray ray = Camera.main.ScreenPointToRay(inputPosition);
		
		if (terrain.collider.Raycast (ray, out terrainHit, Mathf.Infinity)) {
			worldPosition = terrainHit.point;
		}
	
		return worldPosition;

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

				position = InputPositionToWorld(position);

				if (Mathf.Abs((position - lastPosition).magnitude) >= 0.2f){

					wayPoints.Enqueue(position);
					lastPosition = position;

				}

				//destinationPosition = position;
			}
		}
		
		destinationDistance = Vector3.Distance (destinationPosition, transform.position);
		
		if (destinationDistance > 0.2f) { 
			anim.SetBool (isWalkingHash, true);
			transform.rotation = Quaternion.LookRotation (destinationPosition - transform.position);
		} else {
			anim.SetBool (isWalkingHash, false);
		}

		if (wayPoints.Count > 0) {

			//Debug.Log((wayPoints.Peek () - transform.position).magnitude);
			//This threshold needs to be looked at for now < 0.5 should be okay
			if (Mathf.Abs ((wayPoints.Peek () - transform.position).magnitude) < 0.8f) {

				//character has reached the position
				destinationPosition = wayPoints.Dequeue ();

				pathNodes = wayPoints.ToArray();
				
				pathLine.SetVertexCount(pathNodes.Length + 1);

				pathLine.SetPosition(0, this.transform.position);

				for (int i = 0; i < pathNodes.Length; i++){
					pathLine.SetPosition(i + 1, pathNodes[i]);
				}

			}
		}

		transform.position = Vector3.MoveTowards (transform.position, destinationPosition, Time.deltaTime * moveSpeed);
		destinationHeading = destinationPosition - transform.position;

		//Debug.Log (transform.position);
		//Debug.Log (destinationPosition);
		//Debug.Log (wayPoints.Count);
	}

}
