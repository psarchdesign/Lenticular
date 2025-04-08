using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Lenticular
{
    public class Weight : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Weight()
          : base("weight", "weight",
              "total weight of the panels",
              "Lenticular", "Weight")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("breps", "breppanels", "listofbreps", GH_ParamAccess.list);
            pManager.AddNumberParameter("density", "density", "materialdensity", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("weight", "weight", "weightofpanels", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();
            double density = 0.0;

            // Get inputs from Grasshopper
            if (!DA.GetDataList(0, breps)) return;
            if (!DA.GetData(1, ref density)) return;

            // Validate inputs
            if (breps == null || breps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Breps provided.");
                return;
            }

            if (density <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Density must be greater than zero.");
                return;
            }

            double totalVolume = 0.0;

            // Process each Brep
            foreach (Brep brep in breps)
            {
                if (brep != null && brep.IsSolid)
                {
                    if (brep.SolidOrientation == BrepSolidOrientation.Inward)
                    {
                        brep.Flip();
                    }

                    VolumeMassProperties vmp = VolumeMassProperties.Compute(brep);
                    if (vmp != null)
                    {
                        totalVolume += Math.Abs(vmp.Volume);
                    }
                }
            }

            // Check volume
            if (totalVolume <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Total volume is zero or could not be calculated.");
                return;
            }

            // Compute total weight
            double weight = totalVolume * density;

            // Output
            DA.SetData(0, weight);
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
            get { return new Guid("21B38712-BC4C-49A4-B0E6-E4C4638AA43B"); }
        }
    }
}