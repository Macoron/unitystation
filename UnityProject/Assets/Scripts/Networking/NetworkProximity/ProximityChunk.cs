using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single chunk that stores all ProximityObject in bounds
/// </summary>
public class ProximityChunk : IEnumerable<ProximityObject>
{
	/// <summary>
	/// Chunk witdh and height in meters
	/// </summary>
	public const float Size = 5;

	/// <summary>
	/// Chunk position in world grid
	/// </summary>
	public readonly Vector2Int ChunkPos;

	/// <summary>
	/// Set of all object inside chunk
	/// </summary>
	public HashSet<ProximityObject> ObjectsInside = new HashSet<ProximityObject>();

	public ProximityChunk(Vector2Int chunkPos)
	{
		ChunkPos = chunkPos;
	}

	public void RegisterObject(ProximityObject obj)
	{
		// Check if we didn't register object before
		if (ObjectsInside.Contains(obj))
		{
			Logger.LogError($"Attemping register {obj} in chunk {ChunkPos} twice!", Category.Proximity);
			return;
		}

		ObjectsInside.Add(obj);
	}

	public bool HasObject(ProximityObject obj)
	{
		return ObjectsInside.Contains(obj);
	}

	public void UnregisterObject(ProximityObject obj)
	{
		// Check if we didn't unregister object before
		if (!ObjectsInside.Contains(obj))
		{
			Debug.LogError($"Can't unregister {obj} - chunk {ChunkPos} doesn't contain it");
			return;
		}

		ObjectsInside.Remove(obj);
	}

	IEnumerator<ProximityObject> IEnumerable<ProximityObject>.GetEnumerator()
	{
		return ObjectsInside.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ObjectsInside.GetEnumerator();
	}
}
