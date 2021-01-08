using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class BackgroundTutorial : RayTracingTutorial
{
	public BackgroundTutorial(RayTracingTutorialAsset asset) : base(asset)
	{
		
	}

	public override void render(ScriptableRenderContext context, Camera c)
	{
		base.render(context, c);

		RTHandle outputTarget = getOutputTarget(c);
		CommandBuffer cmd = CommandBufferPool.Get(typeof(OutputColorTutorial).Name);
		try
		{
			using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
			{
				cmd.SetRayTracingTextureParam(_asset.shader, SID_outputTarget, outputTarget);
				cmd.DispatchRays(_asset.shader, "OutputColorRayGenShader", (uint)outputTarget.rt.width, (uint)outputTarget.rt.height, 1, c);
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
