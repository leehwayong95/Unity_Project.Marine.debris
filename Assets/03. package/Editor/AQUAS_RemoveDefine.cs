using UnityEditor;

namespace AQUAS
{
    public class AQUAS_RemoveDefine : UnityEditor.AssetModificationProcessor
    {

        static string symbols;

        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {

            if (assetPath.Contains("AQUAS"))
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                if (symbols.Contains("AQUAS_PRESENT"))
                {
                    symbols = symbols.Replace("AQUAS_PRESENT;", "");
                    symbols = symbols.Replace("AQUAS_PRESENT", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }
            }

            if (assetPath.Contains("PostProcessing")) 
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                if (symbols.Contains("UNITY_POST_PROCESSING_STACK_V1"))
                {
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V1;", "");
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V1", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }

                if (symbols.Contains("UNITY_POST_PROCESSING_STACK_V2"))
                {
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2;", "");
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }
            }

            if (assetPath.Contains("PostProcessing-2"))
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                if (symbols.Contains("UNITY_POST_PROCESSING_STACK_V2"))
                {
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2;", "");
                    symbols = symbols.Replace("UNITY_POST_PROCESSING_STACK_V2", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
