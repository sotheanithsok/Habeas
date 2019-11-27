using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Search.Text;
using Search.Document;
using Search.Query;

namespace Search.Index
{
    public class RankingVariant
    {
        //w_{q,t}
        private double query2termWeight;
        //w_{d,t}
        private double doc2termWeight;

        //temporarily stores the document id with its corresponding rank  [docId -> A_{docID}]
        private Dictionary<int, double> accumulator;

        //used to calculate the queryToTermWeight
        private int corpusSize;


        //saves instance of the corpus to access corpus path
        private IDocumentCorpus corpus;

        private List<int> documentIds;


        IIndex index;

        IRankVariant rankType;

        public RankingVariant(IDocumentCorpus corpus, IIndex index, string RankedRetrievalMode)
        {
            query2termWeight = new int();
            doc2termWeight = new int();
            accumulator = new Dictionary<int, double>();
            documentIds = new List<int>();

            this.rankType = SetRankedRetrievalMode(RankedRetrievalMode);
            this.index = index;
            this.corpus = corpus;

            string path = Indexer.path;
            this.corpusSize = this.GetCorpusSize(path);
        }

        /// <summary>
        /// Count the number of documents from index
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private int GetCorpusSize(string path)
        {
            return corpus.CorpusSize;
        }


        public IRankVariant SetRankedRetrievalMode(string RankedRetrievalMode)
        {
            switch (RankedRetrievalMode)
            {
                case "Tf-idf":
                    return new Tf_Idf();
                case "Okapi":
                    return new Okapi();

                case "Wacky":
                    return new Wacky();
                default:
                    return new Default();
            }

        }

        /// <summary>
        /// Method that takes in the query and returns a list of the top ten ranking documents
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IList<MaxPriorityQueue.InvertedIndex> GetTopTen(List<string> query)
        {
            
            //Build the Accumulator Hashmap
            BuildAccumulator(query);

            //Build Priority Queue using the Accumulator divided by L_{d}  
            MaxPriorityQueue priorityQueue = BuildPriorityQueue();

            accumulator.Clear();

            //Retrieve Top Ten Documents according to percent
            return priorityQueue.RetrieveTopTen();

        }


