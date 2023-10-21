using App;
using Spectre.Console.Cli;

var app = new CommandApp<CopyCommand>();
return await app.RunAsync(args);
