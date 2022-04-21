using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mabinogi_Packer;

[TestFixture]
public class Test
{
    private static readonly string gamePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test", "gamePath");
    private static readonly string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test", "temp");
    private static readonly string _sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test", "source");
    private static readonly string packOutput = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test", "pack");

    private static readonly string comparerTxt = "0123456789";

    [SetUp]
    public async Task Setup()
    {
        Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test"), true);
        Directory.CreateDirectory(gamePath);
        Directory.CreateDirectory(tempPath);
        Directory.CreateDirectory(_sourcePath);
        Directory.CreateDirectory(packOutput);

        var sourceFile = Path.Combine(_sourcePath, "000.txt");
        await File.WriteAllTextAsync(sourceFile, comparerTxt);
    }

    [Test]
    public async Task TestPack_ShouldBe_Ture()
    {
        var outputFullFileName = Path.Combine(packOutput, "000.it");
        var pack = await new Packer(gamePath).Pack(_sourcePath, outputFullFileName);
        Assert.True(pack.Item1);
        Assert.True(string.IsNullOrEmpty(pack.Item2));
    }

    [Test]
    public void TestPack_When_InputPathError_ShouldBe_Throw()
    {

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {

            var outputFullFileName = Path.Combine(packOutput, "000.it");
            var sourceFile = Path.Combine(_sourcePath);

            await File.WriteAllTextAsync(sourceFile, comparerTxt);

            var pack = await new Packer(gamePath).Pack(_sourcePath, outputFullFileName);
            Assert.False(pack.Item1);
            Assert.False(string.IsNullOrEmpty(pack.Item2));
        });

    }

    [Test]
    public void TestPack_When_OutPathError_ShouldBe_Throw()
    {

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {

            var outputFullFileName = Path.Combine(packOutput);
            var sourceFile = Path.Combine(_sourcePath);

            await File.WriteAllTextAsync(sourceFile, comparerTxt);

            var pack = await new Packer(gamePath).Pack(_sourcePath, outputFullFileName);
            Assert.False(pack.Item1);
            Assert.False(string.IsNullOrEmpty(pack.Item2));
        });

    }

    [Test]
    public async Task Test_Extract_ShouldBe_True()
    {
        await TestPack_ShouldBe_Ture();
        var outputFullFileName = Path.Combine(packOutput, "000.it");
        var extract = await new Packer(gamePath).Extract(outputFullFileName, tempPath);
        Assert.True(extract.Item1);
        Assert.True(string.IsNullOrEmpty(extract.Item2));
    }

    [Test]
    public async Task Test_List_ShouldBe_True()
    {
        await TestPack_ShouldBe_Ture();
        var outputFullFileName = Path.Combine(packOutput, "000.it");
        var lis = await new Packer(gamePath).List(outputFullFileName);
        Assert.True(lis.succese);
        Assert.True(lis.findFileReuslt.Trim() == "000.txt");
    }

    [Test]
    public async Task Comparer()
    {
        await TestPack_ShouldBe_Ture();
        await Test_Extract_ShouldBe_True();
        var sourceFiles = new DirectoryInfo(_sourcePath).GetFiles();
        var tempFiles = new DirectoryInfo(tempPath).GetFiles();
        Assert.True(sourceFiles.Length == tempFiles.Length);

        foreach (var sourceFile in sourceFiles)
        {
            var fileInfo = tempFiles.First(x => x.Name == sourceFile.Name);
            var result = CompareByReadOnlySpan(fileInfo.FullName, sourceFile.FullName);
            Assert.True(result);
        }
    }

    private static bool CompareByReadOnlySpan(string file1, string file2)
    {
        const int BYTES_TO_READ = 1024 * 10;

        using FileStream fs1 = File.Open(file1, FileMode.Open);
        using FileStream fs2 = File.Open(file2, FileMode.Open);
        var one = new byte[BYTES_TO_READ];
        var two = new byte[BYTES_TO_READ];
        while (true)
        {
            var len1 = fs1.Read(one, 0, BYTES_TO_READ);
            var len2 = fs2.Read(two, 0, BYTES_TO_READ);
            // 位元組陣列可直接轉換為ReadOnlySpan
            if (!((ReadOnlySpan<byte>) one).SequenceEqual((ReadOnlySpan<byte>) two)) return false;
            if (len1 == 0 || len2 == 0) break; // 兩個檔案都讀取到了末尾,退出while迴圈
        }

        return true;
    }
}