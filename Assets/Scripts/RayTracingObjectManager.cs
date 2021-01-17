using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class RayTracingObjectManager : MonoBehaviour
{
	[Header("Raytracing objects")]
	// need to ray tracing object's renderer
	public Renderer[] renderers;
	public GameObject RayTracingObjectRoot = null;

	const int MAX_NUM_SUBMESHES = 32;
	private bool[] subMeshFlagArr = new bool[MAX_NUM_SUBMESHES];
	private bool[] subMeshCutoffArr = new bool[MAX_NUM_SUBMESHES];

	private bool _isDirty = true;
	private bool _needBuild = true;

	private RayTracingAccelerationStructure _accelerationStructure;
	public RayTracingAccelerationStructure accelerationStructure { get { return _accelerationStructure; } }

	[Header("Config")]
	public bool enableAA = true;
	public bool isStaticObj = true;


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
		_needBuild = true;
	}

	public void buildAccelerationStructure()
	{
		if (_needBuild == false) return;

		// create a new acceleration structure to clear
		_accelerationStructure.Dispose();
		_accelerationStructure = new RayTracingAccelerationStructure();

		// fill acceleration structure
		foreach (Renderer r in renderers)
		{
			if (r) _accelerationStructure.AddInstance(r, subMeshFlagArr, subMeshCutoffArr);
		}

		_accelerationStructure.Build();

		_needBuild = false;
	}

	public void updateAccelerationStructure() {
		if (!_isDirty && isStaticObj) return;

		buildAccelerationStructure();

		foreach (Renderer r in renderers)
		{
			if (r) _accelerationStructure.UpdateInstanceTransform(r);
		}

		_accelerationStructure.Update();

		_isDirty = false;
	}

}
