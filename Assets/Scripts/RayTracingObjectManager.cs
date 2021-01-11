using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class RayTracingObjectManager : MonoBehaviour
{
	// need to ray tracing object's renderer
	public Renderer[] renderers;
	public GameObject RayTracingObjectRoot = null;

	const int MAX_NUM_SUBMESHES = 32;
	private bool[] subMeshFlagArr = new bool[MAX_NUM_SUBMESHES];
	private bool[] subMeshCutoffArr = new bool[MAX_NUM_SUBMESHES];

	private bool _isDirty = true;

	private RayTracingAccelerationStructure _accelerationStructure;
	public RayTracingAccelerationStructure accelerationStructure { get { return _accelerationStructure; } }


	private static RayTracingObjectManager s_instance = null;
	public static RayTracingObjectManager instance {
		get {
			if (s_instance == null) {
				s_instance = FindObjectOfType<RayTracingObjectManager>();
				s_instance?.init();
			}

			return s_instance;
		}
	}

	public void Awake()
	{
		if (Application.isPlaying)
		{
			DontDestroyOnLoad(this);
		}

		_accelerationStructure = new RayTracingAccelerationStructure();
	}

	public void Start()
	{
		addAllRenderersWithChildren(RayTracingObjectRoot);
	}

	private void OnDestroy()
	{
		if (_accelerationStructure != null)
		{
			_accelerationStructure.Dispose();
			_accelerationStructure = null;
		}
	}


	private void init() {
		for (int i = 0; i < MAX_NUM_SUBMESHES; i++)
		{
			subMeshFlagArr[i] = true;
			subMeshCutoffArr[i] = false;
		}
	}


	public void addAllRenderersWithChildren(GameObject objRoot) {
		if (objRoot == null) return;

		Renderer[] renderersInChildren = objRoot.GetComponentsInChildren<Renderer>();
		renderers = renderersInChildren;

		_isDirty = true;
	}

	public void buildAccelerationStructure()
	{
		if (!_isDirty) return;

		// create a new acceleration structure to clear
		_accelerationStructure.Dispose();
		_accelerationStructure = new RayTracingAccelerationStructure();

		// fill acceleration structure
		foreach (Renderer r in renderers)
		{
			if (r) _accelerationStructure.AddInstance(r, subMeshFlagArr, subMeshCutoffArr);
		}

		_accelerationStructure.Build();

		_isDirty = false;
	}

}
