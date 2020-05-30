using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows object to see/hide proximity objects depending on their distance
/// </summary>
public class ProximityObserver : NetworkBehaviour
{
	[Tooltip("How far observer can see networked objects. The faster object moves the bigger this value should be.")]
	public float ViewRadius = 1;

	private HashSet<ProximityObject> lastObservedObjects
		= new HashSet<ProximityObject>();

	[ServerCallback]
	private void Update()
	{
		//if (!ProximityManager.Instance)
			//return;

		// Get viewer position
		var viewerPos = transform.position;

		// Find all visible objects in view radius
		var curObservedObjects = ProximityManager.Instance.GetVisibleObjects(viewerPos, ViewRadius);
		curObservedObjects.Add(GetComponent<ProximityObject>());

		// Check if old objects left observer sight 
		foreach (var obj in lastObservedObjects)
		{
			if (!obj)
			{
				// looks like object got destroyed - better delete it now from set
				curObservedObjects.Remove(obj);
				continue;
			}

			if (!curObservedObjects.Contains(obj))
			{
				obj.RemoveObserver(connectionToClient);
			}
		}

		// Add this observer to all new visible objects
		foreach (var obj in curObservedObjects)
		{
			if (!obj)
			{
				continue;
			}

			if (!lastObservedObjects.Contains(obj))
			{
				obj.AddObserver(connectionToClient);
			}
		}

		lastObservedObjects = curObservedObjects;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying || !isServer)
			return;

		Gizmos.color = Color.green;

		var visibleObjects = ProximityManager.Instance.GetVisibleObjects(transform.position, ViewRadius);

		var viewerPos = transform.position;
		foreach (var obj in visibleObjects)
		{
			if (obj)
			{
				var objPos = obj.transform.position;
				Gizmos.DrawLine(viewerPos, objPos);
			}
		}
	}
}
