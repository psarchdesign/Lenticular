using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace Lenticular
{
  public class LenticularInfo : GH_AssemblyInfo
  {
    public override string Name => "Lenticular";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => null;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("9520cdef-c2dd-4b5b-ae2b-6b6928457bc4");

    //Return a string identifying you or your company.
    public override string AuthorName => "";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "";

    //Return a string representing the version.  This returns the same version as the assembly.
    public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
  }
}