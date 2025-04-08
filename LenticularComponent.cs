using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Lenticular
{
  public class LenticularComponent : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public LenticularComponent()
      : base("LenticularComponent", "Contouring",
        "dividing volume into contoruing and forming surfaces",
        "Lenticular", "Contouring")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddBrepParameter("brep", "brep", "brepcontour", GH_ParamAccess.item);
            pManager.AddNumberParameter("interval", "gap", "contourgap", GH_ParamAccess.item);
            pManager.AddNumberParameter("thickness", "thickness", "thicknesssofcontour", GH_ParamAccess.item);

    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("contourcurves", "contours", "contours", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("contoursurfaces", "surface", "surfaces", GH_ParamAccess.list);
            pManager.AddBrepParameter("contourthickness", "thickness", "volume", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
    {
            Brep brep = null;
            double interval = double.NaN;
            double thickness = double.NaN;

            DA.GetData(0, ref brep);
            DA.GetData(1, ref interval);
            DA.GetData(2, ref thickness);

            if (brep == null || !brep.IsValid) return;

            List<Curve> closedContours = new List<Curve>();
            List<Brep> surfaces = new List<Brep>();
            List<Brep> extrusions = new List<Brep>();

            BoundingBox bbox = brep.GetBoundingBox(true);
            double yMin = bbox.Min.Y;
            double yMax = bbox.Max.Y;
            double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            for (double y = yMin; y <= yMax; y += interval)
            {
                Plane plane = new Plane(new Point3d(0, y, 0), Vector3d.YAxis);
                Curve[] intersectionCurves;
                Point3d[] intersectionPoints;

                if (Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, plane, tolerance, out intersectionCurves, out intersectionPoints))
                {
                    foreach (Curve originalCurve in intersectionCurves)
                    {
                        Curve curve = originalCurve.DuplicateCurve(); // Prevent modifying original

                        if (!curve.IsClosed)
                        {
                            Curve[] joinedCurves = Curve.JoinCurves(new List<Curve> { curve }, tolerance);
                            if (joinedCurves.Length > 0)
                            {
                                curve = joinedCurves[0];
                            }
                        }

                        if (!curve.IsClosed)
                        {
                            Point3d start = curve.PointAtStart;
                            Point3d end = curve.PointAtEnd;

                            if (!start.DistanceTo(end).Equals(0)) // Check if already closed
                            {
                                LineCurve closingSegment = new LineCurve(end, start);
                                Curve[] finalJoined = Curve.JoinCurves(new Curve[] { curve, closingSegment }, tolerance);
                                if (finalJoined.Length > 0)
                                {
                                    curve = finalJoined[0];
                                }
                            }
                        }

                        closedContours.Add(curve); // Add final closed curve

                        // Create a planar surface from the closed curve
                        Brep[] planarBreps = Brep.CreatePlanarBreps(curve, tolerance);
                        if (planarBreps != null && planarBreps.Length > 0)
                        {
                            surfaces.Add(planarBreps[0]);
                        }

                        // Extrude the curve into a solid
                        Vector3d extrusionVector = new Vector3d(0, thickness, 0);
                        Surface extrusionSurface = Surface.CreateExtrusion(curve, extrusionVector);

                        if (extrusionSurface != null)
                        {
                            Brep extrusionBrep = Brep.CreateFromSurface(extrusionSurface);

                            // Cap both ends to make it a solid
                            if (extrusionBrep != null)
                            {
                                Brep cappedExtrusion = extrusionBrep.CapPlanarHoles(tolerance);
                                if (cappedExtrusion != null && cappedExtrusion.IsSolid)
                                {
                                    extrusions.Add(cappedExtrusion);
                                }
                            }
                        }
                    }
                }
            }

         

            DA.SetDataList(0, closedContours);
            DA.SetDataList(1, surfaces);
            DA.SetDataList(2, extrusions);
             

        }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// You can add image files to your project resources and access them like this:
    /// return Resources.IconForThisComponent;
    /// </summary>
    protected override System.Drawing.Bitmap Icon => null;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid => new Guid("745e6e8a-12b5-4269-a987-78a204ef88e0");
  }
}