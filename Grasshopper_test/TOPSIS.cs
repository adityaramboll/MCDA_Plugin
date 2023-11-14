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
    public class TOPSIS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public TOPSIS()
          : base("TOPSIS", "TOPSIS",
              "Performs Multi-Criteria Decision analysis process on a decision matrix using the TOPSIS method ",
              "DECIGEN", "TOPSIS")
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
            pManager.AddNumberParameter("Objectives", "Obj", "Specify objectives for the criteria as a list either Min(0) or Max (1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weights", "Wei", "Add the desired weights for each criteria as a list (0.1-1.0)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Rankings", "Rank", "Result of TOPSIS ranking process", GH_ParamAccess.list);
            pManager.AddNumberParameter("Ideal", "Idl", "Normalized sintectic better alternatives", GH_ParamAccess.list);
            pManager.AddNumberParameter("Anti-ideal", "AntiId", "Normalized sintectic worse alternatives", GH_ParamAccess.list);
            pManager.AddGenericParameter("Performance", "Sim", "Indicates how far from the anti-ideal and how closer to the ideal are the real alternatives", GH_ParamAccess.item);
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
            List<double> inpObjectives = new List<double>();
            List<double> inpWeights = new List<double>();
            int numberOfPaths = 0;
            int numberofitems = 0;
            List<int> pathLengths = new List<int>();

            //Inputs  initialized using actual data
            DA.GetDataList(0, Designoptions);
            DA.GetDataList(2, inpCriteria);
            DA.GetDataList(3, inpObjectives);
            DA.GetDataList(4, inpWeights);

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
            int firstValue = numberofitems/numberOfPaths;

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
            if (inpCriteria.Count() != inpObjectives.Count() || inpCriteria.Count() != inpWeights.Count() || Designoptions.Count() != numberOfPaths)
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
            

            var normalized_matrix= converter.CalculateNormalizedmatrix(decisionmatrix);
            var weights = np.array(inpWeights.ToArray());
            var weightedNormalizedmatrix = converter.CalculateWeightedNormalizedmatrix(weights, normalized_matrix);

            var objectivesnparray = np.array(inpObjectives.ToArray());

            var idealworstnparray = converter.Calculateidealworst(objectivesnparray,weightedNormalizedmatrix);
            var idealworstlist = idealworstnparray.GetData<double>();

            var idealbestnparray = converter.Calculateidealbest(objectivesnparray,weightedNormalizedmatrix);
            var idealbestlist = idealbestnparray.GetData<double>();

            var performancescorearray = converter.Calculateperformancescore(idealbestnparray, idealworstnparray);
            var performancescorelist = performancescorearray.GetData<double>();

            var rankinglist = converter.CalculateRankings(performancescorearray, Designoptions);

            DA.SetDataList(0, rankinglist);
            DA.SetDataList(1, idealbestlist);
            DA.SetDataList(2, idealworstlist);
            DA.SetDataList(3, performancescorelist);
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
            get { return new Guid("F45582C5-7538-4E17-B307-2F6486D66097"); }
        }

    }
}