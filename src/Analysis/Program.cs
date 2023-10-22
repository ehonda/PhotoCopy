using Analysis;
using Spectre.Console.Cli;

var app = new CommandApp<AnalysisCommand>();
return await app.RunAsync(args);

