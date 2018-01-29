using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

	public Interactables focus;
    public bool selected = false;
	public LayerMask movementMask;

	Camera cam;
	PlayerMotor motor;


	// Use this for initialization
	void Start () {
		cam = Camera.main;
		motor = GetComponent<PlayerMotor>();
	}
	
	// Update is called once per frame
	void Update () {
        if (selected == true)
        {
            Debug.Log("selected");
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("moving");
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 20000, movementMask))
                {
                    motor.MoveToPoint(hit.point);
                    //Move to

                    RemoveFocus();
                }


            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 20000))
                {
                    Interactables interactable = hit.collider.GetComponent<Interactables>();
                    if (interactable != null)
                    {
                        SetFocus(interactable);
                    }

                }


            }
        }
			
	}

	void SetFocus (Interactables newFocus)
	{
		if (newFocus != focus)
		{
			if (focus != null) 
				focus.OnDefocused ();
			focus = newFocus;
			motor.FollowTarget (newFocus);
		}
		newFocus.OnFocused (transform);
	}

	void RemoveFocus ()
	{
		if (focus != null)
			focus.OnDefocused ();
		focus = null;
		motor.StopFollowingTarget ();
	}
}
