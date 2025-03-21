using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ConfigPath
{
    public const string packageName = "com.clrain.customshadergraph";


    // User Include hlsl Path
    public static readonly string customUnlitForwardPassPath =  GetRootPath() + "Includes/CustomUnlitForwardPass.hlsl";
    public static readonly string customUnlitGBufferPassPath =  GetRootPath() + "Includes/CustomUnlitGBufferPass.hlsl";

    public static readonly string customLitForwardPassPath =    GetRootPath() + "Includes/CustomLitForwardPass.hlsl";
    public static readonly string customLitGBufferPassPath =    GetRootPath() + "Includes/CustomLitGBufferPass.hlsl";
    public static readonly string customLit2DPassPath =         GetRootPath() + "Includes/CustomLit2DPass.hlsl";

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
