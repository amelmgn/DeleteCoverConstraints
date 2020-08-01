using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace DeleteCoverConstraints
{
    class GMSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            Rebar rb = elem as Rebar;
            if (elem is Rebar)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
