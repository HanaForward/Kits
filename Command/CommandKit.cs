using fr34kyn01535.Kits.Model;
using PlayerLibrary;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Kits
{
    public class CommandKit : IRocketCommand
    {
        public string Help
        {
            get { return "Gives you a kit"; }
        }

        public string Name
        {
            get { return "kit"; }
        }

        public string Syntax
        {
            get { return "<kit>"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "kits.kit" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {

            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;

            Kit kit = Kits.Instance.Configuration.Instance.Kits.Where(k => k.Name.ToLower() == command[0].ToLower()).FirstOrDefault();
            if (kit == null)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_not_found"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            bool hasPermissions = caller.HasPermission("kit.*") | caller.HasPermission("kit." + kit.Name.ToLower());

            if (!hasPermissions)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_no_permissions"));
                throw new NoPermissionsForCommandException(caller, this);
            }


            KeyValuePair<string, DateTime> globalCooldown = Kits.GlobalCooldown.Where(k => k.Key == caller.ToString()).FirstOrDefault();
            if (!globalCooldown.Equals(default(KeyValuePair<string, DateTime>)))
            {
                double globalCooldownSeconds = (DateTime.Now - globalCooldown.Value).TotalSeconds;
                if (globalCooldownSeconds < Kits.Instance.Configuration.Instance.GlobalCooldown)
                {
                    UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_cooldown_command", (int)(Kits.Instance.Configuration.Instance.GlobalCooldown - globalCooldownSeconds)));
                    return;
                }
            }
            if (kit.Conditions.Count > 0 && !CheckKitTime(player, kit))
                return;


            PlayerInfo players = PlayerLibrary.PlayerLibrary.GetPlayerByCSteam(player.CSteamID.m_SteamID);
            Model.Kits kitTab = Kits.Db.Queryable<Model.Kits>().Where(it => it.kit == kit.kit && it.player == players.player.Id).First();
            if (kitTab == null)
            {
                kitTab = new Model.Kits(players.player.Id, kit.kit);
                if (NotEnoughtMoney(player, kit))
                {
                    return;
                }
                kitTab = Kits.Db.Insertable(kitTab).ExecuteReturnEntity();

            }
            else
            {
                if (DateTime.Now.Subtract(kitTab.updated_at).TotalSeconds < kit.Cooldown)
                {
                    TimeSpan timeSpan = kitTab.updated_at.AddSeconds(kit.Cooldown.Value).Subtract(DateTime.Now);
                    string timer = GetTimeSpan(timeSpan);
                    UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_cooldown_kit", timer));
                    return;
                }

                if (kit.Count.HasValue && kit.Count.Value <= kitTab.count)
                {
                    UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_count", kit.Name));
                    return;
                }

                if (NotEnoughtMoney(player, kit))
                {
                    return;
                }


            }



            kitTab.count++;
            kitTab.updated_at = DateTime.Now;

            Kits.Db.Updateable(kitTab).ExecuteCommand();

            Kits.Instance.GiveKit(player, kit);
            UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_success", kit.Name));

            if (Kits.GlobalCooldown.ContainsKey(caller.ToString()))
            {
                Kits.GlobalCooldown[caller.ToString()] = DateTime.Now;
            }
            else
            {
                Kits.GlobalCooldown.Add(caller.ToString(), DateTime.Now);
            }

        }


        private bool NotEnoughtMoney(UnturnedPlayer player, Kit kit)
        {
            if (kit.Money.HasValue && kit.Money.Value != 0)
            {
                if ((Uconomy.Uconomy.Instance.Database.GetBalance(player.CSteamID.ToString()) + kit.Money.Value) < 0)
                {
                    UnturnedChat.Say(player, Kits.Instance.Translations.Instance.Translate("command_kit_no_money", Math.Abs(kit.Money.Value), Uconomy.Uconomy.Instance.Configuration.Instance.MoneyName, kit.Name));
                    return true;
                }
                else
                {
                    UnturnedChat.Say(player, Kits.Instance.Translations.Instance.Translate("command_kit_money", kit.Money.Value, Uconomy.Uconomy.Instance.Configuration.Instance.MoneyName, kit.Name));
                }
                Uconomy.Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), kit.Money.Value);
            }
            return false;
        }

        private bool CheckKitTime(UnturnedPlayer player, Kit kit)
        {
            string time = "";
            bool no_start = true;

            foreach (Condition condition in kit.Conditions)
            {
                Time Start_Time = condition.GetStartTime();
                Time End_Time = condition.GetEndTime();
                if (End_Time == null)
                {
                    return true;
                }

#if DEBUG
                Logger.Log(string.Format("Start_Time : {0}, End_Time: {1}", DateTime.Now.CompareTo(Start_Time.dateTime).ToString(), DateTime.Now.CompareTo(End_Time.dateTime).ToString()));
#endif
                if (DateTime.Now.CompareTo(Start_Time.dateTime) < 0)
                {
                    //还没开始
                    no_start = true;
                    time = GetTimeSpan(Start_Time.dateTime.Subtract(DateTime.Now));
                }
                else
                {
                    //已经开始
                    if (DateTime.Now.CompareTo(End_Time.dateTime) < 0)
                    {
                        //还没结束
                        return true;
                    }
                    else
                    {
                        //已经结束
                        no_start = false;
                    }
                }
            }
            if (no_start)
            {
                UnturnedChat.Say(player, Kits.Instance.Translations.Instance.Translate("command_kit_no_start", kit.Name, time));
            }
            else
            {
                UnturnedChat.Say(player, Kits.Instance.Translations.Instance.Translate("command_kit_is_ned", kit.Name));

            }
            return false;


        }
        public string GetTimeSpan(TimeSpan timeSpan)
        {
            string timer;
            if (timeSpan.Days > 0)
            {
                timer = string.Format("{0:%d} 天 {0:%h} 时 {0:%m} 分 {0:%s} 秒", timeSpan);
            }
            else if (timeSpan.Hours > 0)
            {
                timer = string.Format("{0:%h} 时 {0:%m} 分 {0:%s} 秒", timeSpan);
            }
            else if (timeSpan.Minutes > 0)
            {
                timer = string.Format("{0:%m} 分 {0:%s} 秒", timeSpan);
            }
            else
            {
                timer = string.Format("{0:%s} 秒", timeSpan);
            }
            return timer;


        }
    }
}
