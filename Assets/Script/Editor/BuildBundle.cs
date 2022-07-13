using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class BuildBundle : Editor
{
    [MenuItem("Assets/Yoons Build Bundles")]
    static void BuildAssetBundle()
    {
        BuildPipeline.BuildAssetBundles(@"/Users/yoon.lee/Build", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX);
    }

}
