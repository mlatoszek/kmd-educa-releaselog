using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;

namespace Kmd.Educa.ReleaseScope
{
    public class ReleaseLogExporter
    {
        public void ExportRelease(string exportPath, List<TaskInfo> tasks)
        {
            var resourceFileName = Assembly.GetAssembly(this.GetType()).GetManifestResourceNames().First(x => x.EndsWith("ReleaseTemplate.xlsx"));
            var templateStream = Assembly.GetAssembly(this.GetType()).GetManifestResourceStream(resourceFileName);            
            ExcelPackage pck = new ExcelPackage(templateStream);
            //Add the Content sheet
            var worksheet = pck.Workbook.Worksheets["Sheet1"];
            var rowIndex = 2;
            foreach (var taskInfo in tasks)
            {
                worksheet.Cells[rowIndex, 1].Value = taskInfo.Title;
                worksheet.Cells[rowIndex, 2].Value = taskInfo.WorkItemType;
                worksheet.Cells[rowIndex, 3].Hyperlink = new Uri($"https://kmd-education.visualstudio.com/defaultcollection/KMD%20Education/_workitems?id={taskInfo.Id}&_a=edit");
                worksheet.Cells[rowIndex, 3].Value = "" + taskInfo.Id;
                worksheet.Cells[rowIndex, 4].Value = taskInfo.AssignedTo;
                worksheet.Cells[rowIndex, 5].Value = taskInfo.State;
                worksheet.Cells[rowIndex, 6].Value = taskInfo.Priority;

                rowIndex++;
            }
            pck.SaveAs(new FileInfo(exportPath));
        }
    }
}
