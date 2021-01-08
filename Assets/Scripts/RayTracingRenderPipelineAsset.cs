using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(fileName = "RayTracingRenderPipelineAsset", menuName ="Rendering/RayTracingRenderPiplineAsset", order =-1)]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
	public RayTracingTutorialAsset tutorialAsset;

	protected override RenderPipeline CreatePipeline()
	{
		return new RayTracingRenderPipeline(this);
	}
}
