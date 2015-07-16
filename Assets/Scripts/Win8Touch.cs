using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum Win8TouchPhase
{
	Inactive,
	Began,
	Moved,
	Stationnary,
	Ended
}

public class Win8Touch
{
	public int fingerId { get; private set; }
	public Win8TouchPhase phase { get; private set; }
	public Vector2 position;
	public float touchTime;
	
	public float deltaTime { get { return touchTime - _previousTouchTime; } }
	public Vector2 deltaPosition { get { return position - _previousPosition; } }
	public bool isActive { get { return phase != Win8TouchPhase.Inactive; } }
	
	Vector2 _previousPosition;
	float _previousTouchTime;
	
	const int PIXEL_Y_CORRECTION = -34;
	
	public void TouchDown (MarshalledTouchEvent mTouch)
	{
		fingerId = mTouch.ID;
		phase = Win8TouchPhase.Began;
		position = new Vector2(mTouch.x, (Screen.height - mTouch.y + PIXEL_Y_CORRECTION));
		_previousPosition = position;
		touchTime = Time.time;
		_previousTouchTime = touchTime;
	}
	
	public void TouchMove (MarshalledTouchEvent mTouch)
	{
		phase = Win8TouchPhase.Moved;
		position = new Vector2(mTouch.x, (Screen.height - mTouch.y + PIXEL_Y_CORRECTION));	
		touchTime = Time.time;
	}
	
	public void TouchUp (MarshalledTouchEvent mTouch)
	{
		phase = Win8TouchPhase.Ended;
		position = new Vector2(mTouch.x, (Screen.height - mTouch.y + PIXEL_Y_CORRECTION));	
		touchTime = Time.time;
	}
	
	public void FrameEnd ()
	{
		if (phase == Win8TouchPhase.Began)
			phase = Win8TouchPhase.Stationnary;
	
		if (phase == Win8TouchPhase.Moved && _previousPosition == position)
			phase = Win8TouchPhase.Stationnary;
		
		if (phase == Win8TouchPhase.Ended)
			phase = Win8TouchPhase.Inactive;
		
		_previousPosition = position;
		_previousTouchTime = touchTime;
	}
}
