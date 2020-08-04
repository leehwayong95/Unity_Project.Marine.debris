#if UNITY_EDITOR

using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V1 && !UNITY_POST_PROCESSING_STACK_V2 && AQUAS_PRESENT
using UnityEngine.PostProcessing;
#endif
#if UNITY_POST_PROCESSING_STACK_V2 && AQUAS_PRESENT
using UnityEngine.Rendering.PostProcessing;
#endif

public class AQUAS_CameraSwitcher : MonoBehaviour {

    public GameObject camerappv1;
    public GameObject camerappv2;

	// Use this for initialization
	void Awake () {

#if AQUAS_PRESENT && UNITY_POST_PROCESSING_STACK_V1 && !UNITY_POST_PROCESSING_STACK_V2
        camerappv2.SetActive(false);
        camerappv1.SetActive(true);
#endif

#if AQUAS_PRESENT && UNITY_POST_PROCESSING_STACK_V2
        camerappv2.SetActive(true);
        camerappv1.SetActive(false);

        camerappv2.GetComponentInChildren<AQUAS_LensEffects>().underWaterParameters.underwaterProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/AQUAS/Post Processing/AQUAS_Underwater_v2.asset", typeof(PostProcessProfile));
        camerappv2.GetComponentInChildren<AQUAS_LensEffects>().underWaterParameters.defaultProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/AQUAS/Post Processing/DemoPostProcessing_v2.asset", typeof(PostProcessProfile));
#endif

    }
}

#endif
