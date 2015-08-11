using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Janus;

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

	Timeline<Vector3> charPositionTimeline;

	// server and orchestrator machine
	public bool server = true;

	void Start () {
		
		anim = GetComponent<Animator> ();

		pathLine = GetComponent<LineRenderer> ();

		InputHandler.InputDown += OnInputDown;
		InputHandler.InputUp += OnInputUp;

		touchCollider = GetComponent<CapsuleCollider>();

		destinationPosition = transform.position;
		lastPosition = transform.position;

	}

	public void Awake(){

		Debug.Log(gameObject.name);

		charPositionTimeline = TimelineManager.Default.Get<Vector3>(gameObject.name);
		charPositionTimeline.AddSendFilter(TimelineUtils.BuildDeltaRateFilter <Vector3>((x,y) => Vector3.Distance(x,y), ()=>0.05f, ()=>2.0f));

	}

	public void OnDestroy()
	{		
		TimelineManager.Default.Remove(this.charPositionTimeline);
	}

	void OnInputDown (int id)
	{
		Vector3 position;
		
		if (InputHandler.GetInputPositionByID(id, out position))
		{

			// touched this character
			ray = Camera.main.ScreenPointToRay(position);

			if (Physics.Raycast(ray, out hitInfo))
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

		if(server)
		{
			if (dragging)
			{
				Vector3 position;

				if (InputHandler.GetInputPositionByID(draggingID, out position))
				{

					position = InputPositionToWorld(position);

					if (Mathf.Abs((position - lastPosition).magnitude) >= 0.1f){

						wayPoints.Enqueue(position);
						lastPosition = position;

					}
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

				//This threshold needs to be looked at for now < 0.5 should be okay
				if (Mathf.Abs ((wayPoints.Peek () - transform.position).magnitude) < 0.5f) {

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


			if (charPositionTimeline != null){
				Debug.Log("Updating");
				charPositionTimeline[0] = transform.position;
			}

		} else {
			transform.position = charPositionTimeline[-0.05f];
		}

		destinationHeading = destinationPosition - transform.position;

	}

}
