using System;


namespace Calc_library
{
    public class topsis_calc
    {
        public class TreeToArrayConverter
        {
            public double[][] ConvertTreeToArray(GH_Structure<GH_Number> inputTree)
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

                return array;
            }
        }
    }
}
