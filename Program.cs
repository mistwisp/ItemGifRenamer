using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public class FileInfo
{
    public string ID { get; set; }
    public string FileName { get; set; }
    public string BundleNum { get; set; }
}

class Program
{
    static List<FileInfo> ReadXmlFiles(string folder)
    {
        List<FileInfo> fileInfos = new List<FileInfo>();
        string[] xmlFiles = Directory.GetFiles(folder, "*.xml");
        foreach (var txtFile in xmlFiles)
        {
            string content = File.ReadAllText(txtFile);
            var matches = Regex.Matches(content, "<ROW>(.*?)</ROW>", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var row = match.Groups[1].Value;
                var idMatch = Regex.Match(row, "<ID>(.*?)</ID>");
                var fileNameMatch = Regex.Match(row, @"<FileName><!\[CDATA\[(.*?)\]\]></FileName>");
                var bundleNumMatch = Regex.Match(row, "<BundleNum>(.*?)</BundleNum>");
                if (idMatch.Success && fileNameMatch.Success && bundleNumMatch.Success)
                {
                    var fileName = fileNameMatch.Groups[1].Value;
                    var lastBackslashIndex = fileName.LastIndexOf("\\");
                    var extractedFileName = "";
                    if (lastBackslashIndex != -1)
                    {
                        extractedFileName = fileName.Substring(lastBackslashIndex + 1);
                    }
                    else
                    {
                        extractedFileName = fileName;
                    }
                    var fileInfo = new FileInfo
                    {
                        ID = idMatch.Groups[1].Value,
                        FileName = extractedFileName,
                        BundleNum = bundleNumMatch.Groups[1].Value
                    };
                    Console.WriteLine($"Add ID {fileInfo.ID}: FileName {fileInfo.FileName}");
                    fileInfos.Add(fileInfo);
                }
                else
                {
                    Console.WriteLine($"Warning: Incomplete data in {txtFile}. Skipping.");
                }
            }
        }
        Console.WriteLine($"=========================================================");
        return fileInfos;
    }

    static void RenameGifs(string imagesFolder, List<FileInfo> fileInfos)
    {
        string[] gifFiles = Directory.GetFiles(imagesFolder, "*.gif");
        foreach (var gifFile in gifFiles)
        {
            var filename = Path.GetFileNameWithoutExtension(gifFile);
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.ID != null)
                {
                    var paddedBundleNum = fileInfo.BundleNum.PadLeft(3, '0');
                    var oldPath = fileInfo.FileName + "." + paddedBundleNum;
                    if (oldPath == filename)
                    {
                        var newFilename = $"{fileInfo.ID}.gif";
                        var newFilePath = Path.Combine(imagesFolder, newFilename);
                        Console.WriteLine($"Move from {gifFile} to {newFilePath}");
                        File.Move(gifFile, newFilePath);
                        break;
                    }
                }
            }
        }
    }

    static void Main(string[] args)
    {
        string folder = @"D:\Trickster\data"; // Change to your ItemData.xml (From TSGive) folder
        string imagesFolder = @"D:\Trickster\data\image"; // Change to your extracted .gif images folder
        var fileInfos = ReadXmlFiles(folder);
        RenameGifs(imagesFolder, fileInfos);
    }
}
