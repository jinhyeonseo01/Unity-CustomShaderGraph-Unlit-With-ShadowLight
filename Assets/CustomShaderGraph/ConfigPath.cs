using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ConfigPath
{
    public const string rootPath = "Assets/CustomShaderGraph/";
    
    public const string customUnlitForwardPassPath = rootPath + "Custom Tamplate/Editor/ShaderGraph/Includes/UnlitPass.hlsl";
    public const string customUnlitGBufferPassPath = rootPath + "Custom Tamplate/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl";

    public static readonly string RootPath = GetRootPath();

    public static string GetRootPath([CallerFilePath] string filePath = "")
    {
        // 파일 경로의 구분자를 통일 (슬래시 사용)
        filePath = filePath.Replace("\\", "/");
        // 파일이 위치한 디렉토리를 가져옴
        string directory = System.IO.Path.GetDirectoryName(filePath);
        // directory에도 Replace 적용
        directory = directory.Replace("\\", "/");

        // Assets 폴더에 있는 경우
        int assetsIndex = directory.IndexOf("Assets/");
        if (assetsIndex >= 0)
        {
            // "Assets" 이후의 경로를 반환 (예: Assets/CustomShaderGraph)
            return directory.Substring(assetsIndex) + "/";
        }

        // Packages 폴더에 있는 경우
        int packagesIndex = directory.IndexOf("Packages/");
        if (packagesIndex >= 0)
        {
            // "Packages" 이후의 경로를 반환 (예: Packages/Unity Custom Shader Graph)
            return directory.Substring(packagesIndex) + "/";
        }

        return filePath;
    }
}
