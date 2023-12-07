using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FileGenerator : MonoBehaviour
{
    void Start()
    {
        GenerateMapFile();
    }

    void Update()
    {
        
    }

    
    public void GenerateMapFile()
    {
        string filePath = "testFile.txt";
        StreamWriter writer = new StreamWriter(filePath);

        writer.WriteLine("Haoi!");

        writer.Close();
    
    }
}
