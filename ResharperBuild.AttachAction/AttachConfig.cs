using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using EnvDTE80;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.IDE.RunConfig;
using JetBrains.ProjectModel;
using JetBrains.Threading;
using JetBrains.UI.Icons;
using JetBrains.Util;
using JetBrains.VsIntegration.IDE.RunConfig;
using JetBrains.VsIntegration.Resources;
using DTEProcess = EnvDTE80.Process2;
using Process = System.Diagnostics.Process;

namespace ResharperBuild.AttachAction
{
    public class AttachConfig : RunConfigBase
    {
        public string ExePath { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public string ClrVersion { get; set; }
        public int WaitSeconds { get; set; }

        private readonly DTE2 _dte;

        public AttachConfig(DTE2 dte) {
            _dte = dte;
            ExePath = string.Empty;
            WorkingDirectory = string.Empty;
            Arguments = string.Empty;
            ClrVersion = string.Empty;
            WaitSeconds = 0;
        }

        public override void Execute(RunConfigContext context) {
            var processStartInfo = GetStartInfo(context);
            if (context.ExecutionProvider.Name == "Debug") {
                FileSystemPath fileSystemPath = FileSystemPath.Parse(processStartInfo.FileName);
                if (!fileSystemPath.ExistsFile)
                    throw new ArgumentException("Exe does not exists").WithSensitiveData("Path",
                        fileSystemPath);
                if (!FileSystemPath.Parse(processStartInfo.WorkingDirectory).ExistsDirectory)
                    throw new ArgumentException("Invalid working directory");

                var process = new Process {StartInfo = processStartInfo};
                process.Start();

                if (WaitSeconds > 0) {
                    JetDispatcher.RunOrSleep(new TimeSpan(0, 0, WaitSeconds));
                    //JetDispatcher.RunOrSleep(() => process.HasExited, new TimeSpan(0, 0, WaitSeconds));
                }
                
                var dbg2 = _dte.Debugger as Debugger2;
                if (dbg2 == null) return;
                var engines = dbg2.Transports.Item("default").Engines;
                var selectedEngine = new Engine[1];
                foreach (Engine engine in engines) {
                    if (!engine.Name.Equals(ClrVersion)) continue;
                    selectedEngine[0] = engine;
                    break;
                }

                var processToAttach =
                    _dte.Debugger.LocalProcesses.Cast<DTEProcess>().FirstOrDefault(p => p.ProcessID == process.Id);
                processToAttach?.Attach2(selectedEngine);
            }
            else {
                context.ExecutionProvider.Execute(processStartInfo, context, this);
            }
        }

        private ProcessStartInfo GetStartInfo(RunConfigContext context) {
            var processStartInfo = new ProcessStartInfo {
                FileName = SolutionRelativePathUtils.EnsureIsAbsoluteWithSolution(ExePath, context.Solution).FullPath
            };
            var fileSystemPath = SolutionRelativePathUtils.EnsureIsAbsoluteWithSolution(WorkingDirectory,
                context.Solution);
            if (FileSystemPath.Empty != fileSystemPath)
                processStartInfo.WorkingDirectory = fileSystemPath.FullPath;
            processStartInfo.Arguments = Arguments;
            return processStartInfo;
        }

        public override IconId IconId => RunConfigThemedIcons.RunConfigExe.Id;

        public override IRunConfigEditorAutomation CreateEditor(Lifetime lifetime,
            IRunConfigCommonAutomation commonEditor, ISolution solution) {
            return new AttachConfigAutomation(lifetime, this, solution);
        }

        protected override bool ReadSpecific(IContextBoundSettingsStore store, Dictionary<SettingsKey, object> mapping) {
            if (!base.ReadSpecific(store, mapping))
                return false;
            var key = store.GetKey<AttachSettings>(SettingsOptimization.OptimizeDefault, mapping);
            ExePath = key.Executable;
            WorkingDirectory = key.WorkingDirectory;
            Arguments = key.Arguments;
            ClrVersion = key.ClrVersion;
            WaitSeconds = key.WaitSeconds;
            return true;
        }

        protected override void SaveSpecific(IContextBoundSettingsStore store, Dictionary<SettingsKey, object> mapping) {
            base.SaveSpecific(store, mapping);
            store.SetIfChanged((Expression<Func<AttachSettings, string>>) (s => s.Executable), ExePath, mapping);
            store.SetIfChanged((Expression<Func<AttachSettings, string>>) (s => s.WorkingDirectory), WorkingDirectory,
                mapping);
            store.SetIfChanged((Expression<Func<AttachSettings, string>>) (s => s.Arguments), Arguments, mapping);
            store.SetIfChanged((Expression<Func<AttachSettings, string>>) (s => s.ClrVersion), ClrVersion, mapping);
            store.SetIfChanged((Expression<Func<AttachSettings, int>>) (s => s.WaitSeconds), WaitSeconds, mapping);
        }
    }
}