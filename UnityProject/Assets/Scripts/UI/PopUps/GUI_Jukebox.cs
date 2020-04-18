using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_Jukebox : NetTab
{

	//Close the screen.
	public void CloseDialog()
	{
		ControlTabs.CloseTab(Type, Provider);
	}
}
