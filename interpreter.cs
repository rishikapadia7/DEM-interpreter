using System;
using System.IO;

public class Program
{
    static void Main()
    {
        Console.WriteLine("Digital Elevation Model - Body of Water");

        //Store file path of input file
        Console.WriteLine("Enter path to csv file as path/to/filename.csv: ");
        string filepath = Console.ReadLine().ToString();

        Console.WriteLine("Filepath: " + filepath);
        //Throw an argument exception if the file does not exist.
        
        while (Path.GetExtension(filepath) != ".csv")
        {
            if (File.Exists(filepath))
            {
                Console.WriteLine("Not a CSV.  Please re-enter path to a valid CSV file: ");
                filepath = Console.ReadLine().ToString();
                
            }
            else
            {
                Console.WriteLine("File not found.  Please re-enter path to CSV file: ");
                filepath = Console.ReadLine().ToString();
            }
        }
        //Create an elevation object of the ElevationModel Class using input file
        ElevationModel elevObject = new ElevationModel(filepath);

        Console.WriteLine("     Rows:   {0}", elevObject.Rows);
        Console.WriteLine("  Columns:   {0}\n", elevObject.Columns);

        Console.WriteLine("Detected Outlier Cells");
        Console.WriteLine("  Elevation   DEM Grid Cell   Problem");

        //Attain all outlier objects and store in array of GridPoint class
        GridPoint[] allOutliers = elevObject.GetOutliers();

        //Output the erroneous values for all the outliers
        for (int i = 0; i < allOutliers.Length; i++)
        {
            //Store the row reference of current outlier in r
            int r = allOutliers[i].Row;
            //Store the column reference of current outlier in c
            int c = allOutliers[i].Column;

            //Store current outlier's elevation value in elevationValue
            double elevationValue = elevObject.Elevation(r, c);

            //String to store whether the outlier was caused by Fish or Bird
            string problem = "Fish";

            //If the outlier's elevation is below zero, then it must be Fish
            if (elevationValue <= 0)
            {
                problem = "Fish";
            }
            //if the outlier's elevation is above zero, then it must be Bird
            if (elevationValue > 0)
            {
                problem = "Bird";
            }

            //Outlier object is outputted as coordinate, in format specified by
            //the override ToString Method
            Console.WriteLine("  {0,7:F1} m   {1,-12}    {2,4} ",
                elevationValue, allOutliers[i], problem);
        }


        Console.WriteLine("\nCorrected Outlier Cells");
        Console.WriteLine("  Elevation   DEM Grid Cell");

        //Correct elevation of all the outliers
        for (int i = 0; i < allOutliers.Length; i++)
        {
            //Corrects the elevation of outlier to the average of surrounding
            elevObject.SmoothOutlier(allOutliers[i]);

            //Store the row reference of the corrected outlier in r
            int r = allOutliers[i].Row;
            //Store the column reference of the corrected outlier in c
            int c = allOutliers[i].Column;

            //Store current outlier's elevation value in elevationValue
            double elevationValue = elevObject.Elevation(r, c);
            Console.WriteLine("  {0,7:F1} m   {1,-12}", elevationValue, allOutliers[i]);

        }

        //Write a DEM file with the corrected elevations
        string correctedFilename = Path.GetFileNameWithoutExtension(filepath) + "-corrected.csv";
        elevObject.WriteDEM(correctedFilename);


        Console.WriteLine("\nWater Body Size Estimates");

        //Output the area in square km by calling the Area Method
        Console.WriteLine("    Area:   {0,5:F0} square km", elevObject.Area(0));
        //Output the volume in cubic km by calling the Volume Method
        Console.WriteLine("  Volume:   {0,5:F0} cubic km", elevObject.Volume(0));

    }

}

public class GridPoint
{
    //Declare the fields row and column for GridPoint Class
    readonly int row;
    readonly int column;

    //Constructor setting the fields to the parameter arguments
    public GridPoint(int row, int column)
    {
        this.row = row;
        this.column = column;
    }
    //Accessor that returns the row of the GridPoint object
    public int Row
    {
        get
        {
            return row;
        }
    }
    //Accessor that returns the column of the GridPoint object
    public int Column
    {
        get
        {
            return column;
        }

    }

    //Override which formats the GridPoint object as coordinate
    public override string ToString()
    {
        return string.Format("( {0,4}, {1,4} )", row, column);
    }

}

