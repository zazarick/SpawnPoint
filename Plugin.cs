using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace SpawnPoint
{
    public class SpawnPointPlugin : Plugin<SpawnPointConfig>
    {
        public static SpawnPointPlugin Instance;

        public override string Name => "SpawnPoint";
        public override string Author => "zazarick";
        public override string Prefix => "spawnpoint";
        public override Version Version => new Version(2, 2, 6);

        private string SavePath
        {
            get { return Path.Combine(Paths.Configs, "player_spawnpoints.json"); }
        }
        private Dictionary<string, Dictionary<string, SerializableVector3>> playerPoints =
            new Dictionary<string, Dictionary<string, SerializableVector3>>();

        public override void OnEnabled()
        {
            Instance = this;
            LoadPoints();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            SavePoints();
            base.OnDisabled();
        }

        public bool CmdCreate(Player player, string pointName, out string response)
        {
            if (!HasPermission(player))
            {
                response = "<color=red><b>❌ У вас нет прав для использования системы спавна!</b></color>";
                return false;
            }
            if (string.IsNullOrWhiteSpace(pointName))
            {
                response = "<color=red><b>⚠ Укажите <i>название</i> точки!</b></color>";
                return false;
            }

            int max = GetLimit(player);
            if (!playerPoints.ContainsKey(player.UserId))
                playerPoints[player.UserId] = new Dictionary<string, SerializableVector3>();

            if (playerPoints[player.UserId].ContainsKey(pointName))
            {
                response = string.Format("<color=yellow><b>⚠ Точка с именем <i>\"{0}\"</i> уже существует!</b></color>", pointName);
                return false;
            }
            if (playerPoints[player.UserId].Count >= max)
            {
                response = string.Format("<color=red><b>❗ Вы не можете иметь больше <i>{0}</i> точек!</b></color>", max);
                return false;
            }
            playerPoints[player.UserId][pointName] = new SerializableVector3(player.Position);
            response = string.Format("<color=lime><b>✔ Точка <i>\"{0}\"</i> успешно создана!</b>\n<color=orange>({1:0.00}, {2:0.00}, {3:0.00})</color></color>",
                pointName, player.Position.x, player.Position.y, player.Position.z);
            SavePoints();
            return true;
        }

        public bool CmdTp(Player player, string pointName, out string response)
        {
            if (!HasPermission(player))
            {
                response = "<color=red><b>❌ У вас нет прав для использования системы спавна!</b></color>";
                return false;
            }
            if (string.IsNullOrWhiteSpace(pointName))
            {
                response = "<color=red><b>⚠ Укажите <i>название</i> точки!</b></color>";
                return false;
            }
            Dictionary<string, SerializableVector3> dict;
            if (playerPoints.TryGetValue(player.UserId, out dict) && dict.TryGetValue(pointName, out SerializableVector3 pos))
            {
                player.Position = pos.ToVector3();
                response = string.Format("<color=lime><b>✔ Телепорт в <i>\"{0}\"</i>!</b>\n<color=orange>({1:0.00}, {2:0.00}, {3:0.00})</color></color>",
                    pointName, pos.x, pos.y, pos.z);
                return true;
            }
            else
            {
                response = string.Format("<color=red><b>❗ Точка <i>\"{0}\"</i> не найдена!</b></color>", pointName);
                return false;
            }
        }

        public bool CmdDel(Player player, string pointName, out string response)
        {
            if (!HasPermission(player))
            {
                response = "<color=red><b>❌ У вас нет прав для использования системы спавна!</b></color>";
                return false;
            }
            if (string.IsNullOrWhiteSpace(pointName))
            {
                response = "<color=red><b>⚠ Укажите <i>название</i> точки!</b></color>";
                return false;
            }
            Dictionary<string, SerializableVector3> dict;
            if (playerPoints.TryGetValue(player.UserId, out dict) && dict.Remove(pointName))
            {
                response = string.Format("<color=yellow><b>🗑 Точка <i>\"{0}\"</i> удалена!</b></color>", pointName);
                SavePoints();
                return true;
            }
            else
            {
                response = string.Format("<color=red><b>❗ Точка <i>\"{0}\"</i> не найдена!</b></color>", pointName);
                return false;
            }
        }

        public bool CmdList(Player player, out string response)
        {
            if (!HasPermission(player))
            {
                response = "<color=red><b>❌ У вас нет прав для использования системы спавна!</b></color>";
                return false;
            }
            Dictionary<string, SerializableVector3> dict;
            if (!playerPoints.TryGetValue(player.UserId, out dict) || dict.Count == 0)
            {
                response = "<color=yellow><b>ℹ У вас нет сохранённых точек.</b></color>";
                return true;
            }
            response = "<color=cyan><b>📍 Ваши точки:</b></color>\n";
            foreach (KeyValuePair<string, SerializableVector3> kvp in dict)
                response += string.Format("<color=white>- <b>{0}</b>: <color=orange>({1:0.0}, {2:0.0}, {3:0.0})</color></color>\n",
                    kvp.Key, kvp.Value.x, kvp.Value.y, kvp.Value.z);
            return true;
        }

        private bool HasPermission(Player player)
        {
            foreach (KeyValuePair<string, int> perm in Config.RolePermissions)
                if (player.CheckPermission(perm.Key)) return true;
            if (Config.AllowDefaultPlayers)
                return true;
            return false;
        }

        private int GetLimit(Player player)
        {
            int max = Config.DefaultLimit;
            foreach (KeyValuePair<string, int> kvp in Config.RolePermissions)
                if (player.CheckPermission(kvp.Key) && kvp.Value > max) max = kvp.Value;
            if (max <= 0) max = 1;
            return max;
        }

        private void LoadPoints()
        {
            if (!File.Exists(SavePath))
            {
                playerPoints.Clear();
                return;
            }
            try
            {
                string json = File.ReadAllText(SavePath);
                playerPoints = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, SerializableVector3>>>(json)
                    ?? new Dictionary<string, Dictionary<string, SerializableVector3>>();
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка чтения точек спавна: " + ex);
                playerPoints = new Dictionary<string, Dictionary<string, SerializableVector3>>();
            }
        }

        private void SavePoints()
        {
            try
            {
                File.WriteAllText(SavePath, JsonConvert.SerializeObject(playerPoints, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка сохранения точек спавна: " + ex);
            }
        }
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;
        public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }
}
