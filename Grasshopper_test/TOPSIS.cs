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
              "MCDA", "TOPSIS")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("inputMatrix", "IM", "Add a decision matrix to evaluate", GH_ParamAccess.tree);
            pManager.AddTextParameter("Criteria", "Crit", "Add the names of various criteria used for decision making ", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Objectives", "Obj", "Specify objectives for the criteria as a list either Min(0) or Max (1)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weights", "Wei", "Add the desired weights for each criteria as a list (0-10)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Rankings", "Rank", "Result of TOPSIS ranking process", GH_ParamAccess.list);
            pManager.AddNumberParameter("Ideal", "Idl", "Normalized sintectic better alternatives", GH_ParamAccess.list);
            pManager.AddNumberParameter("Anti-ideal", "AntiId", "Normalized sintectic worse alternatives", GH_ParamAccess.list);
            pManager.AddGenericParameter("Similarity", "Sim", "Indicates how far from the anti-ideal and how closer to the ideal are the real alternatives", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Inputs empties initialized
            List<string> inpCriteria = new List<string>();
            List<bool> inpObjectives = new List<bool>();
            List<double> inpWeights = new List<double>();
            int numberOfPaths = 0;
            List<int> pathLengths = new List<int>();

            //Inputs  initialized using actual data
            DA.GetDataList(1, inpCriteria);
            DA.GetDataList(2, inpObjectives);
            DA.GetDataList(3, inpWeights);

            GH_Structure<GH_Number> dataTree = new GH_Structure<GH_Number>();

            // Check if data tree input is correct 
            if (DA.GetDataTree(0, out dataTree))
            {
                numberOfPaths = dataTree.PathCount;

                foreach (GH_Path path in dataTree.Paths)
                {
                    int length = dataTree.get_Branch(path).Count;
                    pathLengths.Add(length);
                }
            }

            bool boolcheck = true;
            int firstValue = numberOfPaths;

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
            if (inpCriteria.Count() != inpObjectives.Count() || inpCriteria.Count() != inpWeights.Count() ||  inpCriteria.Count() != numberOfPaths)
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

            GH_Structure<GH_Number> inputTree = new GH_Structure<GH_Number>();
            

            if (!DA.GetDataTree(0, out inputTree))
            {
                return;
            }

            inputTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            TreeToArrayConverter converter = new TreeToArrayConverter();
            double[][] dmarray = converter.ConvertTreeToArray(inputTree);
            var m = np.array(dmarray[0][1]);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
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