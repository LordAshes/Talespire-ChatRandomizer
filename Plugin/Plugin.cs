using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Talespire;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid)]
    [BepInDependency(LordAshes.ChatServicePlugin.Guid)]
    [BepInDependency(LordAshes.GUIMenuPlugin.Guid)]
    [BepInDependency("org.lordashes.plugins.chatwhisper", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class ChatRandomizerPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Chat Randomzier Plug-In";              
        public const string Guid = "org.lordashes.plugins.chatrandomizer";
        public const string Version = "3.4.0.0";

        // Configuration
        static ConfigEntry<bool> possibilities;
        static ConfigEntry<bool> showAllNames;
        static ConfigEntry<bool> resultSameLine;
        static ConfigEntry<bool> totalSameLine;
        static ConfigEntry<string> rollWord;
        static ConfigEntry<string> resultWord;
        static ConfigEntry<bool> useHeader;
        static ConfigEntry<GUIMenuPlugin.MenuStyle> menuStyle;
        static ConfigEntry<string> menuTextColor;

        // Variables
        static Dictionary<string, string> namedRolls = new Dictionary<string, string>();
        static System.Random random = new System.Random();
        static GUIMenuPlugin.GuiMenu menu = null;

        void Awake()
        {
            UnityEngine.Debug.Log("Chat Randomizer Plugin: "+this.GetType().AssemblyQualifiedName+" Active.");

            possibilities = Config.Bind("Settings", "Show Roll Possibilities", true);
            showAllNames = Config.Bind("Settings", "Show All Names Instead Of Just First", true);
            resultSameLine = Config.Bind("Settings", "Show Result On Same Line", false);
            totalSameLine = Config.Bind("Settings", "Show Total On Same Line", false);
            rollWord = Config.Bind("Settings", "Default Roll Prefix", "Roll:");
            resultWord = Config.Bind("Settings", "Default Result Prefix", "Result:");
            useHeader = Config.Bind("Settings", "Use Chat Header For First Name", false);
            menuStyle = Config.Bind("Settings", "Menu Style", GUIMenuPlugin.MenuStyle.side);
            menuTextColor = Config.Bind("Settings", "Menu Text Color", "00FF00FF");

            ChatServicePlugin.ChatMessageService.AddHandler("/crt", (m, s, r) => CustomRollHandler(m, s, r, false, true));
            ChatServicePlugin.ChatMessageService.AddHandler("/cnrt", (m, s, r) => CustomRollHandler(m, s, r, true, true));
            ChatServicePlugin.ChatMessageService.AddHandler("/cr", (m,s,r) => CustomRollHandler(m,s,r,false));
            ChatServicePlugin.ChatMessageService.AddHandler("/cnr", (m,s,r) => CustomRollHandler(m,s,r,true));

            try
            {
                namedRolls = JsonConvert.DeserializeObject<Dictionary<string, string>>(FileAccessPlugin.File.ReadAllText("custom_dice.json"));
                foreach (string name in namedRolls.Keys)
                {
                    Debug.Log("Chat Randomizer Plugin: Adding '"+name+"' Custom Dice Sequences");
                }
            }
            catch(Exception x)
            {
                Debug.Log("Chat Randomizer Plugin: No Valid Preconfigued Custom Dice Sequences Found");
                Debug.LogException(x);
            }

            Utility.PostOnMainPage(this.GetType());
        }

        void OnGUI()
        {
            if (menu != null) { menu.Draw(); }
        }

        private string CustomRollHandler(string message, string sender, SourceRole role, bool named = false, bool tally = false)
        {
            Debug.Log("Chat Randomizer Plugin: Sender = '" + sender + "'");

            // Process 
            Dictionary<PlayerGuid, PlayerInfo> players = CampaignSessionManager.PlayersInfo;
            if(!players[LocalPlayer.Id].Rights.CanGm)
            {
                Debug.Log("Chat Randomizer Plugin: Not GM. Ignore message.");
                return null;
            }

            if (message.Contains("<")) { message = message.Substring(0,message.IndexOf("<")); }

            Debug.Log("Chat Randomizer Plugin: Processing '" + message + "'");

            // Scan for prompts
            if (message.Contains("?"))
            { 
                string prompt = message.Substring(message.IndexOf("?")).Trim();
                if (prompt.Contains("/")) { prompt = prompt.Substring(0, prompt.IndexOf("/")); }

                Debug.Log("Chat Randomizer Plugin: Processing Prompt '"+prompt+"'");

                string[] items = prompt.Substring(1).Trim().Split(' ');
                List<GUIMenuPlugin.MenuSelection> selections = new List<GUIMenuPlugin.MenuSelection>();
                Color textColor = Color.green;
                ColorUtility.TryParseHtmlString(menuTextColor.Value, out textColor);
                for (int i = 0; i < items.Length; i += 2)
                {
                    Debug.Log("Chat Randomizer Plugin: Adding Option '" + items[i] + "' => '"+items[i+1]+"'");
                    selections.Add(new GUIMenuPlugin.MenuSelection()
                    {
                        color = textColor,
                        gmOnly = false,
                        icon = FileAccessPlugin.Image.LoadTexture("Skills.png"),
                        title = items[i],
                        selection = items[i + 1]
                    });
                }
                menu = new GUIMenuPlugin.GuiMenu(new GUIMenuPlugin.MenuNode[] { new GUIMenuPlugin.MenuNode("Root", selections.ToArray(), menuStyle.Value) });
                menu.Open("Root", (selection) =>
                {
                    Debug.Log("Chat Randomizer Plugin: Selection '"+selection+"' Made");
                    menu.Close();
                    menu = null;
                    try
                    {
                        selection = selection.Replace("\\", "");
                        message = message.Replace(prompt, selection);
                        message = message.Replace("/ /", "/");
                        if (message.Substring(message.IndexOf(" ") + 1, 1) == "/") { message = message.Substring(0, message.IndexOf(" ")) + message.Substring(message.IndexOf(" ") + 2); }
                        Debug.Log("Chat Randomizer Plugin: Rethrowing Message '" + message + "'");
                        ChatManager.SendChatMessage(message, LocalPlayer.Id.Value);
                    }
                    catch (Exception) {; }
                });
                return null;
            }

            message = message.Substring(message.IndexOf(" ")).Trim();
            foreach(KeyValuePair<string,string> replacement in namedRolls)
            {
                Debug.Log("Chat Randomizer Plugin: Replacing '" + replacement.Key + "' With '" + replacement.Value + "' In '" + message + "'");
                message = message.Replace(replacement.Key, replacement.Value);
            }

            string result = "";
            int total = 0;
            bool first = true;

            foreach (string randomization in SmartSplit(message,"/", false))
            {
                Debug.Log("Chat Randomizer Plugin: Randomizer Component Content = '" + randomization.Trim() + "'");

                if (randomization.Trim().StartsWith("\""))
                {
                    string part = randomization.Substring(1);
                    if (part.EndsWith("\"")) { part = part.Substring(0, part.Length - 1); }
                    result += part;
                }
                else if (randomization.Trim() != "")
                {
                    string[] items = randomization.Trim().Split(' ');

                    // Roll Name 
                    if(named && (first || showAllNames.Value))
                    {
                        if (!first) { result += "\r\n"; }
                        if (first && useHeader.Value)
                        {
                            result += "[" + items[0] + " <size=10>" + CampaignSessionManager.GetPlayerName(LocalPlayer.Id) + "</size>]";
                        }
                        else
                        {
                            result += items[0];
                        }
                        items = items.Skip(1).ToArray();
                    }
                    else if(!named && (first || showAllNames.Value))
                    {
                        if (!first) { result += "\r\n"; }
                        result += rollWord.Value + " ";
                    }

                    if (first && resultSameLine.Value && !useHeader.Value && named)
                    {
                        result += " = ";
                    }

                    // Roll Options
                    if (possibilities.Value)
                    {
                        result += " (" + String.Join(" ", items) + ")";
                    }

                    int pick = random.Next(0, items.Length);

                    // Roll Result
                    if (resultSameLine.Value)
                    {
                        result += items[pick]+" ";
                    }
                    else
                    {
                        result += "\r\n" + resultWord.Value + " " + items[pick]+" ";
                    }

                    // Total Result
                    if(tally)
                    {
                        int itemValue = 0;
                        int.TryParse(items[pick], out itemValue);
                        total += itemValue;
                    }
                }

                first = false;
            }

            if (!tally)
            {
                ChatManager.SendChatMessage(result.Substring(0, result.Length - 1),LocalPlayer.Id.Value);
            }
            else
            {
                if (totalSameLine.Value)
                {
                    ChatManager.SendChatMessage(result + " = " + total, LocalPlayer.Id.Value);
                }
                else
                {
                    ChatManager.SendChatMessage(result + "\r\n-----------------------------------\r\nTotal: " + total, LocalPlayer.Id.Value);
                }
            }

            return null;
        }

        static string[] SmartSplit(string source, string split, bool removeEscape = false)
        {
            List<string> results = new List<string>();
            bool escaped = false;
            for (int p = 0; p <= source.Length - split.Length; p++)
            {
                if (source.Substring(p, 1) == "\"") { escaped = !escaped; }
                if (source.Substring(p, split.Length) == split && !escaped)
                {
                    string item = source.Substring(0, p);
                    if (removeEscape && item.StartsWith("\"") && item.EndsWith("\""))
                    {
                        item = item.Substring(1);
                        item = item.Substring(0, item.Length - 1);
                    }
                    results.Add(item);
                    source = source.Substring(p + split.Length);
                    p = -1;
                }
            }
            results.Add(source);
            return results.ToArray();
        }
    }
}
