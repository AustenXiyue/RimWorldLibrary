namespace System.IO;

internal static class FileHelper
{
	internal static FileStream CreateAndOpenTemporaryFile(out string filePath, FileAccess fileAccess = FileAccess.Write, FileOptions fileOptions = FileOptions.None, string extension = null, string subFolder = "WPF")
	{
		int num = 5;
		filePath = null;
		string path = Path.GetTempPath();
		if (!string.IsNullOrEmpty(subFolder))
		{
			string text = Path.Combine(path, subFolder);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			path = text;
		}
		FileStream fileStream = null;
		while (fileStream == null)
		{
			string text2 = Path.Combine(path, Path.GetRandomFileName());
			if (!string.IsNullOrEmpty(extension))
			{
				text2 = Path.ChangeExtension(text2, extension);
			}
			num--;
			try
			{
				fileStream = new FileStream(text2, FileMode.CreateNew, fileAccess, FileShare.None, 4096, fileOptions);
				filePath = text2;
			}
			catch (Exception ex) when (num > 0 && (ex is IOException || ex is UnauthorizedAccessException))
			{
			}
		}
		return fileStream;
	}

	internal static void DeleteTemporaryFile(string filePath)
	{
		if (!string.IsNullOrEmpty(filePath))
		{
			try
			{
				File.Delete(filePath);
			}
			catch (IOException)
			{
			}
		}
	}
}
