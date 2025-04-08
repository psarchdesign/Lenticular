using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Lenticular
{
    public class Area : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Area()
          : base("Area", "SurfaceArea",
              "total material area",
              "Lenticular", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("surfaces", "surfaces", "totalarea", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("totalarea", "area", "totalarea", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Surface> surfaces = new List<Surface>();

            // Get surfaces from Grasshopper input
            if (!DA.GetDataList(0, surfaces)) return;
            if (surfaces == null || surfaces.Count == 0) return;

            double totalArea = 0.0;

            foreach (Surface surface in surfaces)
            {
                if (surface != null && surface.IsValid)
                {
                    Brep brepSurface = surface.ToBrep(); // Convert Surface to Brep

                    if (brepSurface != null)
                    {
                        AreaMassProperties areaProps = AreaMassProperties.Compute(brepSurface);

                        if (areaProps != null)
                        {
                            totalArea += areaProps.Area;
                        }
                    }
                }
            }

            // Output the total area
            DA.SetData(0, totalArea);
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
            get { return new Guid("C63ECA66-4B99-45CD-BB9F-C708C093BD73"); }
        }
    }
}