using EnvDTE;
using EnvDTE80;
using JetBrains.Application;
using JetBrains.IDE.RunConfig;
using JetBrains.UI.Icons;
using JetBrains.VsIntegration.Resources;

namespace ResharperBuild.AttachAction
{
    [ShellComponent]
    public class AttachConfigProvider : RunConfigProviderBase
    {
        private readonly DTE2 _dte;

        public AttachConfigProvider(DTE dte) {
            _dte = dte as DTE2;
        }

        public override IRunConfig CreateNew() {
            AttachConfig attachConfig = new AttachConfig(_dte) {Type = Type};
            return attachConfig;
        }

        public override string Name {
            get { return "Attach"; }
        }

        public override string Type {
            get { return "atc"; }
        }

        public override IconId IconId {
            get { return RunConfigThemedIcons.RunConfigExe.Id; }
        }
    }
}