//Declare ElevationModel class which holds and manipulates the 
//elevation values
public class ElevationModel
{
    //Private rectangular array that stores all the elevation values
    //It indexes based on the CSV cells as [row,column]
    private double[,] elevations;

    //Constructor which causes the object to be created based
    //on reading the input file
    public ElevationModel(string inFileName)
    {
        //Call the ReadDEM method to read the input file
        ReadDEM(inFileName);

    }

    //Accessor to get the number of rows in the elevations array
    public int Rows
    {
        get
        {
            return elevations.GetLength(0);
        }
    }
    //Accessor to get the number of columns in the elevations array
    public int Columns
    {
        get
        {
            return elevations.GetLength(1);
        }
    }

    //This instance method returns the elevation at row, column specified
    public double Elevation(int row, int column)
    {
        return elevations[row, column];
    }

    //ReadDEM method takes an input file name as an argument and reads
    //the DEM file and stores its values in the rectangular elevations array.
    void ReadDEM(string inFileName)
    {
        //create StreamReader object making the input stream
        //the inFileName passed by the method
        StreamReader inputStream = new StreamReader(inFileName);

        //First line indicates the number of rows for the elevations array
        int totalrows = int.Parse(inputStream.ReadLine());
        //Second line indicates number of columns for the elevations array
        int totalcolumns = int.Parse(inputStream.ReadLine());

        //Use the cells array to store the elevation values of an entire row
        //for a given row
        string[] cells = new string[totalcolumns];

        //Declare an instance of the elevations array
        elevations = new double[totalrows, totalcolumns];

        //These nested for loops take every row of the CSV file into string
        //and splits it by a comma to the cells array which will contain all
        //the elevation values for a given row...
        for (int r = 0; r < totalrows; r++)
        {
            string currentline = inputStream.ReadLine();
            cells = currentline.Split(',');

            //...and then the elevations array will store all the values
            //in the cells array at the correct row,column indexes
            for (int c = 0; c < totalcolumns; c++)
            {
                elevations[r, c] = double.Parse(cells[c]);

            }

        }
        //Close the file input stream
        inputStream.Close();
    }

    //This method writes the values stored in the Elevations array to a
    //CSV file identical in format to the origianl DEM file.  
    public void WriteDEM(string outFileName)
    {
        //Create a StreamWriter object using the filename specified by
        //the argument passed through the paramater of the method
        StreamWriter outStream = new StreamWriter(outFileName);
        //Write the total number of rows to first line of file
        outStream.WriteLine(Rows);
        //Write total number of columns to the second line of file
        outStream.WriteLine(Columns);

        //Repeat for the all the rows stored in the elevations array
        for (int r = 0; r < Rows; r++)
        {
            //Repeat for all the columns stored in the elevations array
            for (int c = 0; c < Columns; c++)
            {
                //The last column should not have comma and space following 
                //the elevations value
                if (c == (Columns - 1))
                {
                    outStream.Write("{0,7:F1}", elevations[r, c]);
                }
                //If it is not last column, have a comma and space after the
                //elevations value
                else
                {
                    outStream.Write("{0,7:F1}, ", elevations[r, c]);
                }
            }
            //After the row has ended have a line break in the file
            outStream.Write("\n");
        }
        //Close the file output stream
        outStream.Close();
    }

