// See https://aka.ms/new-console-template for more information

using System.Linq;
using Mabinogi_Packer;

const string gamePath = @"I:\package";
const string tempPath = @"I:\temp";
const string _sourcePath = @"H:\新增資料夾 (3)\新增資料夾";
const string packOutput = @"I:\pack";
// var list = await new Packer().List(testFile);
//
// Console.WriteLine(list.Item2);

// Stopwatch stopwatch = Stopwatch.StartNew();
// stopwatch.Start();
// var findAllPackage = await new Packer().FindAllPackageContainsType(gamePath, "tailoring.dds");
// Console.WriteLine($"{findAllPackage.Item1} {findAllPackage.Item2}");
// stopwatch.Stop();
// Console.WriteLine(stopwatch.Elapsed);

//FindPic(@"H:\新增資料夾 (3)");

// 尋找
// 解壓 & temp
// 替換(改大小寫) & 打包
// delTemp

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

async Task ExtractAllMinMap(string gamePath1)
{

    var packer = new Packer(gamePath1);
    var findAllPackage = await packer.FindAll("minimap");

    var value = string.Join('\n', findAllPackage.result);
    Console.WriteLine($"{findAllPackage.success} {value}");

    var enumerable = findAllPackage.result.Select(x => x.Item2).Distinct();
    foreach (var s in enumerable)
    {
        await packer.Extract(Path.Combine(gamePath1, s), @"H:\Output", Array.Empty<string>());
    }
}

async void FindPic(string originPath)
{
    var fileInfos = new DirectoryInfo(originPath).GetFiles("*.jpg");

    foreach (var fileInfo in fileInfos)
    {
        var packer = new Packer(gamePath);
        var result = await packer.FindAllPackageFirst(gamePath, fileInfo.Name);
        if (result.Item1)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{fileInfo.Name}  Find");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{fileInfo.Name} Not Find");
        }

    }
}

namespace Mabinogi_Packer
{
    public class MainWindow
    {
        private static void TestCase()
        {

            var data = @"H:\Origin\data_00869.it";
            var outputFile = @"H:\data_00869.it";
            var outputPath = @"H:\data";

            var d = Do(data, outputPath, outputFile);
        }

        private static async Task Do(string data, string outputPath, string outputFile)
        {

            // await List(data);
            //
            // await Extract(data, outputPath);

            // await Pack(outputPath, outputFile);

            // Compare(data, outputFile);
            //
            // Trace.WriteLine("---------Compare End --------");
        }

        // private static async Task Pack(string outputPath, string outputFile)
        // {
        //
        //     var pack = await new Packer().Pack(outputPath, outputFile);
        //     Trace.WriteLine(pack.Item1);
        //     Trace.WriteLine(pack.Item2);
        //     Trace.WriteLine("---------Pack End --------");
        // }
        //
        // private static async Task Extract(string data, string outputPath)
        // {
        //
        //     var extract = await new Packer().Extract(data, outputPath, new string[] { });
        //     Trace.WriteLine(extract.Item1);
        //     Trace.WriteLine(extract.Item2);
        //     Trace.WriteLine("---------Extract End --------");
        // }
        //
        // private static async Task<(bool, string)> List(string data)
        // {
        //
        //     var list = await new Packer().List(data);
        //     Trace.WriteLine(list.Item1);
        //     Trace.WriteLine(list.Item2);
        //     Trace.WriteLine("---------list End --------");
        //     return list;
        // }
    }
}