using UnityEngine;
using System.Collections;
using Janus;

public class PlayerCharacterController : MonoBehaviour {

	public float moveSpeed = 0.5f;            // speed of the character
	Timeline<Vector3> charPositionTimeline;   // timeline for synchronizing the position of the character
	public bool owner = true;                 // if true, this client created this cube and can select it, move it and delete it
	uint playerNumber = 1;

	protected Animator anim;
	private Rigidbody rb;

	int isWalkingHash = Animator.StringToHash ("isWalking");


	public void Awake () {

		gameObject.name = "Player " + playerNumber;
		//transform.position = new Vector3(44f,36.5f,33);

		charPositionTimeline = TimelineManager.Default.Get<Vector3>(gameObject.name);
		// add a send filter to reduce the number of messages
		charPositionTimeline.AddSendFilter(TimelineUtils.BuildDeltaRateFilter <Vector3>((x,y) => Vector3.Distance(x,y), ()=>0.05f, ()=>2.0f));

		if (playerNumber == Janus.TimelineClient.Index){
			owner = true;
		}
	}

	public void Start() {

		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody> ();

	}

	// remove the timeline and destroy the game object
	public void OnDestroy()
	{		
		TimelineManager.Default.Remove(this.charPositionTimeline);
	}
	
	// Update is called once per frame
	void Update () {

		/*
		if (rb.velocity.magnitude > 0.45f) { 
			anim.SetBool (isWalkingHash, true);
		} else {
			anim.SetBool (isWalkingHash, false);
		}
		*/

		if(owner){

			float h = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
			float v = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
			transform.Translate(h,0,v);

			if(Input.GetKeyDown(KeyCode.T)){

				//do some talking
				//TimelineManager.Default.Get<string>("command")[0] = "talk"+this.name;
			}

			if (charPositionTimeline != null){
				charPositionTimeline[0] = transform.position;
			}

		} else { // not owner

			transform.position = charPositionTimeline[-0.05f];

		}
	
	}
}
