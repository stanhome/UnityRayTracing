﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class AntialiasingTutorial : RayTracingTutorial
{
	public AntialiasingTutorial(RayTracingTutorialAsset asset) : base(asset)
	{
		
	}

	public override void render(ScriptableRenderContext context, Camera c)
	{
		// alphaAA is ONE when first frame.
		float alphaAA = getLerpAAVal();

		base.render(context, c);

		ComputeBuffer PRNGStates = _pipeline.getPRNGStates(c);

		RTHandle outputTarget = getOutputTarget(c);
		Vector4 outputTargetSize = getOutputTargetSize(c);
		CommandBuffer cmd = CommandBufferPool.Get(typeof(OneSphereTutorial).Name);
		try
		{
			using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
			{
				// for game objects which need to be raytraced
				cmd.SetRayTracingShaderPass(_asset.shader, "MyRayTracing");
				cmd.SetRayTracingAccelerationStructure(_asset.shader, SID_accelerationStructure, RayTracingObjectManager.instance.accelerationStructure);

				cmd.SetRayTracingFloatParam(_asset.shader, SID_alphaAA, alphaAA);
				cmd.SetRayTracingBufferParam(_asset.shader, SID_PRNGStates, PRNGStates);
				cmd.SetRayTracingTextureParam(_asset.shader, SID_outputTarget, outputTarget);
				cmd.SetRayTracingVectorParam(_asset.shader, SID_outputTargetSize, outputTargetSize);
				cmd.DispatchRays(_asset.shader, "AntialiasingRayGenShader", (uint)outputTarget.rt.width, (uint)outputTarget.rt.height, 1, c);
			}
			context.ExecuteCommandBuffer(cmd);

			using (new ProfilingScope(cmd, new ProfilingSampler("FinalBlit")))
			{
				cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
			}
			context.ExecuteCommandBuffer(cmd);
		}
		finally
		{
			CommandBufferPool.Release(cmd);
		}
	}
}
