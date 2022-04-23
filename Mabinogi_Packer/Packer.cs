using System.Diagnostics;
using System.Text;

namespace Mabinogi_Packer;

internal class Packer
{
    private const string corePath = @"C:\Users\zxc30\Documents\GitHub\mabi-pack2\target\release\mabi-pack2.exe";
    private readonly string _root;

    public Packer(string root)
    {
        _root = root;
    }

    public Task<(bool succese, string findFileReuslt)> List(string fileName)
    {
        var command = $"list -i {Path.Combine(_root, fileName)}";
        return RunCmd(command);
    }

    private static Task<(bool succese, string result)> RunCmd(string command)
    {

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = corePath,
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        proc.Start();
        var tasksArr = new[] {proc.StandardOutput.ReadToEndAsync(), proc.StandardError.ReadToEndAsync()};
        return Task<(bool, string)>
            .Factory
            .ContinueWhenAll(tasksArr, tasks =>
            {
                var isError = !string.IsNullOrEmpty(tasksArr[1].Result) || tasksArr[0].Result.Contains("os error");
                var result = tasksArr[isError ? 1 : 0].Result;
                return (!isError, result);
            });
    }

    public Task<(bool succese, string result)> Extract(string fileName, string extractPath, string[] filers = default!)
    {
        var filersString = string.Empty;
        if (filers?.Any() ?? false)
        {
            var filerBuilder = new StringBuilder();
            foreach (var filer in filers)
            {
                filerBuilder.Append($@"--filter ""\.{filer}"" ");
            }

            filersString = filerBuilder.ToString();
        }

        var command = $"extract -i {Path.Combine(_root, fileName)} -o {extractPath} {filersString}";
        return RunCmd(command);
    }

    public Task<(bool scueeces, string result)> Pack(string inputFullFilePath, string outputFullFileName)
    {
        var command = $"pack -i {inputFullFilePath} -o {outputFullFileName}";
        return RunCmd(command);
    }

    public Task<(bool success, (string findedFile, string itFileName)[] result)> FindAll(string needFindFilePath)
    {

        var files = new DirectoryInfo(_root).GetFiles("*.it");

        List<Task<(string findedFile, string itFileName)>> tasksArr = new();


        var index = -1;
        var result = new List<(string findedFile, string findFileName)>();
        while (index < files.Length)
        {
            index = FindItFile(needFindFilePath, index, files, tasksArr);
            Task.WaitAll(tasksArr.ToArray());
            var enumerable = tasksArr.Where(x => !string.IsNullOrEmpty(x.Result.findedFile)).Select(x => x.Result);
            result.AddRange(enumerable);
            tasksArr.Clear();
        }

        return Task.FromResult((result.Count != 0, result.ToArray()));

    }

    public Task<(bool, (string findedFile, string findFileName))> FindAllPackageFirst(string needFindFilePath)
    {
        var files = new DirectoryInfo(_root).GetFiles("*.it");
        List<Task<(string findedFile, string itFileName)>> tasksArr = new();

        var index = -1;
        (string findedFile, string findFileName) result = default;
        while (index < files.Length)
        {
            index = FindItFile(needFindFilePath, index, files, tasksArr);

            Task.WaitAll(tasksArr.ToArray());

            result = tasksArr.FirstOrDefault(x => !string.IsNullOrEmpty(x.Result.findedFile))!.Result;

            tasksArr.Clear();
        }

        return Task.FromResult((result != default, result));


    }

    private int FindItFile(string needFindFilePath, int index, FileInfo[] files, List<Task<(string findedFile, string itFileName)>> tasksArr)
    {

        for (var i = 0; i < 5; i++)
        {
            index++;
            if (index >= files.Length - 1)
            {
                break;
            }

            var index1 = index;
            Task<(string findedFile, string itFileName)> task = Task<(string, string)>.Factory.StartNew(() =>
            {
                var fileInfo = files[index1];
                var split = List(fileInfo.FullName).Result.findFileReuslt.Split('\n');

                var filePath = split.FirstOrDefault(item => item.ToLowerInvariant().Contains(needFindFilePath.ToLowerInvariant())) ?? string.Empty;
                var itFileName = !string.IsNullOrEmpty(filePath) ? fileInfo.Name : string.Empty;
                return (filePath, itFileName);

            });
            tasksArr.Add(task);

        }

        return index;
    }
}