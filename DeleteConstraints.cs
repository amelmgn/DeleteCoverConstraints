using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace DeleteCoverConstraints
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DeleteConstraints : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            //Выделение элемента
            ElementId selRebId;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;
            ISelectionFilter SelFilter = new GMSelectionFilter();

            if (sel.GetElementIds().Count != 1)
            {
                Reference R = sel.PickObject(ObjectType.Element, SelFilter, "Выберите стержень");
                selRebId = R.ElementId;
            }
            else
            {
                selRebId = sel.GetElementIds().First();
            }

            Rebar selReb = doc.GetElement(selRebId) as Rebar;
            if (selReb == null)
            {
                TaskDialog.Show("Ошибка", "Выбран не стержень!");
                return Result.Failed;
            }
            //Конец блока выделения элемента

            RebarConstraintsManager rcm = selReb.GetRebarConstraintsManager();
            IList<RebarConstrainedHandle> rch_list = rcm.GetAllConstrainedHandles();

            double hostFaceDistPrev;
            double hostFaceDistCur;
            RebarConstrainedHandle selConstHandle = null;
            RebarConstraint correctConst = null;

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Удалить зависимости к з.с.");

                foreach (RebarConstrainedHandle rch in rch_list)
                {
                    if (rch.GetHandleType() == RebarHandleType.Edge)
                    {
                        hostFaceDistPrev = 9999;
                        RebarConstraint curRebarConst = rcm.GetCurrentConstraintOnHandle(rch);
                        IList<RebarConstraint> rebConstCandidatesList = rcm.GetConstraintCandidatesForHandle(rch);

                        foreach (RebarConstraint rcc in rebConstCandidatesList)
                        {
                            if (rcc.IsFixedDistanceToHostFace() == true)
                            {
                                hostFaceDistCur = rcc.GetDistanceToTargetHostFace();

                                if (Math.Abs(hostFaceDistCur) < hostFaceDistPrev)
                                {
                                    correctConst = rcc;
                                    selConstHandle = rch;
                                    hostFaceDistPrev = Math.Abs(hostFaceDistCur);
                                }
                                else continue;
                                
                            }
                            else continue;
                        }

                    }
                    else continue;

                    rcm.SetPreferredConstraintForHandle(selConstHandle, correctConst);

                }

                t.Commit();

                TaskDialog.Show("Информация", "Зависимости удалены!");
            }
            message = "Возникли ошибки. Действие не выполнено";
            return Result.Succeeded;
        }
    }
}
