using System.Diagnostics;
using System.Text;

namespace Kmd.Educa.ReleaseScope
{
    class GitTasks
    {
        public string GetCommits(string workingDirectory, string fromBranch, string toBranch)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git.exe",
                    Arguments = $"log --pretty=oneline {fromBranch}..{toBranch}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };

            proc.Start();
            StringBuilder builder = new StringBuilder();
            while (!proc.StandardOutput.EndOfStream)
            {
                builder.AppendLine(proc.StandardOutput.ReadLine());
            }

            return builder.ToString();
        }
    }
}
