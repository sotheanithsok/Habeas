using System.IO;
using Xunit;
using FluentAssertions;
using Search.Index;
using System.Collections.Generic;
using Search.Document;

namespace UnitTests
{
    public class DiskIndexWriterTests
    {
        string dirPath = "../../../Models/UnitTests/testCorpus3/index/";

        [Fact]
        public void BinaryWriterTest()
        {
            //Just testing the BinaryWriter
            Directory.CreateDirectory(dirPath);
            string filePath = dirPath + "test.bin";

            File.Create(filePath).Dispose();
            BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Append));
            writer.Write(16);
            writer.Write(40);

            long length = writer.BaseStream.Length;
            length.Should().Be( 2 * 4 );  // two 4-byte integers
            
            writer.Dispose();
        }

        [Fact]
        public void WritePostingTest()
        {
            Directory.CreateDirectory(dirPath);
            //Arrange
            string filePath = dirPath + "postings.bin";
            File.Create(filePath).Dispose();
            BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Append));
            DiskIndexWriter indexWriter = new DiskIndexWriter();
            IList<Posting> postings;
            long startByte;
            long actualLength;
            long expectedLength;

            //Act
            postings = UnitTest.GeneratePostings("(10,[16,17]), (32,[20,26])");
            //with gaps (10 [16, 1]), (22 [20, 6])  --> 0A 10 01 16 14 06 (hex)
            startByte = indexWriter.WritePostings(postings, writer);
            actualLength = writer.BaseStream.Length;
            //Assert
            actualLength.Should().Be( 6 * 4 );  // six 4-byte integers
            startByte.Should().Be(0);

            //Act2
            postings = UnitTest.GeneratePostings("(7,[160,161])");
            //with gaps (7 [160, 1])  --> 07 A0 01 (hex)
            startByte = indexWriter.WritePostings(postings, writer);
            actualLength = writer.BaseStream.Length;
            //Assert2
            actualLength.Should().Be( 9 * 4 );  // nine 4-byte integers so far
            startByte.Should().Be(24);  // where the first docID of this postings starts

            writer.Dispose();
        }

        [Fact]
        public void WriteVocabTableTest()
        {
            Directory.CreateDirectory(dirPath);
            //Arrange
            string filePath = dirPath + "vocabTable.bin";
            File.Create(filePath).Dispose();
            BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Append));
            DiskIndexWriter indexWriter = new DiskIndexWriter();

            //Act
            indexWriter.WriteVocabTable(4, 160, writer);    //04 A0
            indexWriter.WriteVocabTable(8, 169, writer);    //08 A9
            indexWriter.WriteVocabTable(12, 176, writer);   //0C B0
            
            //Assert
            long length = writer.BaseStream.Length;
            length.Should().Be(6 * 8);  // six 8-byte integers
            
            writer.Dispose();
        }

        [Fact]
        public void WriteIndexTest()
        {
            //Arrange
            string corpusDir = "../../../Models/UnitTests/testCorpus3";
            IDocumentCorpus corpus = DirectoryCorpus.LoadTextDirectory(corpusDir);
            PositionalInvertedIndex index = Indexer.IndexCorpus(corpus);
            
            //Act
            DiskIndexWriter indexWriter = new DiskIndexWriter();
            indexWriter.WriteIndex(index, corpusDir+"/index/");

            //Assert
            //???
        }

    }
}