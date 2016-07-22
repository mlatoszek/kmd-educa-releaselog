using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kmd.Educa.ReleaseScope
{
    class Program
    {
        static void Main(string[] args)
        {
            var fromBranch = args[0];
            var toBranch = args[1];
            var currentDirectory = Environment.CurrentDirectory;
            var exportPath = ConfigurationManager.AppSettings["ExportPath"];

            TfsConnection tfsConnection = new TfsConnection(
                ConfigurationManager.AppSettings["TfsUrl"],
                ConfigurationManager.AppSettings["TfsLogin"],
                ConfigurationManager.AppSettings["TfsPassword"]);

            var commitsText = new GitTasks().GetCommits(currentDirectory, fromBranch, toBranch);
            Console.WriteLine("Getting unique tasks numbers from commits");
            var taskNumbers = GetUniqueTaskNumbers(commitsText);

            Console.WriteLine($"Found {taskNumbers.Count} unique tasks" );

            var taskInfos = new List<TaskInfo>();

            foreach (var taskNumber in taskNumbers)
            {
                var taskInfo = tfsConnection.GetTastInfo(int.Parse(taskNumber.Trim('#')));
                if (taskInfo.ParentId.GetValueOrDefault(0) != 0)
                {
                    taskInfo = tfsConnection.GetTastInfo(taskInfo.ParentId.Value);
                }
                taskInfos.Add(taskInfo);
            }

            Console.WriteLine($"Exporting {taskInfos.Count} to excel file");
            
            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }

            taskInfos = taskInfos.Distinct(new TaskInfoEqualityByIdComparer()).ToList();
            new ReleaseLogExporter().ExportRelease(exportPath, taskInfos);
            Console.WriteLine("Generation of release log complete. Press any key to exit.");
            Console.ReadKey();
        }

        public static List<string> GetUniqueTaskNumbers(string commitText)
        {
            var taskNumberRegex = "#[0-9]{4}";
            var matches = Regex.Matches(commitText, taskNumberRegex);
            HashSet<string> elements = new HashSet<string>();

            foreach (Match match in matches)
            {
                
                if (!elements.Contains(match.Value))
                {
                    elements.Add(match.Value);
                }
            }

            return elements.ToList();
        }        
    }

    public class TaskInfoEqualityByIdComparer : IEqualityComparer<TaskInfo>
    {
        public bool Equals(TaskInfo x, TaskInfo y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(TaskInfo obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
