using JetBrains.Application.Settings;
using JetBrains.IDE.RunConfig;

namespace ResharperBuild.AttachAction
{
    [SettingsKey(typeof(ConfigSettings), "Attach config")]
    public class AttachSettings
    {
        [SettingsEntry(null, "Path to .exe")]
        public string Executable;
        [SettingsEntry(null, "Working directory")]
        public string WorkingDirectory;
        [SettingsEntry(null, "Command line arguments")]
        public string Arguments;
        [SettingsEntry(null, "Clr version")]
        public string ClrVersion { get; set; }
        [SettingsEntry(null, "Wait Seconds")]
        public int WaitSeconds { get; set; }
    }
}