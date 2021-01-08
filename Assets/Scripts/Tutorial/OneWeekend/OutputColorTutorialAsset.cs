using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutputColorTutorialAsset", menuName ="Rendering/OutputColorTutorialAsset")]
public class OutputColorTutorialAsset : RayTracingTutorialAsset
{
	public override RayTracingTutorial createTutorial()
	{
		return new OutputColorTutorial(this);
	}
}
