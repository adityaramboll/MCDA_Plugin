using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Numpy;
using DeciGenArch.Calc_library ;
using Rhino.FileIO;
using System.Runtime.InteropServices;
using Numpy.Models;
using System.Drawing;
using DeciGenArch.Properties;

namespace MCDA
{
    public class WeightedProduct : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public WeightedProduct()
          : base("WeightedProduct", "WP",
              "Performs Multi-Criteria Decision analysis process on a decision matrix using the WeightedProduct method ",
              "DECIGEN", "WeightedProduct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("DesignOptions", "Opt", "Add names to the design options", GH_ParamAccess.list);
            pManager.AddNumberParameter("inputMatrix", "IM", "Add a decision matrix to evaluate", GH_ParamAccess.tree);
            pManager.AddTextParameter("Criteria", "Crit", "Add the names of various criteria used for decision making ", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Weights", "Wei", "Add the desired weights for each criteria as a list (0-10)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Rankings", "Rank", "Result of TOPSIS ranking process", GH_ParamAccess.list);
            pManager.AddNumberParameter("Decision Matrix", "DM", "Decision matrix with values for various criteria for design options", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weighted Decision Matrix", "WDM", "Scaled decision matrix according to the weights", GH_ParamAccess.list);
            pManager.AddNumberParameter("Performance", "Per", "Indicates the final aggregation value for each design option", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Inputs empties initialized
            List<string> inpCriteria = new List<string>();
            List<string> Designoptions = new List<string>();
            List<int> inpWeights = new List<int>();
            int numberOfPaths = 0;
            int numberofitems = 0;
            List<int> pathLengths = new List<int>();

            //Inputs  initialized using actual data
            DA.GetDataList(0, Designoptions);
            DA.GetDataList(2, inpCriteria);
            DA.GetDataList(3, inpWeights);

            GH_Structure<GH_Number> dataTree = new GH_Structure<GH_Number>();

            // Check if data tree input is correct 
            if (DA.GetDataTree(1, out dataTree))
            {
                numberOfPaths = dataTree.PathCount;
                numberofitems = dataTree.DataCount;
                foreach (GH_Path path in dataTree.Paths)
                {
                    int length = dataTree.get_Branch(path).Count;
                    pathLengths.Add(length);
                }
            }

            bool boolcheck = true;
            double firstValue = numberofitems/numberOfPaths;

            // Check if data tree input is correct 
            foreach (int value in pathLengths)
            {
                if (value != firstValue)
                {
                    boolcheck = false;
                    break;
                }
            }
            
            // Run time messages for problematic inputs
            if ( inpCriteria.Count() != inpWeights.Count() || Designoptions.Count() != numberOfPaths)
            {
                // Error handling: if data retrieval fails for either input, display an error message.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The length of input Lists should match. Each option should have input for each defined Criteria");
                return;
            }
            
            if (boolcheck == false)
            {
                // Error handling: if data retrieval fails for either input, display an error message.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The length of each branch in a data matrix doesnt match. Each option should have data for all the criteria");
                return;
            }
            

            //TOPSIS process
            GH_Structure<GH_Number> inputTree;


            if (!DA.GetDataTree(1, out inputTree))
            {
                return;
            }

            inputTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            TreeToArrayConverter converter = new TreeToArrayConverter();
            double[,] dmarray = converter.ConvertTreeToArray(inputTree);
            var decisionmatrix = np.array(dmarray);
            var dmoutput = decisionmatrix.GetData<double>();

            var weights = np.array(inpWeights.ToArray());
            Axis axis = 0;
            var check_weights = (int)weights.sum(axis);

            if (check_weights != 10 )
            {
                // Error handling: if data retrieval fails for either input, display an error message.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The sum of all weights is not equal to 10");

                return;
            }

            var weights_deci = weights / 10;
            var weightedmatrix = converter.CalculateWeightedNormalizedmatrix(weights_deci, decisionmatrix);


            var performmatrix = weightedmatrix.sum(axis);
            var rankinglist = converter.CalculateRankings(performmatrix, Designoptions);

            DA.SetDataList(0, rankinglist);
            DA.SetDataList(1, dmoutput);
            DA.SetDataList(2, weightedmatrix.GetData<double>());
            DA.SetDataList(3, performmatrix.GetData<double>());

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
             return Resources.MCDA_Logo;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F45582C5-7538-4E17-B307-2F6486D66098"); }
        }

    }
}