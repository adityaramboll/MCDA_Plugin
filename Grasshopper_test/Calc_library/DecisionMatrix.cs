using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;

namespace DeciGenArch.Calc_library
{
    public class TreeToArrayConverter
    {
                public double[,] ConvertTreeToArray(GH_Structure<GH_Number> inputTree)
                {
                    int rowCount = inputTree.PathCount;
                    int columnCount = inputTree.Branches[0].Count;

                    double[][] array = new double[rowCount][];
                    for (int i = 0; i < rowCount; i++)
                    {
                        array[i] = new double[columnCount];
                        for (int j = 0; j < columnCount; j++)
                        {
                            array[i][j] = inputTree.get_DataItem(new GH_Path(i), j).Value;
                        }
                    }
                    int numRows = array.Length;
                    int numCols = array[0].Length;
                    double[,] multidimensionalArray = new double[numRows, numCols];

                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            multidimensionalArray[i, j] = array[i][j];
                        }
                    }


                     return multidimensionalArray;
                }
     
        
    }
}
