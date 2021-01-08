using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChangePipeline : MonoBehaviour
{
	public RenderPipelineAsset newRenderPipelineAsset;

	private RenderPipelineAsset _oldPipline;

    IEnumerator Start()
    {
		yield return new WaitForEndOfFrame();

		_oldPipline = GraphicsSettings.renderPipelineAsset;
		GraphicsSettings.renderPipelineAsset = newRenderPipelineAsset;
	}

	void OnDestroy() {
		GraphicsSettings.renderPipelineAsset = _oldPipline;
	}
}
