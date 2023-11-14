using Spectre.Console.Cli;
using Verify;

var app = new CommandApp<VerifyCommand>();
return await app.RunAsync(args);
