using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVWriter : MonoBehaviour
{
    //public Driving DrivingAI;
    public string filename = "";

    public struct DataToWrite
    {
        public int generation;
        public float averageFitness;
        public float bestFitness;
        public int goodCars;
        public float bestLap;
    }

    public List<DataToWrite> AllData;

    // Start is called before the first frame update
    void Start()
    {
        filename = Application.dataPath + "/" + filename + ".csv";
        Debug.Log(filename);
        AllData = new List<DataToWrite>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WriteCSV()
    {
        if(AllData.Count > 0)
        {
            TextWriter textWriter = new StreamWriter(filename, false);
            textWriter.WriteLine("Generation; Avg Fitness; Best Fitness; Cars Successful; Best Lap Time");
            textWriter.Close();

            textWriter = new StreamWriter(filename, true);
            foreach(DataToWrite d in AllData)
            {
                textWriter.WriteLine(d.generation + ";" + d.averageFitness + ";" + d.bestFitness + ";" + d.goodCars +  ";" + d.bestLap); 
            }
            textWriter.Close();
        }
        else
        {
            Debug.Log("No Data");
        }
    }


    
}
