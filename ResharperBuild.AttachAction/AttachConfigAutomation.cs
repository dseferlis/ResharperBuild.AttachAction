using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using JetBrains.Application.UI.Commands;
using JetBrains.DataFlow;
using JetBrains.IDE.RunConfig;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.UI.Controls;
using JetBrains.UI.Wpf;
using JetBrains.Util;
using JetBrains.VsIntegration.IDE.RunConfig;
using Microsoft.Win32;

namespace ResharperBuild.AttachAction
{
    public class AttachConfigAutomation : AAutomation, IRunConfigEditorAutomation, IAutomation, INotifyPropertyChanged
    {
        private readonly AttachConfig _myRunConfig;
        private readonly ISolution _mySolution;

        public IProperty<string> Path { get; private set; }

        public IProperty<string> WorkingDirectory { get; private set; }

        public IProperty<string> Arguments { get; private set; }

        public ICommand SelectPathCommand { get; private set; }

        public ICommand SelectDirectoryCommand { get; private set; }

        public IProperty<string> SelectedClrVersion { get; set; }

        public IProperty<string> WaitingSeconds { get; set; }

        public Property<string> WaitingSecondsError { get; private set; }

        public IProperty<Visibility> WaitingSecondsErrorVisibility { get; private set; }

        public Property<string> DirectoryError { get; private set; }

        public Property<Visibility> DirectoryErrorVisibility { get; private set; }

        public Property<Visibility> PathErrorVisibility { get; private set; }

        public Property<string> PathError { get; private set; }

        public IRunConfig Config {
            get { return _myRunConfig; }
        }

        public Property<bool> IsValid { get; private set; }

        public AttachConfigAutomation(Lifetime lifetime, AttachConfig runConfig, ISolution solution) {
            _myRunConfig = runConfig;
            _mySolution = solution;
            Path = new Property<string>(lifetime, "Path",
                SolutionRelativePathUtils.EnsureIsAbsoluteWithSolution(runConfig.ExePath, solution).FullPath);
            WorkingDirectory = new Property<string>(lifetime, "WorkingDirectory",
                SolutionRelativePathUtils.EnsureIsAbsoluteWithSolution(runConfig.WorkingDirectory, solution).FullPath);
            Arguments = new Property<string>(lifetime, "Arguments", runConfig.Arguments);
            var listEvents2 = new ListEvents<string>(lifetime, "ClrVersions");
            var dte = Shell.Instance.GetComponent<DTE>() as DTE2;
            var debugger2 = dte?.Debugger as Debugger2;
            if (debugger2 != null) {
                var transports = debugger2.Transports.Item("default");
                foreach (Engine engine in transports.Engines) {
                    listEvents2.Add(engine.Name);
                }
            }

            ClrVersions =
                (ListEvents<string>)
                listEvents2.OrderByLive(lifetime, StringComparer.Create(CultureInfo.CurrentCulture, true));
            SelectedClrVersion = new Property<string>(lifetime, "SelectedClrVersion", runConfig.ClrVersion);
            WaitingSeconds = new Property<string>(lifetime, "WaitingSeconds", runConfig.WaitSeconds.ToString());
            IsValid = new Property<bool>(lifetime, "IsValid");
            SelectPathCommand = new DelegateCommand(SelectPathExecute);
            SelectDirectoryCommand = new DelegateCommand(SelectDirectoryExecute);
            PathError = new Property<string>(lifetime, "NameError", null, true);
            PathErrorVisibility = new Property<Visibility>(lifetime, "NameErrorVisibility");
            PathError.FlowInto(lifetime, PathErrorVisibility,
                s => !s.IsNullOrWhitespace() ? Visibility.Visible : Visibility.Collapsed);
            DirectoryErrorVisibility = new Property<Visibility>(lifetime, "DirectoryErrorVisibility");
            DirectoryError = new Property<string>(lifetime, "DirectoryError", null, true);
            DirectoryError.FlowInto(lifetime, DirectoryErrorVisibility,
                s => !s.IsNullOrWhitespace() ? Visibility.Visible : Visibility.Collapsed);
            WaitingSecondsErrorVisibility = new Property<Visibility>(lifetime, "WaitingSecondsErrorVisibility");
            WaitingSecondsError = new Property<string>(lifetime, "WaitingSecondsError", null, true);
            int waitSeconds;
            WaitingSecondsError.FlowInto(lifetime, WaitingSecondsErrorVisibility,
                s => !s.IsNullOrWhitespace() ? Visibility.Visible : Visibility.Collapsed);

            PropertyBinding.Create3(lifetime, DirectoryError, PathError, WaitingSecondsError, IsValid, (s, s1, s2) => {
                var executableError = s.IsNullOrWhitespace() && s1.IsNullOrWhitespace();
                return executableError && s2.IsNullOrWhitespace();
            });

            Path.Change.Advise_HasNew(lifetime, args => {
                string str = args.New;
                if (str.IsNullOrEmpty()) PathError.Value = "Path cannot be empty";
                else if (FileSystemPath.TryParse(str).IsEmpty)
                    PathError.Value = "Invalid path";
                else
                    PathError.Value = null;
            });
            WorkingDirectory.Change.Advise_HasNew(lifetime, args => {
                    string str = args.New;
                    if (!str.IsNullOrEmpty() && FileSystemPath.TryParse(str).IsEmpty)
                        DirectoryError.Value = "Invalid path";
                    else
                        DirectoryError.Value = null;
                });

            WaitingSeconds.Change.Advise_HasNew(lifetime, args => {
                var str = args.New;
                if (!int.TryParse(str, out waitSeconds)) {
                    WaitingSecondsError.Value = "Has to be a number (int)";
                }
                else if (waitSeconds < 0) {
                    WaitingSecondsError.Value = "Has to be non-negative number";
                }
                else {
                    WaitingSecondsError.Value = null;
                }
            });
        }

