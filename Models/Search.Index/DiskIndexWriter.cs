
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Search.Query;

namespace Search.Index
{
    /// <summary>
    /// Write index to the disk
    /// </summary>
    public class DiskIndexWriter
    {
        /// <summary>
        /// Writes the index on disk with 3 files
        /// </summary>
        /// <param name="index">the index to write</param>
        /// <param name="filePath">the absolute path to save the index files</param>
        public void WriteIndex(IIndex index, string filePath)
        {
            new BinaryWriter(File.Open("Vocab.bin", FileMode.Create));
            new BinaryWriter(File.Open("Postings.bin", FileMode.Create));
            IReadOnlyList<string> vocabulary = index.GetVocabulary();
            foreach (string s in vocabulary)
            {
                long vocabStart = WriteVocab(s);
                long postingStart = WritePostings(s);
                WriteVocabTable(s);
            }
        }

        /// <summary>
        /// Writes the vocab.bin file
        /// </summary>
        public long WriteVocab(string vocabulary)
        {
            long finalLong;
            using (BinaryWriter writer = new BinaryWriter(File.Open("Vocab.bin", FileMode.Append)))
            {
                finalLong=writer.BaseStream.Length;
                writer.Write(vocabulary);
            }
        return finalLong; 
        }
    }
}
