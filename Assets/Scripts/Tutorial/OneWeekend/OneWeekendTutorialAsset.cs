using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OneWeekendTutorialAsset", menuName = "Rendering/OneWeekendTutorialAsset")]
public class OneWeekendTutorialAsset : RayTracingTutorialAsset
{
	public string tutorialName;

	public override RayTracingTutorial createTutorial()
	{
		RayTracingTutorial ret = null;

		switch (tutorialName)
		{
			default:
			case "OutputColorTutorial": ret = new OutputColorTutorial(this); break;
			case "BackgroundTutorial": ret = new BackgroundTutorial(this); break;
		}

		return ret;
	}
}
