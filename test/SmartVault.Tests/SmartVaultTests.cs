using SmartVault.Tests;
using SmartVaultProgram = SmartVault.Program.Program;

namespace SmartVault.Test
{
    public class SmartVaultTests : IClassFixture<SmartVaultFixture>
    {
        private readonly SmartVaultFixture _fixture;

        public SmartVaultTests(SmartVaultFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void WriteEveryThirdFileToFile()
        {
            SmartVaultProgram.WriteEveryThirdFileToFile(SmartVaultFixture.ACCOUNT_ID, _fixture.Connection);

            var expectedFile = Path.Combine(SmartVaultFixture.OUTPUT_DIRECTORY, $"Account_{SmartVaultFixture.ACCOUNT_ID}_ThirdSmithProperties.txt");
            Assert.True(File.Exists(expectedFile));
            var outputContent = File.ReadAllText(expectedFile);
            Assert.Contains(SmartVaultFixture.SMITH_PROPERTY_CONTENT, outputContent);
            Assert.DoesNotContain(SmartVaultFixture.NOT_MATCH_CONTENT, outputContent);
        }
        
        [Fact]
        public void ReturnCorrectTotalSize()
        {
            var totalSize = SmartVaultProgram.GetAllFileSizes(_fixture.Connection);

            long expectedTotalSize = GetFileLength("file3.txt") + GetFileLength("file6.txt");
            Assert.Equal(expectedTotalSize, totalSize);
        }

        private static long GetFileLength(string filename)
        {
            return new FileInfo(Path.Combine(SmartVaultFixture.TESTS_DIRECTORY, filename)).Length;
        }
    }
}