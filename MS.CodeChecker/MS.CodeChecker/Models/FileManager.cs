namespace MS.CodeChecker.Models;

public static class FileManager
{
    private const string CredentialsFilename = "credentials.txt";
    private const string CodesFilename = "codes.txt";
    private const string ValidCodesFilename = "valid.txt";

    public static void CheckEnvironment()
    {
        if (!File.Exists(CredentialsFilename))
        {
            File.Create(CredentialsFilename);
            throw new FileNotFoundException("Отсутствующий файл с учётными данными был создан. Для начала работы заполните его.");
        }
        
        if (!File.Exists(CodesFilename))
        {
            File.Create(CodesFilename);
            throw new FileNotFoundException("Отсутствующий файл с кодами был создан. Для начала работы заполните его.");
        }

        if (!File.Exists(ValidCodesFilename))
            File.Create(ValidCodesFilename);

        GetSourceCodes();
    }
    
    private static List<string> ReadFile(string file)
    {
        StreamReader fileStream = new StreamReader(file);
        List<string> fileData = new List<string>();

        while (fileStream.ReadLine() is { } line)
            fileData.Add(line.Replace("\n", ""));
        
        fileStream.Close();
        
        return fileData;
    }

    public static string[] GetCredentials() => ReadFile(CredentialsFilename).ToArray();
    
    public static List<string> GetSourceCodes()
    {
        List<string> codes = ReadFile(CodesFilename);

        if (!codes.Any())
            throw new ArgumentException("Файл с кодами пуст!");

        return codes;
    }

    public static IEnumerable<string> GetProcessedCodes()
        => ReadFile(ValidCodesFilename).Select(codeDump => codeDump.Split(" ")[0]).ToList();

    public static void WriteValidCode(string code, CodeStatus? status)
    {
        StreamWriter streamWriter = new StreamWriter(ValidCodesFilename, true);

        string codeStatus = status != null ? " - " + status : "";
        
        streamWriter.WriteLine($"{code}{codeStatus}");
        streamWriter.Close();
    }
}
