namespace MiniblogToGhost
{
    using CommandLine;
    using CommandLine.Text;

    public class Options
    {
        [Option('n', "cloudinaryapikey", Required = true, DefaultValue = "", HelpText = "ApiKey")]
        public string CloudinaryApiKey { get; set; }

        [Option('a', "cloudinaryapisecret", Required = true, DefaultValue = "", HelpText = "Api Secret.")]
        public string CloudinaryApiSecret { get; set; }

        [Option('c', "cloudinarycloudname", Required = true, DefaultValue = "", HelpText = "Prints all messages to standard output.")]
        public string CloudinaryCloudName { get; set; }

        [Option('f', "Input Directory", DefaultValue = @"")]
        public string InputDirectory { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [Option('o', "Output Path", DefaultValue = @"")]
        public string OutputPath { get; set; }

        [Option('v', "verbose", DefaultValue = true, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage() => HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }
}
