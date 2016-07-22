using System;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Kmd.Educa.ReleaseScope
{
    public class TaskInfo
    {
        public int Id { get; set; }
        public string WorkItemType { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string AssignedTo { get; set; }
        public int? ParentId { get; set; }
        public int? Priority { get; set; }
    }

    class TfsConnection
    {
        private TfsTeamProjectCollection tpc;
        private WorkItemStore workItemStore;

        public TfsConnection(string tfsUrl, string login, string password)
        {
            tpc = new TfsTeamProjectCollection(new Uri(tfsUrl));
            tpc.Credentials = new NetworkCredential(login, password);
            tpc.EnsureAuthenticated();
            workItemStore = (WorkItemStore)tpc.GetService(typeof(WorkItemStore));
        }

        public TaskInfo GetTastInfo(int taskNumber)
        {           
            WorkItemCollection queryResults = workItemStore.Query(
               "Select [Work Item Type], [Title], [State], [Assigned To], [Priority] " +
               "From WorkItems " +
               $"Where [ID] = {taskNumber}" +
               "Order By [ID] Asc");

            TaskInfo info = new TaskInfo();

            foreach (WorkItem workItem in queryResults)
            {
                info.Id = workItem.Id;
                info.State = workItem["State"] as string;
                info.Title = workItem["Title"] as string;
                info.Priority = workItem["Priority"] as int?;
                info.WorkItemType = workItem["Work Item Type"] as string;
                info.AssignedTo = workItem["Assigned To"] as string;
                info.ParentId =
                    workItem.Links.OfType<RelatedLink>()
                        .Where(x => x.LinkTypeEnd.Name == "Parent")
                        .Select(x => x.RelatedWorkItemId)
                        .FirstOrDefault();
            }
            return info;
        }
    }
}
