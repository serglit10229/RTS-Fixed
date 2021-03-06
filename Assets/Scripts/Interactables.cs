using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactables : MonoBehaviour {

	public float radius = 3f;
	public Transform interationTransform;

	bool isFocus = false;
	Transform player;
	bool hasInteracted = false;

	public virtual void Interact ()
	{
		
	}

	void Update()
	{
		if (isFocus && !hasInteracted) 
		{
			float distance = Vector3.Distance (player.position, interationTransform.position);
			if (distance <= radius) 
			{
				Interact ();
				hasInteracted = true;
			}
		}
	}

	public void OnFocused (Transform playerTransform)
	{
		isFocus = true;
		player = playerTransform;
		hasInteracted = false;
	}

	public void OnDefocused ()
	{
		isFocus = false;
		player = null;
		hasInteracted = false;
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (interationTransform.position, radius);

	}
		

}
