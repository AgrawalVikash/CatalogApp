using Catalog.Service.Utils;
using FluentAssertions;
using Xunit;

namespace Catalog.Tests.Utils
{
    public class DirectoryHelperTests
    {
        [Fact]
        public void EnsureDirectoryExists_Should_Create_Directory_If_Not_Exists()
        {
            string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestDir");
            string testFilePath = Path.Combine(testDirectory, "testfile.txt");

            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }

            DirectoryHelper.EnsureDirectoryExists(testFilePath);

            Directory.Exists(testDirectory).Should().BeTrue();

            Directory.Delete(testDirectory, true);
        }

        [Fact]
        public void EnsureDirectoryExists_Should_Not_Throw_When_Directory_Already_Exists()
        {
            string testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ExistingDir");
            string testFilePath = Path.Combine(testDirectory, "testfile.txt");

            Directory.CreateDirectory(testDirectory);

            Action act = () => DirectoryHelper.EnsureDirectoryExists(testFilePath);

            act.Should().NotThrow();

            Directory.Delete(testDirectory, true);
        }

        [Fact]
        public void EnsureDirectoryExists_Should_Not_Create_Directory_When_FilePath_Is_Null_Or_Empty()
        {
            Action act1 = () => DirectoryHelper.EnsureDirectoryExists(null);
            Action act2 = () => DirectoryHelper.EnsureDirectoryExists("");

            act1.Should().NotThrow();
            act2.Should().NotThrow();
        }

        [Fact]
        public void EnsureDirectoryExists_Should_Handle_Invalid_FilePath()
        {
            string invalidFilePath = "C:\\InvalidPath\\?InvalidFile.txt";

            Action act = () => DirectoryHelper.EnsureDirectoryExists(invalidFilePath);

            act.Should().NotThrow();
        }
    }
}
