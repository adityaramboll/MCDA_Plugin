using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper;
using System.Drawing;
using Grasshopper.Kernel.Parameters;

namespace FirstComponent
{
    #region Methods of GH_Component interface

    public class FirstCompGH : GH_Component, IGH_VariableParameterComponent
    {

        public FirstCompGH() : base("test", "test", "test", "MCDA", "Test")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Criteria", "Criteria 1", "Add the criteria for comparison", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Rankings", "Rankings", "Result of TOPSIS ranking process", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int inputParamCount = Params.Input.Count;
            List<double> outputlist = new List<double>();


            if (inputParamCount > 0)
            {
                for (int i = 0; i < inputParamCount; i++)
                {
                   List<double> input_list = new List<double>();
                   DA.GetDataList(i, input_list);
                   List<double> multipliedNumbers = input_list.Select(n => n * 5).ToList();
                   DA.SetDataList(0, multipliedNumbers);
                }
            }

            
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("831F08AB-044A-4897-A936-E30528ADA4F9"); }
        }
        #endregion

        #region Methods of IGH_VariableParameterComponent interface

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            //We only let input parameters to be added (output number is fixed at one)
            if (side == GH_ParameterSide.Input)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            //We can only remove from the input
            if (side == GH_ParameterSide.Input && index == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number param = new Param_Number();

            param.Name = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.NickName = param.Name;
            param.Description = "Param" + (Params.Input.Count + 1);
            param.SetPersistentData(0.0);
            param.MutableNickName = false;
            param.Access = GH_ParamAccess.list;

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //Nothing to do here by the moment
            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            for (int i = 0; i < Params.Input.Count; i++)

            {

                Params.Input[i].NickName = "Criteria" + (i + 1).ToString();
            }
        }

        #endregion

    }


}
