
using System;
using System.Collections.Generic;
using System.Linq;
using Magic8HeadService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;

public class CommandCommand : ICommandMbhToTwitch
{
    private readonly TwitchClient client;
    private readonly IEnumerable<ICommandMbhToTwitch> listOfCommands;
    private readonly IConfiguration config;
    private readonly ILogger<Worker> logger;

    public string Name => "command";
    public string Description => "command of commands";

    public IEnumerable<string> commandNames { get; set; }


    // public CommandCommand(TwitchClient client, IEnumerable<ICommandMbhToTwitch> listOfCommands, IConfiguration config, ILogger<Worker> logger)
    public CommandCommand(TwitchClient client, IOptionsSnapshot<List<string>> commandNames, IConfiguration config, ILogger<Worker> logger)
    {
        this.client         = client;
        this.commandNames   = commandNames.Get("CommandNames");
        this.config         = config;
        this.logger         = logger;
    }

    public void Handle(OnChatCommandReceivedArgs args)
    {
        logger.LogInformation($"commands: {string.Join(',', commandNames)}");
        client.SendMessage(args.Command.ChatMessage.Channel, $"commands: ");
        // {string.Join(',', commands.ToArray())}
    }
}
