using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace Lenticular
{
    public class Opennest : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Opennest()
          : base("OpenNest", "Opnenest",
              "Arrangement of output curves",
              "Lenticular", "Arrangement")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("curves", "curve", "listofcurves", GH_ParamAccess.list);
            pManager.AddNumberParameter("width", "width", "widthofcutsheet", GH_ParamAccess.item);
            pManager.AddNumberParameter("length", "length", "lengthofcutsheet", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("curves", "curves", "listofarrangedcurves", GH_ParamAccess.list);
            pManager.AddCurveParameter("curves", "curves", "listofcutsheetcurves", GH_ParamAccess.list);
            pManager.AddTextParameter("labels", "labels", "textlableofarrangedcurves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> inputCurves = new List<Curve>();
            double u = 0.0;
            double v = 0.0;

            // Get inputs
            if (!DA.GetDataList(0, inputCurves)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;

            if (inputCurves == null || inputCurves.Count == 0)
            {
                DA.SetData(0, "No curves provided.");
                DA.SetDataList(1, new List<Curve>());
                DA.SetDataList(2, new List<string>());
                return;
            }

            List<Curve> arrangedCurves = new List<Curve>();
            List<Curve> boundingBoxes = new List<Curve>();
            List<string> labels = new List<string>();

            // Sort curves by bounding box area (descending)
            inputCurves.Sort((c1, c2) =>
            {
                BoundingBox bbox1 = c1.GetBoundingBox(true);
                BoundingBox bbox2 = c2.GetBoundingBox(true);
                double area1 = (bbox1.Max.X - bbox1.Min.X) * (bbox1.Max.Z - bbox1.Min.Z);
                double area2 = (bbox2.Max.X - bbox2.Min.X) * (bbox2.Max.Z - bbox2.Min.Z);
                return area2.CompareTo(area1);
            });

            double xOffset = 0;
            double zOffset = 0;
            double maxHeightInRow = 0;
            int boundingBoxCount = 0;

            List<Curve> remainingCurves = new List<Curve>(inputCurves);
            int curveIndex = 1;

            while (remainingCurves.Count > 0)
            {
                // Create a new bounding box (XZ plane)
                double baseX = boundingBoxCount * (u + 10);
                Point3d p0 = new Point3d(baseX, 0, 0);
                Point3d p1 = new Point3d(baseX + u, 0, 0);
                Point3d p2 = new Point3d(baseX + u, 0, v);
                Point3d p3 = new Point3d(baseX, 0, v);
                Polyline boundingPolyline = new Polyline(new List<Point3d> { p0, p1, p2, p3, p0 });
                Curve boundingCurve = boundingPolyline.ToNurbsCurve();
                boundingBoxes.Add(boundingCurve);

                List<Curve> placedCurves = new List<Curve>();

                for (int i = remainingCurves.Count - 1; i >= 0; i--)
                {
                    Curve crv = remainingCurves[i];
                    BoundingBox bbox = crv.GetBoundingBox(true);
                    double width = bbox.Max.X - bbox.Min.X;
                    double height = bbox.Max.Z - bbox.Min.Z;

                    // Next row if needed
                    if (xOffset + width > u)
                    {
                        xOffset = 0;
                        zOffset += maxHeightInRow;
                        maxHeightInRow = 0;
                    }

                    // New bounding box if vertical space exceeded
                    if (zOffset + height > v)
                    {
                        boundingBoxCount++;
                        xOffset = 0;
                        zOffset = 0;
                        break;
                    }

                    maxHeightInRow = Math.Max(maxHeightInRow, height);

                    // Move curve into bounding box
                    Transform move = Transform.Translation(new Vector3d(baseX + xOffset - bbox.Min.X, -bbox.Min.Y, zOffset - bbox.Min.Z));
                    Curve newCrv = crv.DuplicateCurve();
                    newCrv.Transform(move);

                    // Check for overlaps
                    bool intersects = false;
                    foreach (Curve placed in placedCurves)
                    {
                        if (Rhino.Geometry.Intersect.Intersection.CurveCurve(newCrv, placed, 0.01, 0.01).Count > 0)
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (!intersects)
                    {
                        arrangedCurves.Add(newCrv);
                        placedCurves.Add(newCrv);
                        remainingCurves.RemoveAt(i);

                        // Add label
                        Point3d labelPosition = new Point3d(baseX + xOffset + width / 2, 0, zOffset + height / 2);
                        string textLabel = curveIndex.ToString();
                        labels.Add(textLabel);
                        curveIndex++;

                        xOffset += width;
                    }
                }
            }

            // Output
            DA.SetDataList(0, arrangedCurves);
            DA.SetDataList(1, boundingBoxes);
            DA.SetDataList(2, labels);
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
            get { return new Guid("E99631A1-3A56-458C-B79F-DCEE15484E2B"); }
        }
    }
}