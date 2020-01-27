using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Kits
{
    public class Kits : RocketPlugin<KitsConfiguration>
    {
        public static Kits Instance = null;
        public static SqlSugarClient Db;
        public static Dictionary<string, DateTime> GlobalCooldown = new Dictionary<string, DateTime>();

        public void GiveKit(UnturnedPlayer player, Kit kit)
        {

            foreach (KitItem item in kit.Items)
            {
                try
                {
                    if (!player.GiveItem(item.ItemId, item.Amount))
                    {
                        Logger.Log(Kits.Instance.Translations.Instance.Translate("command_kit_failed_giving_item", item.ItemId, item.Amount, player.CharacterName));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed giving item " + item.ItemId + " to player");
                }
            }
            if (kit.XP.HasValue && kit.XP != 0)
            {
                player.Experience += kit.XP.Value;
                UnturnedChat.Say(player, Kits.Instance.Translations.Instance.Translate("command_kit_xp", kit.XP.Value, kit.Name));
            }

            if (kit.Vehicle.HasValue)
            {
                try
                {
                    player.GiveVehicle(kit.Vehicle.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed giving vehicle " + kit.Vehicle.Value + " to player");
                }
            }


        }

        public void GiveKit(UnturnedPlayer player, string KitName)
        {
            Kit kit = Instance.Configuration.Instance.Kits.Where(k => k.Name.ToLower() == KitName).FirstOrDefault();
            if (kit == null)
            {
                Logger.LogWarning("Kit " + KitName + " not found.");
            }
            else
            {
                GiveKit(player, kit);
            }

        }

        protected override void Load()
        {
            Instance = this;
            Db = PlayerLibrary.DbMySQL.DbContext();

            if (PlayerLibrary.DbMySQL.CheckTable(Configuration.Instance.TableName))
            {
                PlayerLibrary.DbMySQL.CreateTables("CREATE TABLE `" + Configuration.Instance.TableName + "` ( `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, `kit` int(10) UNSIGNED NOT NULL, `player` int(10) UNSIGNED NOT NULL, `count` smallint(5) UNSIGNED DEFAULT NULL, `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`id`), KEY `kits_player` USING HASH (`player`, `kit`), CONSTRAINT `kits_player` FOREIGN KEY (`player`) REFERENCES `Players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE ) ENGINE = InnoDB CHARSET = utf8;");
            }

            if (IsDependencyLoaded("Uconomy"))
            {
                Logger.Log("Optional dependency Uconomy is present.");
            }
            else
            {
                Logger.Log("Optional dependency Uconomy is not present.");
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                    {"command_kit_no_start","礼包 {0} 还需要 {1} 才能领取"},
                    {"command_kit_is_ned","礼包 {0} 已经结束领取了"},
                    {"command_kit_invalid_parameter","无效的命令!,示例 /kit <礼包名字>"},
                    {"command_kit_not_found","指定的礼包没有找到"},
                    {"command_kit_count","超过 {0} 礼包的领取次数"},
                    {"command_kit_no_permissions","没有足够的权限去领取对应的礼包"},
                    {"command_kit_cooldown_command","必须等待 {0} 秒才能再使用这个命令"},
                    {"command_kit_cooldown_kit","必须等待 {0} 才能再领取这个礼包"},
                    {"command_kit_failed_giving_item","无法将: ({0},{1})  给 : {2}"},
                    {"command_kit_success","成功领取: {0} 礼包" },
                    {"command_kits","可领取以下礼包: {0}" },
                    {"command_kit_no_money","无法购买 {2} 礼包. 至少需要: {0} {1}." },
                    {"command_kit_money","获得 {0} {1} ,来自 {2} 礼包." },
                    {"command_kit_xp","获得 {0} 经验,来自 {1} 礼包." }
                };
            }
        }
    }
}
