using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class OneSphereTutorial : RayTracingTutorial
{
	public OneSphereTutorial(RayTracingTutorialAsset asset) : base(asset)
	{
		
	}

	public override void render(ScriptableRenderContext context, Camera c)
	{
		base.render(context, c);

		RTHandle outputTarget = getOutputTarget(c);
		CommandBuffer cmd = CommandBufferPool.Get(typeof(OneSphereTutorial).Name);
		try
		{
			using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
			{
				cmd.SetRayTracingShaderPass(_asset.shader, "MyRayTracing");
				cmd.SetRayTracingAccelerationStructure(_asset.shader, SID_accelerationStructure, RayTracingObjectManager.instance.accelerationStructure);
				cmd.SetRayTracingTextureParam(_asset.shader, SID_outputTarget, outputTarget);
				cmd.DispatchRays(_asset.shader, "SphereRaygenShader", (uint)outputTarget.rt.width, (uint)outputTarget.rt.height, 1, c);
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
