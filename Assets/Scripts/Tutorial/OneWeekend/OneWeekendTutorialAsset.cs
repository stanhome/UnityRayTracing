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
			case "OutputColorTutorial":
			case "BackgroundTutorial": ret = new OutputColorTutorial(this); break;
			case "OneSphereTutorial": ret = new OneSphereTutorial(this); break;
			case "AntialiasingTutorial": 
			case "DiffuseTutorial": ret = new AntialiasingTutorial(this); break;
			case "FocusCameraTutorial": ret = new FocusCameraTutorial(this); break;
		}

		return ret;
	}
}
