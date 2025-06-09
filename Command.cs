using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;

namespace SpawnPoint
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SpawnPointCommand : ICommand
    {
        public string Command => "sp";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Управление личными точками спавна: create/tp/del/list";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "<color=red><b>Вы должны быть игроком!</b></color>";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "<color=#FFD700><b>Используйте:</b> <color=cyan>sp create/tp/del/list</color></color>";
                return false;
            }

            string sub = arguments.At(0).ToLower();
            string name = arguments.Count > 1 ? string.Join(" ", arguments.Skip(1)).Replace("\"", "").Trim() : string.Empty;

            switch (sub)
            {
                case "create":
                    return SpawnPointPlugin.Instance.CmdCreate(player, name, out response);
                case "tp":
                    return SpawnPointPlugin.Instance.CmdTp(player, name, out response);
                case "del":
                    return SpawnPointPlugin.Instance.CmdDel(player, name, out response);
                case "list":
                    return SpawnPointPlugin.Instance.CmdList(player, out response);
                default:
                    response = "<color=red><b>Неизвестная sub-команда: create/tp/del/list</b></color>";
                    return false;
            }
        }
    }
}
