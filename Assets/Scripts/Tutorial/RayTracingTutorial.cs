using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public abstract class RayTracingTutorial
{
	protected RayTracingTutorialAsset _asset;

	protected RayTracingRenderPipeline _pipeline;

	protected RayTracingTutorial(RayTracingTutorialAsset asset) {
		_asset = asset;
	}

	public virtual bool init(RayTracingRenderPipeline pipeline) {
		_pipeline = pipeline;

		return true;
	}

	public virtual void render(ScriptableRenderContext context, Camera c) {
		CameraShaderParams.setupCamera(c);
	}

	public virtual void dispose(bool isDisposing) {
		foreach (var item in _outputTargetsCache)
		{
			RTHandles.Release(item.Value);
		}

		_outputTargetsCache.Clear();
	}

	// the output target cache
	private readonly Dictionary<int, RTHandle> _outputTargetsCache = new Dictionary<int, RTHandle>();
	protected static readonly int SID_outputTarget = Shader.PropertyToID("RenderTarget");

	protected RTHandle getOutputTarget(Camera c)
	{
		RTHandle ret = null;

		int id = c.GetInstanceID();
		if (_outputTargetsCache.TryGetValue(id, out ret)) return ret;

		ret = RTHandles.Alloc(c.pixelWidth, c.pixelHeight, 1,
			DepthBits.None,
			GraphicsFormat.R32G32B32A32_SFloat,
			FilterMode.Point,
			TextureWrapMode.Clamp,
			TextureDimension.Tex2D,
			true, false, false, false,
			1, 0f, MSAASamples.None,
			false,
			false,
			RenderTextureMemoryless.None,
			$"OutputTarget_{c.name}");

		_outputTargetsCache.Add(id, ret);

		return ret;
	}

	private readonly Dictionary<int, Vector4> _outputTargetSizesCache = new Dictionary<int, Vector4>();
	protected static readonly int SID_outputTargetSize = Shader.PropertyToID("_RenderTargetSize");

	protected Vector4 getOutputTargetSize(Camera c) {
		Vector4 ret;

		do
		{
			int id = c.GetInstanceID();
			if (_outputTargetSizesCache.TryGetValue(id, out ret)) break;

			ret = new Vector4(c.pixelWidth, c.pixelHeight, 1.0f / c.pixelWidth, 1.0f / c.pixelHeight);
			_outputTargetSizesCache.Add(id, ret);

		} while (false);

		return ret;
	}

	public readonly static int SID_accelerationStructure = Shader.PropertyToID("_AccelerationStructure");
	public readonly static int SID_PRNGStates = Shader.PropertyToID("_PRNGStates");
	public readonly static int SID_alphaAA = Shader.PropertyToID("_AlphaAA");

	/////////////////////////////////////////////////////////////////////////
	// Shader Params
	private static class CameraShaderParams {
		public static readonly int SID_WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
		public static readonly int SID_InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");
		public static readonly int SID_CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");

		public static void setupCamera(Camera c) {
			Matrix4x4 projM = GL.GetGPUProjectionMatrix(c.projectionMatrix, false);
			Matrix4x4 viewM = c.worldToCameraMatrix;
			Matrix4x4 viewProjM = projM * viewM;
			Matrix4x4 invViewProjM = Matrix4x4.Inverse(viewProjM);

			Shader.SetGlobalVector(SID_WorldSpaceCameraPos, c.transform.position);
			Shader.SetGlobalMatrix(SID_InvCameraViewProj, invViewProjM);
			Shader.SetGlobalFloat(SID_CameraFarDistance, c.farClipPlane);
		}
	}
}
