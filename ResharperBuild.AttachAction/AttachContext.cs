using JetBrains.Annotations;
using JetBrains.IDE.RunConfig;
using JetBrains.ProjectModel;

namespace ResharperBuild.AttachAction
{
    public struct AttachContext
    {
        [NotNull]
        public IExecutionProvider ExecutionProvider { get; set; }

        public ISolution Solution { get; set; }

        public bool BuildInExecution { get; set; }

        public IRunConfigProcessTracker ProcessTracker { get; set; }
    }
}