using System;
using System.IO;

namespace API.Constants;

public class FileStorage
{
    private static readonly string HomePath = Directory.GetCurrentDirectory();
    
    public static readonly string FileStorageDir = Path.Combine(HomePath, "Files");

    public static readonly string DownloadUrl = EnvironmentMode.ClientHostForEmail + "/api/web/v1/me/projects/downloads/";
}