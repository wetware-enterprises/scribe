using Scribe.Bot;
using Scribe.Hackmud;

var client = new GameClientBuilder().Build();

using var bot = new ScribeBot(client);
bot.Initialize();

Console.ReadLine();