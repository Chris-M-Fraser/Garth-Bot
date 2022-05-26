﻿using Renci.SshNet;
using Spectre.Console;

namespace Garth.Deploy;

public class DeploymentManager
{
    private readonly DeploymentTarget[] _deploymentTargets = new[]
    {
        new DeploymentTarget()
        {
            Name = "Production",
            Address = "138.197.128.238",
            User = "root",
            TargetType = TargetType.Production
        }
    };
    
    public DeploymentManager()
    {
        var target = _deploymentTargets[0];
        Console.WriteLine($"Enter the password for {target.User}@{target.Address}");
        Console.Write(" > ");
        var pass = Console.ReadLine();

        var connectionInfo = new ConnectionInfo(target.Address, 22, target.User,
            new PasswordAuthenticationMethod(target.User, pass));

        AnsiConsole.WriteLine("Starting deployment...");
        
        using (var client = new SshClient(connectionInfo))
        {
            AnsiConsole.WriteLine("Connecting to server...");
            client.Connect();
            AnsiConsole.WriteLine("Connecting...");
            var pull = client.RunCommand("cd /home/garth/Garth-Bot && git pull");
            var restart = client.RunCommand("sudo systemctl restart Garth");
            foreach (var sshCommand in new [] {pull, restart}.Where(t => t.ExitStatus != 0))
            {
                AnsiConsole.MarkupLine($"[red]ERR: [/][gray]{sshCommand.Error}[/]");
            }
            AnsiConsole.WriteLine("Deployment Complete!");
        }
    }
}