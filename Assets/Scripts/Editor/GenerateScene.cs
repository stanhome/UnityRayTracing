using UnityEditor;
using UnityEngine;

public class GenerateScene : EditorWindow
{

	public static GameObject prefabSphere;
	public static Material matLambertian;
	public static Material matMetal;
	public static Material matDielectric;
	public static GameObject gameObjectsRootNode;

	[MenuItem("RayTracing/Generate Final Scene Objects")]
	static void initWindow() {
		GenerateScene window = GetWindow<GenerateScene>("Generate Final Scene Objects", true);
		window.Show();
	}

	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();

		onGUISelect();

		EditorGUILayout.EndVertical();

		if (GUILayout.Button("Generate!"))
		{
			if (isValid())
			{
				ShowNotification(new GUIContent("please fill all filed."));
			}

			// do generate logic
			if (generateSceneObjects())
			{
				ShowNotification(new GUIContent("Generate Scene done."));
			}
			else {
				ShowNotification(new GUIContent("Generate Scene failed."));
			}
		}
	}

	const float POS_Y = 0.2f;
	readonly Vector3 AVOID_POS = new Vector3(4.0f, POS_Y, 0.0f);

	private bool generateSceneObjects() {
		GameObject go;
		Renderer r;
		Material mat = null;

		Random.InitState((int)System.DateTime.Now.Ticks);

		for (int i = -11; i < 11; i++)
		{
			for (int j = -11; j < 11; j++)
			{
				float chooseMat = Random.value;
				Vector3 pos = new Vector3(i + 0.9f * Random.value, POS_Y, j + 0.9f * Random.value);

				if ((pos - AVOID_POS).sqrMagnitude > 0.81f)
				{
					go = Instantiate(prefabSphere);
					go.transform.localPosition = pos;
					go.transform.SetParent(gameObjectsRootNode.transform);

					if (chooseMat < 0.7f) {
						// diffuse
						go.name = $"sphere_L_{i}_{j}";
						mat = new Material(matLambertian);
						mat.SetColor("_Color", new Color(
							Random.value * Random.value,
							Random.value * Random.value,
							Random.value * Random.value
						));
					}
					else if(chooseMat < 0.9f) {
						// metal
						go.name = $"sphere_M_{i}_{j}";
						mat = new Material(matMetal);
						mat.SetColor("_Color", new Color(
							(float)(0.5f + 0.5 * Random.value),
							(float)(0.5f + 0.5 * Random.value),
							(float)(0.5f + 0.5 * Random.value)
						));
					}
					else {
						// glass
						go.name = $"sphere_D_{i}_{j}";
						mat = new Material(matDielectric);
						mat.SetColor("_Color", Color.white);
						mat.SetFloat("_RefIdx", 1.5f);
					}

					r = go.GetComponent<Renderer>();
					r.material = mat;
				}
			}
		}

		return true;
	}


	public static bool isValid()
	{
		bool hasEmpty = (prefabSphere == null || matLambertian == null || matMetal == null || matDielectric == null || gameObjectsRootNode == null);

		return !hasEmpty;
	}

	public static void onGUISelect()
	{
		// bool : allowSceneObjects
		prefabSphere = (GameObject)EditorGUILayout.ObjectField("sphere prefab", prefabSphere, typeof(GameObject), false);
		matLambertian = (Material)EditorGUILayout.ObjectField("Lambertian material", matLambertian, typeof(Material), false);
		matMetal = (Material)EditorGUILayout.ObjectField("Metal material", matMetal, typeof(Material), false);
		matDielectric = (Material)EditorGUILayout.ObjectField("Dielectric material", matDielectric, typeof(Material), false);

		gameObjectsRootNode = (GameObject)EditorGUILayout.ObjectField("Gameobject root node", gameObjectsRootNode, typeof(GameObject), true);
	}
}
