﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class RayTracingTutorialAsset : ScriptableObject
{
	public RayTracingShader shader;

	public Cubemap skybox;

	public abstract RayTracingTutorial createTutorial();
}
