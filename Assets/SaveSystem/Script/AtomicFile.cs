using System.IO;
using System.Text;

/// <summary>Commit complete JSON files without truncating the previous save.</summary>
public static class AtomicFile
{
    public static void WriteAllText(string destination, string contents)
    {
        string temporary = destination + ".tmp";
        string backup = destination + ".bak";
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(contents);
                writer.Flush();
                stream.Flush(true);
            }
            if (File.Exists(destination)) File.Replace(temporary, destination, backup);
            else File.Move(temporary, destination);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
