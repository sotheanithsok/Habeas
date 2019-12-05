using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System;
using System.Linq;

namespace Metrics.MeanAveragePrecision
{
    public class PrecisionTest
    {
        private static string corpusPath = "../../../corpus/Cranfield/relevance/";
        private static string queryFilePath = corpusPath + "Actualqueries";
        private static string qrelFilePath = corpusPath + "qrel";


        [Fact]
        public void TestSearchQuery()
        {
            List<string> queries = ReadStringList(queryFilePath);
            List<List<int>> relevances = ReadIntList(qrelFilePath);

            
            // for(int i=0; i<queries.Count; i++)
            // {
            //     string query = queries[i];
            //     List<int> qrel = relevances[i];
                
            //     //Get the query result from the program
            //     List<string> results = SearchQuery(query);
            //     List<int> numbers = processResults(results);

            //     //calculate average precision
            //     float precision = GetAveragePrecision(numbers, qrel);
            // }
            
        }


        [Fact]
        public void TestGetMAP()
        {
            List<List<int>> result = new List<List<int>>{
                new List<int>{10},
                new List<int>{10},
                new List<int>{10},
                new List<int>{10},
            };
            List<List<int>> actual = new List<List<int>>{
                new List<int>{10},
                new List<int>{10},
                new List<int>{10,20},
                new List<int>{10,20},
            };

            float map = GetMAP(result, actual);
            map.Should().Be( 0.75F );
        }

        /// <summary>
        /// Calculates MeanAveragePrecision of the query results.
        /// </summary>
        /// <param name="results">list of all query results</param>
        /// <param name="actuals">list of actual relevances for all queries</param>
        /// <returns></returns>
        public float GetMAP(List<List<int>> results, List<List<int>> actuals)
        {
            float sumaps = 0;
            for(int i=0; i<results.Count; i++)
            {
                sumaps += GetAveragePrecision(results[i], actuals[i]);
            }
            return sumaps / results.Count;
        }

        [Fact]
        public void TestGetAP()
        {
            List<int> result = new List<int>{1,2,33,4,55,66,77,8};
            List<int> actual = new List<int>{1,2,3,4,5,6,7,8};

            float ap = GetAveragePrecision(result, actual);
            ap.Should().Be( 13/32 );
        }

        /// <summary>
        /// Calculates MeanAveragePrecision with the query result and actual relevance
        /// </summary>
        /// <param name="result"></param>
        /// <param name="actual"></param>
        public float GetAveragePrecision(List<int> result, List<int> actual)
        {
            int totalRelevant = 0;
            List<float> pks = new List<float>();
            float sumpks = 0;

            for(int i=0; i<result.Count; i++)
            {
                if( actual.Contains(result[i]) ) {
                    totalRelevant++;
                    sumpks += totalRelevant/(i+1);
                }
            }
            return sumpks / actual.Count;
        }

        [Fact]
        public void TestReads()
        {
            List<string> queries = ReadStringList("C:\\Users\\Lenovo\\Desktop\\Computer Science\\CECS 529\\Cranfield\\relevance\\Actualqueries");
            // List<string> queryResult = BackendQuery(queries[1]);
            queries.Count.Should().Be(225, "That's how many queries there are!");
            queries[0].Should().Be("similarity law aeroelastic model high speed aircraft", "That's the first query .");
            queries[224].Should().Be("what design factors can be used to control lift-drag ratios at mach numbers above 5", "That's the last query");
        }

        public List<string> ReadStringList(string fileName)
        {
            int counter = 0;
            string line;


            System.IO.StreamReader queryFile =new System.IO.StreamReader(fileName);
            List<String> s = new List<string>(); 
            while ((line = queryFile.ReadLine()) != null)
            {
                counter++;
                s.Add(line);
            }

            queryFile.Close();
            System.Console.WriteLine("There were {0} lines.", counter);
            return s;
        }

        public List<List<int>> ReadIntList(string FileName)
        {
            int counter = 0;
            string line;
            List<List<int>> listOfRelevanceResults = new List<List<int>>();

            System.IO.StreamReader queryFile = new System.IO.StreamReader(FileName);
            List<int> i = new List<int>();
            while ((line = queryFile.ReadLine()) != null)
            {
                List<int> relevanceJudgements = line.Split(' ').Select(Int32.Parse).ToList();
                listOfRelevanceResults.Add(relevanceJudgements);
            }

            queryFile.Close();
            System.Console.WriteLine("There were {0} lines.", counter);
            return listOfRelevanceResults;
        }

    }
}