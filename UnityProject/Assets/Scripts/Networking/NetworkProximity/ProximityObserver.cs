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

	private HashSet<ProximityObject> allObservedObjects
		= new HashSet<ProximityObject>();

	[ServerCallback]
	private void Update()
	{
		if (!ProximityManager.Instance)
			return;

		// Get viewer position
		var viewerPos = transform.position;

		// Find all visible objects in view radius
		var visibleObjects = ProximityManager.Instance.GetVisibleObjects(viewerPos, ViewRadius);

		// Check if old objects left observer sight 
		foreach (var obj in allObservedObjects)
		{
			if (!obj)
			{
				// looks like object got destroyed - better delete it now from set
				visibleObjects.Remove(obj);
				continue;
			}

			if (!visibleObjects.Contains(obj))
			{
				// object left this observer sight
				visibleObjects.Remove(obj);
				obj.RemoveObserver(connectionToClient);
			}
		}

		// Add this observer to all new visible objects
		foreach (var obj in visibleObjects)
		{
			if (!obj)
			{
				continue;
			}

			if (!allObservedObjects.Contains(obj))
			{
				// object enter this observer sight
				visibleObjects.Add(obj);
				obj.AddObserver(connectionToClient);
			}
		}
	}
}
