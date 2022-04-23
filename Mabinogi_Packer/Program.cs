// See https://aka.ms/new-console-template for more information

using System.Linq;
using Mabinogi_Packer;

const string gamePath = @"F:\package";
const string tempPath = @"F:\temp";
const string _sourcePath = @"E:\新增資料夾 (3)\新增資料夾";
const string packOutput = @"F:\pack";


await ReplaceJpg(gamePath, _sourcePath);
await ComparerD();

async Task ComparerD()
{
    var files = new DirectoryInfo(packOutput).GetFiles("*.it");

    foreach (var fileInfo in files)
    {
        var first = new DirectoryInfo(gamePath).GetFiles(fileInfo.Name).First();

        var tuple = await new Packer(packOutput).List(fileInfo.Name);
        var gamePathTuple = await new Packer(gamePath).List(first.Name);
        var equals = tuple.findFileReuslt.Length == gamePathTuple.findFileReuslt.Length;
        Console.WriteLine(equals);
    }
}

async Task ReplaceJpg(string gamePath, string sourcePath)
{
    var packer = new Packer(gamePath);
    List<(string findedFile, string itFileName)> list = new();
    var sourceFiles = new DirectoryInfo(sourcePath).GetFiles("*.jpg");
    foreach (var sourceFile in sourceFiles)
    {
        var (_, result) = await packer.FindAll(sourceFile.Name);
        foreach (var valueTuple in result)
        {
            Console.WriteLine($"Find it {valueTuple}");
        }

        list.AddRange(result);
    }

    HashSet<string> itFiles = new();


    foreach (var item in list)
    {
        itFiles.Add(item.itFileName);
    }

    await ExtractAndRePackFile(itFiles, packer, sourceFiles, sourcePath, packOutput);

    Console.WriteLine("End");
}

async Task ExtractAndRePackFile(HashSet<string> itFileNames, Packer packer1, FileInfo[] sourceFiles, string sourcePath, string packOutput)
{
    await DeleteFolder(tempPath);

    foreach (var itFileName in itFileNames)
    {
        Console.WriteLine($"start Extract {itFileName}");
        await packer1.Extract(itFileName, tempPath);
        var fileInfos = new DirectoryInfo(tempPath).GetFiles("*.jpg", SearchOption.AllDirectories);
        foreach (var fileInfo in fileInfos)
        {
            var tempFileName = sourceFiles.Select(x => x.Name.ToLowerInvariant()).FirstOrDefault(x => x == fileInfo.Name.ToLowerInvariant());
            if (tempFileName != default)
            {
                var sourcePathFileName = Path.Combine(sourcePath, tempFileName);
                await CopyFileAsync(sourcePathFileName, Path.Combine(fileInfo.FullName));
                await packer1.Pack(tempPath, Path.Combine(packOutput, itFileName));

            }
        }

        await DeleteFolder(tempPath);
    }
}

async Task DeleteFolder(string dir)
{
    await Task.Factory.StartNew(path =>
    {
        if (Directory.Exists((string) path))
        {
            Directory.Delete((string) path, true);
        }


    }, dir);
}

async Task CopyFileAsync(string sourcePaths, string destinationPath)
{
    await using Stream source = File.Open(sourcePaths, FileMode.OpenOrCreate);
    await using Stream destination = File.Create(destinationPath);
    await source.CopyToAsync(destination);

}
