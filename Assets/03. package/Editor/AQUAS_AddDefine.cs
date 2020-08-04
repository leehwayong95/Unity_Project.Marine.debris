using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AQUAS
{
	[InitializeOnLoad]
	public class AQUAS_AddDefine : Editor {
        
		static AQUAS_AddDefine()
		{

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

			if (!symbols.Contains("AQUAS_PRESENT"))
			{
				symbols += ";" + "AQUAS_PRESENT";
				PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
			}

            string[] results = AssetDatabase.FindAssets("PostProcessingProfile");

            if (!symbols.Contains("UNITY_POST_PROCESSING_STACK_V1") && results.Length>0)
            {
                symbols += ";" + "UNITY_POST_PROCESSING_STACK_V1";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }

            if (symbols.Contains("UNITY_POST_PROCESSING_STACK_V1") && results.Length == 0)
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V1;", "");
                symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V1", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }

#if UNITY_2017 || UNITY_5_6
            results = AssetDatabase.FindAssets("PostProcessLayer");

            if (!symbols.Contains("UNITY_POST_PROCESSING_STACK_V2") && results.Length>0)
            {
                symbols += ";" + "UNITY_POST_PROCESSING_STACK_V2";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
            

            if (symbols.Contains("UNITY_POST_PROCESSING_STACK_V2") && results.Length == 0)
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2;", "");
                symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
#endif
        }
	}
}