        /// <summary>
        /// Builds the Accumulator hashmap for the query to retrieve top 10 documents
        /// </summary>
        /// <param name="query"></param>
        private void BuildAccumulator(List<string> query)
        {

            //stores temporary Accumulator value that will be added to the accumulator hashmap
            double docAccumulator;


            //caculate accumulated Value for each relevant document A_{d}
            foreach (string term in query)
            {
                //posting list of a term grabbed from the On Disk file
                IList<Posting> postings = index.GetPostings(term);


                if (postings != default(List<Posting>))
                {
                    int docFrequency = postings.Count;

                    //implements formula for w_{q,t}
                    this.query2termWeight = this.rankType.calculateQuery2TermWeight(docFrequency , this.corpusSize);

                    foreach (Posting post in postings)
                    {
                        int termFrequency = post.Positions.Count;


                        //implements formula for w_{d,t}
                        this.doc2termWeight = this.rankType.calculateDoc2TermWeight(termFrequency, post.DocumentId, this.corpusSize, this.index);

                        //the A_{d} value for a specific term in that document
                        docAccumulator = this.query2termWeight * this.doc2termWeight;

                        //if the A_{d} value exists on the hashmap increase its value else create a new key-value pair
                        if (accumulator.ContainsKey(post.DocumentId))
                        {
                            this.accumulator[post.DocumentId] += docAccumulator;
                        }
                        else
                        {
                            this.accumulator.Add(post.DocumentId, docAccumulator);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new priority queue by inserting the rank of the document and document id 
        /// </summary>
        /// <returns> a priority queue with max heap property</returns>
        private MaxPriorityQueue BuildPriorityQueue()
        {

            //temporary variable to hold the doc weight
            double normalizer;
            //temporary variable to hold the final ranking value of that document
            double finalRank;

            //Make a new priority queue
            MaxPriorityQueue priorityQueue = new MaxPriorityQueue();

            //for every key value in the Accumulator divide A_{d} by L_{d}
            foreach (KeyValuePair<int, double> candidate in this.accumulator)
            {
                //get corresponding L_{d} value according to ranking system
                normalizer = this.rankType.GetDocumentWeight(candidate.Key, this.index);

                // divide Accumulated Value A_{d} by L_{d} 
                finalRank = (double)candidate.Value / normalizer;

                //add to list to perform priority queue on 
                priorityQueue.MaxHeapInsert(finalRank, candidate.Key);
            }

            return priorityQueue;

        }
    }



    public class Default : IRankVariant
    {
        public double calculateQuery2TermWeight(int docFrequency, int corpusSize)
        {
            return Math.Log(1 + (double)corpusSize / docFrequency);
        }

        public double calculateDoc2TermWeight(int termFrequency, int docID, int corpusSize, IIndex index)
        {
            return (double)(1 + Math.Log(termFrequency));
        }

        public double GetDocumentWeight(int docID, IIndex index)
        {
            DiskPositionalIndex.PostingDocWeight temp = index.GetPostingDocWeight(docID);
            return temp.GetDocWeight();
        }

    }


    public class Tf_Idf : IRankVariant
    {
        public double calculateQuery2TermWeight(int docFrequency, int corpusSize)
        {
            return Math.Log((double)corpusSize / docFrequency);
        }
        public double calculateDoc2TermWeight(int termFrequency, int docID, int corpusSize, IIndex index)
        {
            return termFrequency;
        }


        public double GetDocumentWeight(int docID, IIndex index)
        {
            double docWeight;
            DiskPositionalIndex.PostingDocWeight temp = index.GetPostingDocWeight(docID);
            docWeight = temp.GetDocWeight();
            return docWeight;

        }
    }

    public class Okapi : IRankVariant
    {
        public double calculateQuery2TermWeight(int docFrequency, int corpusSize)
        {
            double OkapiWqtValue = Math.Log((double)(corpusSize - docFrequency + 0.5) / (docFrequency + 0.5));
            if (0.1 > OkapiWqtValue)
            {
                return 0.1;
            }
            else
                return OkapiWqtValue;
        }
        public double calculateDoc2TermWeight(int termFrequency, int docID, int corpusSize, IIndex index)
        {

            DiskPositionalIndex.PostingDocWeight temp = index.GetPostingDocWeight(docID);
            int documentLength = temp.GetDocTokenCount();
            double numeratorO = 2.2 * termFrequency;
            double denominatorO = 1.2 * (0.25 + 0.75 * (double)(documentLength / Indexer.averageDocLength)) + termFrequency;
            double OkapiWdtValue = (double)numeratorO / denominatorO;
            return OkapiWdtValue;
        }


        public double GetDocumentWeight(int docID, IIndex index)
        {
            return 1.0;
        }

    }

    public class Wacky : IRankVariant
    {
        public double calculateQuery2TermWeight(int docFrequency, int corpusSize)
        {
            int numerator = corpusSize - docFrequency;

            double division = (double)numerator / docFrequency;
            if (division > 1)
            {
                double WackyWqtValue = Math.Log(division);
                return WackyWqtValue;

            }
            else
                return 0.0;
        }

        public double calculateDoc2TermWeight(int termFrequency, int docID, int corpusSize, IIndex index)
        {

            DiskPositionalIndex.PostingDocWeight temp = index.GetPostingDocWeight(docID);

            double avDocTermFreq = temp.GetDocAveTermFreq();
            double numeratorW = (double)1 + Math.Log(termFrequency);
            double denominatorW = (double)1 + Math.Log(avDocTermFreq);
            double WackyWdtValue = (double)numeratorW / denominatorW;
            return WackyWdtValue;
        }


        public double GetDocumentWeight(int docID, IIndex index)
        {
            DiskPositionalIndex.PostingDocWeight temp = index.GetPostingDocWeight(docID);
            int fileSizeInByte = temp.GetDocByteSize();
            double WackyLd = (double)(Math.Sqrt(fileSizeInByte));
            Console.WriteLine(WackyLd);
            return WackyLd;

        }
    }

}

