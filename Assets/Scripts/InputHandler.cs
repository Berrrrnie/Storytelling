using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
	static InputHandler _instance;

	public delegate void InputDownHandler(int id);
	public delegate void InputUpHandler(int id);
	
	public static event InputDownHandler InputDown;
	public static event InputUpHandler InputUp;

	void PCInputUpdate ()
	{
		if (Input.GetMouseButtonDown(0))
		{
			InputDown(-1);
			Debug.Log("Click Down");
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			InputUp(-1);
		}
	}

	void TouchUpdate()
	{
	#if UNITY_STANDALONE_WIN
		for (int i = 0; i < Win8TouchInput.Touches.Length; i++)
		{
			Win8Touch touch = Win8TouchInput.Touches[i];
			
			if (!touch.isActive)
				continue;

			if (touch.phase == Win8TouchPhase.Began)
				InputDown(touch.fingerId);
			if (touch.phase == Win8TouchPhase.Ended)
				InputUp(touch.fingerId);
		}
	#else
	
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			
			if (touch.phase == TouchPhase.Began)
				InputDown(touch.fingerId);
				
			if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
				InputUp(touch.fingerId);
		}
	#endif
	}
	
	public static bool GetInputPositionByID (int id, out Vector3 position)
	{
		if (id == -1)
		{
			position = Input.mousePosition;
			return true;
		}
		#if UNITY_STANDALONE_WIN
		if (Win8TouchInput.Touches.Any(t => t.fingerId == id))
		{
			position = Win8TouchInput.Touches.First(t => t.fingerId == id).position;
			return true;
		}
		#else
		if (Input.touches.Any(t => t.fingerId == id))
		{
			position = Input.touches.First(t => t.fingerId == id).position;
			return true;
		}
		#endif
		position = Vector3.zero;
	    return false;
	}

	void Update ()
	{
	#if UNITY_EDITOR
		PCInputUpdate();
	#endif		
		TouchUpdate();
	}
}