    //This method compares all the elevation values to its surrouding 8
    //and if the absolute difference between the average of the 8 and and
    //the elevation value is greater than 100, then that point is considered
    //an outlier which is stored in the outlier array
    public GridPoint[] GetOutliers()
    {
        //set the outlier counter to zero
        int outlierCounter = 0;

        //Execute for all rows except first and last
        for (int r = 1; r < (Rows - 1); r++)
        {
            //Excecute for all columns except first and last
            for (int c = 1; c < (Columns - 1); c++)
            {
                //Retrieve 8 elevations surrounding the 
                //elevation of concern
                double s1 = elevations[r - 1, c - 1];
                double s2 = elevations[r, c - 1];
                double s3 = elevations[r + 1, c - 1];
                double s4 = elevations[r - 1, c];
                double s5 = elevations[r + 1, c];
                double s6 = elevations[r - 1, c + 1];
                double s7 = elevations[r, c + 1];
                double s8 = elevations[r + 1, c + 1];

                //Find the average of those 8 elevations
                double eightAverage = (s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 8;

                //increase the outlier counter if absolute difference between the elevation
                //of concern and the 8 surrouding it is greater than 100, then consider it
                //an outlier and increasing the outlier counter by 1
                if (Math.Abs(elevations[r, c] - eightAverage) > 100.0)
                {
                    outlierCounter = outlierCounter + 1;

                }
            }
        }

        //Make an outlier object array of the GridPoint class with
        //an array length of the number of outliers
        GridPoint[] outlier = new GridPoint[outlierCounter];

        //Set the outlier counter back to zero
        outlierCounter = 0;

        //Execute for all rows except first and last
        for (int r = 1; r < (Rows - 1); r++)
        {
            //Excecute for all columns except first and last
            for (int c = 1; c < (Columns - 1); c++)
            {
                //Retrieve 8 elevations surrounding the 
                //elevation of concern
                double s1 = elevations[r - 1, c - 1];
                double s2 = elevations[r, c - 1];
                double s3 = elevations[r + 1, c - 1];
                double s4 = elevations[r - 1, c];
                double s5 = elevations[r + 1, c];
                double s6 = elevations[r - 1, c + 1];
                double s7 = elevations[r, c + 1];
                double s8 = elevations[r + 1, c + 1];

                //Calculate average of the 8 elevations
                double eightAverage = (s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 8;


                if (Math.Abs(elevations[r, c] - eightAverage) > 100.0)
                {
                    //At the index of the outlierCounter, create a new
                    //GridPoint object using the outlier's row and column
                    outlier[outlierCounter] = new GridPoint(r, c);
                    //increase the outlierCounter by 1
                    outlierCounter = outlierCounter + 1;


                }
            }
        }
        //This method should return the outlier object array
        return outlier;
    }


    //This method retrieves the outlier's indexes in the
    //elevations array and then smooths or corrects the 
    //outlier by changing its elevation value to the average
    //of the eight elevations surrounding the outlier
    public void SmoothOutlier(GridPoint gridPoint)
    {
        //Retreive the row and column for the outlier
        int r = gridPoint.Row;
        int c = gridPoint.Column;

        //Retrieve the elevations of the surround 8 elevations
        //and find its average
        double s1 = elevations[r - 1, c - 1];
        double s2 = elevations[r, c - 1];
        double s3 = elevations[r + 1, c - 1];
        double s4 = elevations[r - 1, c];
        double s5 = elevations[r + 1, c];
        double s6 = elevations[r - 1, c + 1];
        double s7 = elevations[r, c + 1];
        double s8 = elevations[r + 1, c + 1];

        double eightAverage = (s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 8;

        //Correct the outlier by overwriting the value stored in the
        //elevations array with the average of the 8 surrouding elevations
        elevations[r, c] = eightAverage;
    }

    //This method returns the area of Water Body which is at or below limit
    //specified by the argument of the parameter.  This area is represented by
    //cells and they can be counted and combined to determine the total area
    public double Area(double limit)
    {
        //Use this counter to see how many cells are at or below the limit specified
        int areaCells = 0;

        //Repeat for all rows
        for (int r = 0; r < Rows; r++)
        {
            //Repeat for all columns
            for (int c = 0; c < Columns; c++)
            {
                //if the elevation is less than the limit
                //then increase the areaCells counter by 1
                if (elevations[r, c] <= limit)
                {
                    areaCells = areaCells + 1;
                }
            }
        }
        //return the area in square km by converting the sum of the 
        //individual 0.1km by 0.1km cells to represents the total area
        return areaCells * 0.01;
    }

    //This method returns the volume of the Water Body which is at or below limit
    //specified by the argument of the parameter.  This volume is represented by
    //0.1km by 0.1km by the elevation and the volume can be summed to determine 
    //the total volume
    public double Volume(double limit)
    {
        //Set the volume initially to zero
        double volumeCells = 0;

        //Repeat for all rows
        for (int r = 0; r < Rows; r++)
        {
            //Repeat for all columns
            for (int c = 0; c < Columns; c++)
            {
                //If the elevation is less than the limit specified,
                //then that volume will be added to the volume holder.
                //The amount is stored in cubic km.
                if (elevations[r, c] <= limit)
                {
                    volumeCells += 0.1 * 0.1 * elevations[r, c] / 1000;
                }
            }
        }
        //The method returns the the total volume in cubic km.
        return Math.Abs(volumeCells);
    }

}