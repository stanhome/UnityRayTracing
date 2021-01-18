using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class RandomMove : MonoBehaviour
{
	private Animation _anim;
	private AnimationState _animState;

    // Start is called before the first frame update
    void Start()
    {
		_anim = GetComponent<Animation>();
		_animState = _anim.PlayQueued("Jump", QueueMode.PlayNow, PlayMode.StopAll);
		_animState.speed = 0;
	}

	// Update is called once per frame
	void Update()
    {
		RayTracingObjectManager.instance.dirty();
		_animState.normalizedTime = Random.value;
	}
}