        public ListEvents<string> ClrVersions { get; set; }

        private void SelectDirectoryExecute() {
            FileSystemPath fileSystemPath = CommonFileDialogs.BrowseForFolder("Select Folder",
                FileSystemPath.Parse(WorkingDirectory.Value), null, null);
            if (!(fileSystemPath != null))
                return;
            WorkingDirectory.Value = fileSystemPath.FullPath;
        }

        private void SelectPathExecute() {
            FileSystemPath other = FileSystemPath.TryParse(Path.Value);
            FileSystemPath path = FileSystemPath.TryParse(WorkingDirectory.Value);
            bool flag = path.Equals(other.Directory) || path.Equals(other) || path.IsNullOrEmpty();
            OpenFileDialog openFileDialog1 = new OpenFileDialog {
                InitialDirectory = other.Directory.FullPath,
                Filter = "Executables (*.exe)|*.exe|All Files|*.*"
            };
            OpenFileDialog openFileDialog2 = openFileDialog1;
            bool? nullable = openFileDialog2.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 1 : (!nullable.HasValue ? 1 : 0)) != 0)
                return;
            Path.Value = openFileDialog2.FileName;
            FileSystemPath fileSystemPath = FileSystemPath.TryParse(Path.Value);
            if (!flag || fileSystemPath.IsEmpty)
                return;
            WorkingDirectory.Value = fileSystemPath.Directory.FullPath;
        }

        public void UpdateModel() {
            UpdateModel(_myRunConfig);
        }

        private void UpdateModel(AttachConfig config) {
            config.ExePath = SolutionRelativePathUtils.TryMakeRelativeToSolution(Path.Value.Trim(), _mySolution);
            config.WorkingDirectory =
                SolutionRelativePathUtils.TryMakeRelativeToSolution(WorkingDirectory.Value.Trim(), _mySolution);
            config.Arguments = Arguments.Value;
            config.ClrVersion = SelectedClrVersion.Value;
            config.WaitSeconds = int.Parse(WaitingSeconds.Value);
        }
    }
}