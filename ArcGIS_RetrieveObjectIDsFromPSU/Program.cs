using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArcGIS_RetrieveObjectIDsFromPSU
{
    class Program
    {
        //This program retrieves a list of PSU/Meshblock numbers with associated Qtr. It then scans a Query of desired feature service layer
        //from arcGIS to return any corresponding objectIDs for these PSUs for the given Qtr. The Object IDs are then stored, separated by a comma, in
        //a text file which can be input to the the "Delete Features" REST API in arcGIS online.

        //NOTE: The query must be retrieved with Qtr = XX as where clause to get the appropriate Qtr PSUs. For query, only return fields SamplingUnit, ObjectID, Qtr (for verification).
        //The query is found on arcGIS online in Home > services > NZCVS(FeatureServer) > NZCVS > query
        //Set return gemoetry to false when sending query.
        static void Main(string[] args)
        {
            string csvPath = @"C:\Users\Surveyor\Desktop\NZCVSremove.csv";
            string queryPath = @"C:\Users\Surveyor\Desktop\NZCVSqtr10query.txt";

            List<string[]> qtrPsuList = new List<string[]>();
            qtrPsuList = returnQtrPSUList(csvPath);

            List<string> psuObjIdList = new List<string>();
            psuObjIdList = returnPSUObjIDStringList(queryPath);
            int i = 0;

            string ObjectIDs = returnDesiredObjIds(qtrPsuList, psuObjIdList);
            string[] testarray = ObjectIDs.Split(',');
            File.WriteAllText(@"C:\Users\Surveyor\Desktop\objectIDs.txt", ObjectIDs);

            if (testarray.Distinct().Count() != testarray.Count())
            {
                Console.WriteLine("Duplicates found");
            }
            else
            {
                Console.WriteLine("No duplicates found");
            }

        }

        static string returnDesiredObjIds(List<string[]> qtrPsuList, List<string> psuObjIDList)
        {
            int count = 0;
            string ObjectIDs = null;
            foreach (string[] psuQtr in qtrPsuList)
            {
                string psu = psuQtr[1]; //index where PSU number string is located.
                foreach (string psuObj in psuObjIDList)
                {
                    if (psuObj.Contains(psu))
                    {
                        int psuIndex = psuObjIDList.IndexOf(psuObj);
                        string objIDtemp = psuObjIDList[psuIndex + 1];
                        Console.WriteLine(psu + " corresponds to object ID: " + objIDtemp);
                        objIDtemp = objIDtemp.Replace("ObjectId : ", "");
                        ObjectIDs = ObjectIDs + objIDtemp + ", ";
                        count = count + 1;
                    }
                }
            }

            Console.WriteLine("Total ObjectIDs found: " + count.ToString());

            return ObjectIDs;
        }

        //Takes the Qtr -> PSU csv as input and outputs the PSUs 
        static List<string[]> returnQtrPSUList(string csvPath)
        {
            List<string[]> csvLines = new List<string[]>();

            TextFieldParser CSVFile1 = new TextFieldParser(csvPath);
            CSVFile1.SetDelimiters(",");


            while (!CSVFile1.EndOfData)
            {
                csvLines.Add(CSVFile1.ReadFields());
            }

            return csvLines;

        }

        //Takes the query and stores as a list of strings
        static List<string> returnPSUObjIDStringList(string queryPath)
        {
            var logFile = File.ReadAllLines(queryPath);
            var logList = new List<string>(logFile);

            var psuObjIDlist = new List<string>();
            foreach (string entry in logList)
            {
    
                if(entry.Contains("\"SamplingUnit\" :"))
                {
                    string temp = entry;
                    temp = temp.Replace("\"", "");
                    temp = temp.Replace(",", "");
                    temp = temp.Trim();
                    psuObjIDlist.Add(temp);
                }
                if (entry.Contains("\"ObjectId\" :"))
                {
                    string temp = entry;
                    temp = temp.Replace("\"", "");
                    temp = temp.Replace(",", "");
                    temp = temp.Trim();
                    psuObjIDlist.Add(temp);
                }

            }

            return psuObjIDlist;

        }
    }
}
