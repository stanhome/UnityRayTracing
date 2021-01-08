using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RayTracingRenderPipeline : RenderPipeline
{
	private RayTracingRenderPipelineAsset _asset;

	private RayTracingTutorial _tutorial;

	public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset) {
		_asset = asset;

		// acceleration structure

		_tutorial = _asset.tutorialAsset.createTutorial();
		if (_tutorial == null)
		{
			Debug.LogError("Can't create tutorial.");
			return;
		}

		if (!_tutorial.init(this))
		{
			_tutorial = null;
			Debug.LogError("Initialize tutorial failed.");
			return;
		}
	}

	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		if(!SystemInfo.supportsRayTracing) {
			Debug.LogError("Your system is not support ray tracing.");
			return;
		}

		BeginFrameRendering(context, cameras);
		Array.Sort(cameras, (lhs, rhs) => {
			return (int)(lhs.depth - rhs.depth);
		});

		// build acceleration structure

		foreach (Camera c in cameras)
		{
			// only render game and scene view camera
			if (c.cameraType != CameraType.Game && c.cameraType != CameraType.SceneView) continue;

			BeginCameraRendering(context, c);
			_tutorial?.render(context, c);
			context.Submit();
			EndCameraRendering(context, c);
		}

		EndFrameRendering(context, cameras);
	}
}
