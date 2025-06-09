using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpawnPoint
{
    public class SpawnPointConfig : IConfig
    {
        [Description("Включить плагин?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Разрешить обычным игрокам без прав использовать свои точки?")]
        public bool AllowDefaultPlayers { get; set; } = false;

        [Description("Лимит точек по умолчанию для игроков без специальных прав")]
        public int DefaultLimit { get; set; } = 1;

        [Description("Ограничения по правам: ключ - permission, значение - лимит")]
        public Dictionary<string, int> RolePermissions { get; set; } = new Dictionary<string, int>
        {
            { "spawnpoint.owner", 10 },
            { "spawnpoint.admin", 5 },
            { "spawnpoint.moderator", 3 },
            { "spawnpoint.user", 1 }
        };

        [Description("Сообщение при входе (отображается в консоли игрока)")]
        public string ConsoleHelloMessage { get; set; } = "<color=yellow><b>Плагин <color=lime>SpawnPoint</color> активен!</b>\n<color=cyan>Для справки: <b>sp list</b></color></color>";

        [Description("Сообщение о недостатке прав")]
        public string NoPermissionMsg { get; set; } = "<color=red><b>❌ У вас нет разрешения на использование точек спавна!</b></color>";

        [Description("Включить режим отладки")]
        public bool Debug { get; set; } = false;
    }
}
