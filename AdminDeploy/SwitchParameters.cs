using Ookii.CommandLine;

namespace AdminDeploy
{
    class SwitchParameters
    {
        [CommandLineArgument(IsRequired = false)]
        public string ComputerList { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string ComputerName { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string OutputLocation { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string FormatJSON { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string ThreadCount { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string WmiTimeout { get; set; }
        [CommandLineArgument(IsRequired = false)]
        public string ThreadTimeout { get; set; }
        [CommandLineArgument(Position = 0, IsRequired = true)]
        public string SourceDirectory { get; set; }
        [CommandLineArgument(Position = 1, IsRequired = true)]
        public string ScriptToRun { get; set; }
        [CommandLineArgument(Position = 2, IsRequired = true)]
        public string Username { get; set; }
        [CommandLineArgument(Position = 3, IsRequired = true)]
        public string Domain { get; set; }
    }
}
