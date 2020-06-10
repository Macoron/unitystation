using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adding this as a separate class so we can easily extend it in future -
/// add price or required access, stock amount and etc.
/// </summary>
[System.Serializable]
public class VendorItem
{
	public GameObject Item;
	public int Stock = 5;

	public VendorItem(VendorItem item)
	{
		this.Item = item.Item;
		this.Stock = item.Stock;
	}
}

[CreateAssetMenu(fileName = "VendorMachineContent", menuName = "ScriptableObjects/Machines/VendorMachineContent")]
public class VendorMachineContent : ScriptableObject
{
	[ArrayElementTitle("Item")]
	public List<VendorItem> ContentList = new List<VendorItem>();
}
