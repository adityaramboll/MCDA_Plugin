using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;
using Numpy.Models;
using static Rhino.Render.TextureGraphInfo;
using Axis = Numpy.Models.Axis;

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

        public NDarray CalculateNormalizedmatrix(NDarray decisionmatrix)
        {
            Axis axis = 0;
            var factorialdm = decisionmatrix.sum(axis);
            var reshapedfactorial = np.array(factorialdm).reshape(1, -1);
            var normalisedmatrix = decisionmatrix / reshapedfactorial;

            return normalisedmatrix;
        }

        public NDarray CalculateWeightedNormalizedmatrix(NDarray weights, NDarray normalizedmatrix)
        {
            var reshapeweights = np.array(weights).reshape(1, -1);
            var wegihtedmatrix = normalizedmatrix * reshapeweights;

            return wegihtedmatrix;
        }

        public NDarray Calculateidealbest (NDarray Weightednormalizedmatrix)
        {
            var axislist = new int [1];
            Axis axis = 1;
            // Calculate the maximum value in each column
            var max_values = np.max(Weightednormalizedmatrix, axislist);
            var reshapemaxvalues = np.array(max_values).reshape(1, -1);
            var Idealbestlistsquared = np.square(Weightednormalizedmatrix - reshapemaxvalues);
            var idealbestlist = Idealbestlistsquared.sum(axis);
            var idealbestvaluelist = idealbestlist.sqrt();

            return idealbestvaluelist;
        }
        public NDarray Calculateidealworst(NDarray Weightednormalizedmatrix)
        {
            var axislist = new int[1];
            Axis axis = 1;
            // Calculate the maximum value in each column
            var min_values = Weightednormalizedmatrix.min(axislist);
            var reshapeminvalues = np.array(min_values).reshape(1, -1);
            var Idealworstlistsquared = np.square(Weightednormalizedmatrix - reshapeminvalues);
            var idealworstlist = Idealworstlistsquared.sum(axis);
            var idealworstvaluelist = idealworstlist.sqrt();

            return idealworstvaluelist;
        }

        public NDarray Calculateperformancescore(NDarray idealbest, NDarray idealworst)
        {
            var performancescore = idealworst/(idealbest + idealworst);
            return performancescore;
        }
        public List<string> CalculateRankings(NDarray performancescore, List<string> designoptions)
        {
            var sorted_ranks = np.flip(performancescore.sort());
            var sorted_rank_list = sorted_ranks.GetData<double>();

            List<string> RankList = new List<string>();

            var Design_options = np.array(designoptions.ToArray());
            var sorted_indices = np.argsort(performancescore);
            var sorted_strings = np.flip(Design_options[sorted_indices]);

            // Calculate ranks and create a list of strings with ranks
            for (int i = 0; i < sorted_rank_list.Length; i++)
            {
                int rank = i + 1;
                RankList.Add($"Rank {rank}: {sorted_rank_list[i]} : {sorted_strings[i]}");
            }

            return RankList;
        }
    }
}
