using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MarshalledTouchEvent
{
    public int x;
    public int y;
    public int ID;
    public int state;
    public int time;
};

public class Win8TouchInput : MonoBehaviour
{
    public const string TouchHookDLLName = "TouchHookx86";

    /// <summary>Initialize the Touch hook DLL with the name of your window. Use this initializer if you want to track the points for the whole area of a window</summary> 
    /// <param name="Str">name of your window (usually the name put in the top border)</param>
    /// <param name="deviceTouchCapabilities">number of touch points your device is able to handle.</param>
    /// <returns>0 if everything gone well, -x if not. </returns>
    [DllImport(TouchHookDLLName)]
    static extern int InitializeWindowArea(string Str, int deviceTouchCapabilities);

    /// <summary>Initialize the Touch hook DLL with the name of your window.
    /// use this initializer if you want to track the points for a specific area a window. 
    /// eg. for the area of a specific control.
    /// </summary> 
    /// <param name="Str">name of your window (usually the name put in the top border)</param>
    /// <param name="deviceTouchCapabilities">number of touch points your device is able to handle.</param>
    /// <param name="windowTopXPos">off set of the top corner of your control from the top corner of the screen</param>
    /// <param name="windowTopYPos">off set of the left corner of your control from the left corner of the screen</param>
    /// <returns>0 if everything gone well, -x if not. </returns>
    [DllImport(TouchHookDLLName)]
    static extern int InitializeSpecificArea(string Str, int deviceTouchCapabilities, int windowTopXPos, int windowTopYPos);

    /// <summary>Allows to you to change the area receiving the touch</summary>
    /// <param name="windowTopXPos">off set of the top corner of your control from the top corner of the screen</param>
    /// <param name="windowTopYPos">off set of the left corner of your control from the left corner of the screen</param>
    [DllImport(TouchHookDLLName)]
    static extern void SetWindowPosition(int windowTopXPos, int windowTopYPos);

    /// <summary>Fill and allocate a touch array with the current touch points on your window.</summary>
    /// <param name="touches_not_allocated">pointer of touch events array to be filled and allocated. The right amount of memory is be allocated by the DLL.</param>
    /// <param name="size">size of the array.</param>
    /// <remarks>The position is calculated like this: (0,0) is the top-left corner of your window and (Unity window width,Unity window height) is the bottom-right corner.</remarks>
    [DllImport(TouchHookDLLName)]
    static extern void ReadLastTouchEvents(out IntPtr touches_not_allocated, out int array_size);

	#region Window Title
	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow();
	
	[DllImport("user32.dll")]
	static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
	
	private string GetActiveWindowTitle()
	{
		const int nChars = 256;
		StringBuilder Buff = new StringBuilder(nChars);
		IntPtr handle = GetForegroundWindow();
		
		if (GetWindowText(handle, Buff, nChars) > 0)
		{
			return Buff.ToString();
		}
		return null;
	}
    #endregion
    
	public static Win8Touch[] Touches { get { return _touches; } }
	
    bool _initialized = false;
    string _windowName;

    static Win8Touch[] _touches = new Win8Touch[10];

#if UNITY_EDITOR
    public static EditorWindow GetMainGameView()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetMainGameView.Invoke(null, null);
        return (EditorWindow)Res;
    }
    
#endif
    void Start()
    {
		_windowName = GetActiveWindowTitle();
		
		for (int i = 0; i < _touches.Length; i++)
			_touches[i] = new Win8Touch();
    }

    void Update()
    {
        if (!_initialized)
        {
#if (UNITY_EDITOR)
            EditorWindow editor = Win8TouchInput.GetMainGameView();
            Rect pos = editor.position;
			
			int result = InitializeSpecificArea(_windowName, _touches.Length, Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
#else
			int result = InitializeWindowArea(_windowName, _touches.Length);
#endif
            if (result < 0)
            	throw new Exception("Initialize Window Area failed. Return " + result);

            _initialized = true;
        }
        else
        {
            PollEventQueue();
        }
    }

    void PollEventQueue()
    {
        IntPtr arrayTouches;
        int size;
		ReadLastTouchEvents(out arrayTouches, out size);
        
        int touchsizeinbytes = Marshal.SizeOf(typeof(MarshalledTouchEvent));
        
		for (int i = 0; i < size; i++)
	    {
	        IntPtr p = new IntPtr((arrayTouches.ToInt32() + i * touchsizeinbytes));
	        MarshalledTouchEvent touchevent = (MarshalledTouchEvent)System.Runtime.InteropServices.Marshal.PtrToStructure(p, typeof(MarshalledTouchEvent));
	        switch (touchevent.state)
	        {
	            case 0: //began
	                TouchDown(touchevent);
	                break;
	            case 1: //move
	                TouchMove(touchevent);
	                break;
	            case 2: //end
	                TouchUp(touchevent);
	                break;
	        }
	    }
    }
    
	void LateUpdate ()
	{
		FrameEnd ();
	}
	
	void FrameEnd()
	{
		for (int i = 0; i < _touches.Length; i++)
		{
			Win8Touch touch = _touches[i];
			
			if (touch.isActive)
				touch.FrameEnd();
		}
	}
	
    void TouchDown(MarshalledTouchEvent nativetouch)
    {
    	Win8Touch touch = GetInactiveTouch();
		touch.TouchDown(nativetouch);
    }

    void TouchMove(MarshalledTouchEvent mTouch)
    {
        Win8Touch touch = GetActiveTouchByID(mTouch.ID);

		if (touch == null || touch.phase == Win8TouchPhase.Began)
			return;

        touch.TouchMove(mTouch);
    }

    void TouchUp(MarshalledTouchEvent mTouch)
    {
		Win8Touch touch = GetActiveTouchByID(mTouch.ID);

		if (touch == null)
			return;

		touch.TouchUp(mTouch);
	}
    
    Win8Touch GetActiveTouchByID (int id)
    {
    	return _touches.FirstOrDefault(t => t.fingerId == id && t.isActive);
    }
    
    Win8Touch GetInactiveTouch ()
    {
    	return _touches.FirstOrDefault(t => !t.isActive);
    }
}
