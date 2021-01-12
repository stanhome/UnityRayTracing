using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class AntialiasingTutorial : RayTracingTutorial
{
	private int _AAFrameCount = 0;

	public AntialiasingTutorial(RayTracingTutorialAsset asset) : base(asset)
	{
		
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>0: 停止渲染， 1: 关闭 AA， (0, 1) 使用上一帧的数据进行AA</returns>
	public float getLerpAAVal() {
		if (! RayTracingObjectManager.instance.enableAA) {
			_AAFrameCount = 0;
			// disable aa
			return 1.0f;
		}
		else {
			float alphaAA = _AAFrameCount > 0 ? 1.0f / _AAFrameCount : 1.0f;
			if (_AAFrameCount > 1000)
			{
				alphaAA = 0;
			}

			return alphaAA;
		}
	}

	public override void render(ScriptableRenderContext context, Camera c)
	{
		base.render(context, c);

		// alphaAA is ONE when first frame.
		float alphaAA = getLerpAAVal();

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

		if (c.cameraType == CameraType.Game && RayTracingObjectManager.instance.enableAA)
		{
			_AAFrameCount++;
		}
	}
}
