using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ConfigPath
{
    public const string packageName = "com.clrain.customshadergraph";
    public static readonly string customUnlitForwardPassPath = $"{GetRootPath()}Custom Tamplate/Editor/ShaderGraph/Includes/UnlitPass.hlsl";
    public static readonly string customUnlitGBufferPassPath = $"{GetRootPath()}Custom Tamplate/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl";

    public static string GetRootPath([CallerFilePath] string filePath = "")
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        filePath = filePath.Replace("\\", "/");
        filePath = System.IO.Path.GetDirectoryName(filePath);
        filePath = filePath.Replace("\\", "/");
        projectRoot = projectRoot.Replace("\\", "/");
        if (filePath.StartsWith(projectRoot))
        {
            string relativePath = filePath.Substring(projectRoot.Length);
            if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);
            if (relativePath.StartsWith("Assets/"))
            {
                return relativePath + "/";
            }
        }
        return $"Packages/{packageName}/";
    }

}
