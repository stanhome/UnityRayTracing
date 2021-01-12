using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RayTracingRenderPipeline : RenderPipeline
{
	private RayTracingRenderPipelineAsset _asset;

	private RayTracingTutorial _tutorial;

	// for MersenneTwister random
	private readonly Dictionary<int, ComputeBuffer> _PRNGStates = new Dictionary<int, ComputeBuffer>();

	public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset) {
		_asset = asset;

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
		RayTracingObjectManager.instance.buildAccelerationStructure();

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

	protected override void Dispose(bool isDisposing)
	{
		base.Dispose(isDisposing);

		if (_tutorial != null)
		{
			_tutorial.dispose(isDisposing);
			_tutorial = null;
		}

	}

	public ComputeBuffer getPRNGStates(Camera c) {
		int id = c.GetInstanceID();
		ComputeBuffer buf = null;

		do
		{
			if (_PRNGStates.TryGetValue(id, out buf)) break;

			buf = new ComputeBuffer(c.pixelWidth * c.pixelHeight, 4 * 4, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

			var mt19937 = new MersenneTwister.MT.mt19937ar_cok_opt_t();
			mt19937.init_genrand((uint)System.DateTime.Now.Ticks);

			int dataSize = c.pixelWidth * c.pixelHeight * 4;
			uint[] data = new uint[dataSize];
			for (int i = 0; i < dataSize; i++)
			{
				data[i] = mt19937.genrand_int32();
			}

			buf.SetData(data);

			_PRNGStates.Add(id, buf);

		} while (false);

		return buf;
	}
}
