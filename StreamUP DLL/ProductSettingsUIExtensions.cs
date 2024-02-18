using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Label = System.Windows.Forms.Label;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    // Settings definition
    // Currently support for 11 different settings types:
    // - StreamUpSettingType.Action
    // - StreamUpSettingType.Boolean
    // - StreamUpSettingType.Colour
    // - StreamUpSettingType.Double
    // - StreamUpSettingType.Dropdown
    // - StreamUpSettingType.Heading
    // - StreamUpSettingType.Integer
    // - StreamUpSettingType.Label
    // - StreamUpSettingType.Reward
    // - StreamUpSettingType.Ruler
    // - StreamUpSettingType.Secret
    // - StreamUpSettingType.Spacer
    // - StreamUpSettingType.String
    //
    // Name = The variable name, Description = The UI label, Type = See above, Default = The default value as a string.

    public static class ProductSettingsUIExtensions {
        public static bool savePressed = false;
        private static readonly string iconString = "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39/f39/f39/f39/f3+fn5/79/v////////79/ufn59/f39/f39/f39/f39/f3+Li4qampgwMDAQEAp2WQv/3a//0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAsKBcO6Uv/3a//0av/0av/0av/0av/0av/0av/1au/mZENAHQAAAFlZWff39/////////////////////////////////////////////////////////////////f391lZWQAAAENAHe/mZP/1av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAgIBL21UP/4bP/0av/0av/0av/0av/0av/0av/0av/3bLCpSwoKBAsLDLKysv///////////////////////////////////////////////////////////////7KysgsLDAoKBLCpSv/3bP/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAJONPv/4bP/0av/0av/0av/0av/0av/0av/0av/1avbtZ1VSJAAAAEVFRfDw8P////////////////////////////////////////////////////////Dw8EVFRQAAAFVSJPbtZ//1av/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAADg2GOXcYP/3a//0av/0av/0av/0av/0av/0av/0av/3a8G6UhMSCAUFBZ+fn////////////////////////////////////////////////////////5+fnwUFBRMSCMG6Uv/3a//0av/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAAEBAFdUJdzSXP71a//4bP/4bP/4bP/4bP/4bP/4bP/4bPz1a2xoLgAAADU1Nefn5////////////////////////////////////////////////+fn5zU1NQAAAGxoLvz1a//4bP/4bP/4bP/4bP/4bP/4bP/1av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAAAAAAEBACYkEGllLYiCOYqEOoqEOoqEOoqEOoqEOoqEOoyGO2VgKwUFAgEBAYuLi////////////////////////////////////////////////4uLiwEBAQUFAmVgK4yGO4qEOoqEOoqEOoqEOomDOZmSQOrgYv/1av/0av/3a8O6UgsKBQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACYmJtvb2////////////////////////////////////////9vb2yYmJgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEBAZ2WQv/4bP/0av/3a8O6UgsKBQAAAAAAAAMDAjw5Gk9MIk5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5LIU1JIRkYCwAAAHd3d/7+/v////////////////////////////////7+/nd3dwAAABkYC01JIU5LIU5KIU5KIU5KIU5KIU5KIU1KIV1ZKNPKWf/2a//0av/3a8O7UgsKBQAAAAAAAAsKBcC3Uf7zavvwafvwafvwafvwafvwafvwafvwafvwafvwafvwafvwaf3yapCKPQEBABoaGs3Nzf///////////////////////////////83NzRoaGgEBAJCKPf3yavvwafvwafvwafvwafvwafvwafvwafzyaf/1a//0av/0av/4bLOsSwYFAwAAAAAAAAsKBcO6Uv/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a+ngYjk3GQAAAGNjY/r6+v////////////////////////r6+mNjYwAAADk3GengYv/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av71anBrMAAAAAAAAAAAAAsKBcO6Uv/3a//0av/0av/0av/1av/1av/1av/1av/1av/1av/1av/1av/1av/5bKWfRgcGAhAQEL29vf///////////////////////729vRAQEAYGAqWfRv/5bP/1av/1av/1av/1av/1av/1av/1av/1av/1av/1a//3a//3a7SsTBYVCQAAAAAAAAAAAAsKBcO6Uv/3a//0av/0av70avHnZeziY+ziY+ziY+ziY+ziY+ziY+ziYuziYu3iY+DWXj88GwAAAFBQUPT09P////////////////T09FBQUAAAAD88G9/VXuziY+vhYuvhYuvhYuvhYuvhYuvhYuvhYuvhYuvhYurgYtjPW4yGOxwbDAAAAAAAAAAAAAAAAAsKBcO6Uv/3a//0av/3a8O7Ujo4GS0qEy0rEy0rEy0rEy0rEy0rEy0rEy0rEy0rEy4sExYVCQAAAAkJCaurq////////////////6urqwkJCQAAABUUCS0rFCwqEywqEywqEywqEywqEywpEywpEywpEyspEywpEyspEhkYCwMCAQAAAAAAAAAAAAAAAAAAAAsKBcO6Uv/3a//0av/4bKWeRQcGAwEBAAEBAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQMDAQMDAQAAAD8/P+zs7P///////+zs7D8/PwAAAAMDAQMDAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQMDAQUEAgUFAgQEAgAAAAAAAAAAAAsKBcO6Uv/3a//0av/1avTpZry0T6+nSbCoSrCoSrCoSrCoSrCoSrCpSrCpSrCpSrGpSrKqS5iSQBcVCgIDA5eXl////////5eXlwIDAxcWCpmSQLOrS7KqSrKqSrKqSrKqSrKqSrKqSrKqSrKrSrKrSrKrS7OrS7OrS7OrS7OrS7WtTImDOggHAwAAAAAAAAsKBcO6Uv/3a//0av/0av/1av/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP31a3FsMAAAAC8vL+Tk5OTk5C8vLwAAAHFsMP31a//4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/8bcO+UwsKBQAAAAAAAAoJBMC4Uf/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a9bOWiIhDwAAAISEhISEhAAAACIhD9bOWv/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/4a8K6UgoKBQAAAAAAAAICAaSeRv/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a4WAOAAAABQUFRQUFQAAAIV/Of/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/4bKagRgMDAQAAAAAAAAAAAFNPI/XsZ//1a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a+PaXzAuFQAAAAAAADAuFePaX//2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/1a/btZ1VRJAAAAAAAAAAAAAAAAAcHA4aAOfbsZ//4bP/4bP/3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//6bZuWQgUEAgUEApuWQv/6bf/3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//4bP/4bPbtZ4iCOggIBAAAAAAAAAAAAAAAAAAAAAgIBFNPJKWeRsG5UcO7UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsS8UrOsTC4sFS4sFbOsTMS8UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO7UsG5UaafRlRQJAkIBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDAQoJBAsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQwLBQcHAwcHAwwLBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQoJBAMDAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgAAAAgAAAAQAAAAAEACAAAAAAAAAQAAAAAAAAAAAAAAAEAAAAAAAAAAAAAZltzAD4sVAD/+m0Ajog8AFNTUwBWPXQAa02RAL6I/wC/iP8A0NDRABscHQDCi/8AMiNDAH95NADDjv8Ab29wAAsIDwCwqEoAg303AF9EgAAPDgYAISEjAMma/wC3r00AiIQ6ABISEgCJhDoA//ZrAF1ZJwBSO28AkGfDAD4sVQAwLhQAVDxyANCm/wBWP3IAqqRIAFlFcgAcHB4ACgoEAAoHDQDGvVMA17L/APXrZgCEgoUAsH7sALF+7ABrZy0ADw4HAJ1w1QCyf+8AhYA4ABIRBwD88mkAiYM4AHNuMABROm0A//lsAGVlZgClnUYA08tZALyG+wADAwIA48r/AL2H/gBXP3MAamF1AKp55wDCuVEAgl2wABsbHABaRXMAODUYAAsKBQDq1v8AXUN/ACAXKwAlIxAAVFEjAK6urgBgRIIA49lfABIRCAC2rkwA8eL/ANDHVwD+9WoA//VqAI9mwgD//G0AGRgLAKd34gAtID0AvYf/AGtMkQD47v8AHRsLAL6H/wDYzloABgYDABoaGgDBiv8Agl2xABsbHQCDXbEAIB8OAAkGDAAeFikAIyIOAN7VXQD++v8Agnw3ACEXLAAODQYAsapKALKqSgCyf/EAysNVAO7u7gAUEwYA5txgABINGACJgzoA/vVrAFxYJwD/9WsAZ0mJANPKWADg4OAAPz9AADY0FwAJCQQA9OpmANex/wD16mYAHxYqAFxKcgCYkUAAOzgaALKqSwA+OxoAdFOeAPrxaQBYVCUA3b3/AHFtMABkSIcAZkmKAKR13gD/+GwAGBcKAF9bKAC8hv4AvYb+AOTk5ADDu1EAZmIrAB0VKAAeFSgA+/v7ADo3GABdSnMADQwFAJuUQQCampoASzZlAE03aACHgjkAiII5ABQTCABkSIgA//RqAAEBAAD/+20A39/fAEhFHgC8hv8A1s1aAL2G/wDXzVoAwIn/AMO7UgA4NhYAxLtSANrUXQCtfOsAHhUpAK586wAjIQ4Av7/AAGllLABqZSwACwsMAA0MBgCxqUoAc1KdAPjtaAAODQkA////AEs2ZgAPDgwAh4eHAHVToACLY70A2traAGFiYgAUEwkAZUiJAIuFOgD/92sAFxYJANPJWAABAQEApZ5FAAMCBABWPXIAa2V0ACIgDAB/ejUArnzsAMa+UwBva3QAx75TADw6GgAPDwcAYE9yABAPBwAQCxYAn3LYAGJVcgBjR4cAcWwwAGZIigD/92wA1cxZAPX19QDCulEA0NDQAGxldQAvLzAAw7pRAGVhKwBmYSsAOTYYAG9rdQCEg4YAh2C2AGBPcwARCxcAYlVzAP3zagCKhDkALSsTABMTFAC4g/kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHrIq9DQ0NDQ0NDQ0NDQlF3kTOnQ54gAAAAAAAAAAAB695oJCQkJCQkJCQkJCQlm2GwutQk+OWxxcU161QAAAKZBsbGxsbGxsbGxsV5esWIin7xesbF1ury8ujKn1wAAkwmxsbGxsbGxsbH/B8xiYiKfvF6xsWK1tbViCTMNAADQCbGxsbGxsbGxXtyIBggIIp+8XrGxXFlZH0SzCRQAANAJsbGxsbGxsbFevLtCDw8kn7xesZkeaxEpUV4J0AAA0AmxsbGxsbGxsV7cu0gXFyafvF6xseVGZ2mVXgnQAADQCbGxsbGxsbGxXty7oiMjiZ+8XrGxYgkJCV6xCdAAANAJsbGxsbGxsbFe3Lv4K4binrxesbGxsbGxsbEJ0AAAfwwJCQkJCQkJCbUvu/qRkea7L7UJCQkJCQkJCQx/AAACy8TExMTExMTEjl/5AUBAAflfjsTExMTExMTEyyAAAErPqqqqqqqqqqrPUwBDS0tDAFPPqqqqqqqqqqrPSgAA6LLU1NSA67S0tGPd2tlVVe/aKmO0tLTrgNTU1LLoAAAbrtLS0tK20fw3/A543mBg9XgO/Df80bbS0tLSrhsAAHuWrKysrDAA/idoC2Utb2/2ZQtoJ/4AMKysrKyWewAAe5asrKzSuDXOgYGBr3fHx3evgYGBzjW40qysrJZ7AAA0lqysrKw2wEcKx8fHx8fHx8fHx+5HwDasrKyslnsAAJCPWKysrNI9YTugx8fHx8fHx8egO2E90qysrKyWewAAcoosrKysrH7bGr7Hx8fHx8fHx74a236srKx+rJZ7AAAAhI19HR0dmLCtBezHx8fHx8fsBa2wmB0dHcMclnsAAGrg9ElJSUlJoSHJUMfHx8fHx1DJIaFJSUmDpByWewAAE8WFhYWFhYUsUreCd8fHx8d3grdSLIWFhYc2rNKSAAB7OlgcOjo6OjpaJcalx8fHx6XGJVo6Ojo6OpY6Vk4AAHuWWG6Mc3Nzw3TWbfCbx8eb8L08wxISEhISEgT9AAAAe5YccMIVFRUVFeOjP8rHx8o/o+MxMTEx4eExKIRkAAB7llh57UVFRe3t7fFPFs3NFk+cnJycnJycnLi4378AAKiWrH7Slurq6urqA3bTEBDTdgPS0tLS0tLS0tKuGQAA8lesrKysrKysrKys+zjBwTj7rKysrKysrKysrHydAACXVFeWlpaWlpaWlpYDuU5OuQOWlpaWlpaWlpZ8GJcAAACX8ql7e3t7e3t7e3t7i4t7e3t7e3t7e3t7qfNbAAAAAAAAAAAAAAAAAAAAAACtrQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKAAAABAAAAAgAAAAAQAIAAAAAAAAAQAAAAAAAAAAAAAAAQAAAAAAAAAAAABrTI4Ak3ezAGJeLAC+iP8Av4j/APPoZQBoZCwADw4GAPjvaACdcNQAnImzACMZLwAoJxEA2dnaAKCZQgDn3WAAo5xCAP/2awC4g/oAMC4UAHdyMgC4tL4AubS+AH5+eQCDXrIAUk4iAE1LNAD262YAIxkwAM3EVgBYVSUAWVUlAP3yaQCMY8EAXFglAI5kxAD/+WwA1MtZAKd34QB4cTkAgl2wAHx1PABnYysA3tVcAJtv0wD57mcAiWezAFlVJgC1gfYAtoH2AOrgYgC6hfkA6+BiAHZxMQBpS44ALSA9AHhxOgDXzloAvof/AMGK/wAxIkMA9OplADs4GQCvfesA3tVdAGpmLAD37mgA+e5oALJ/8QDMw1UA5txgAC8tFABdWi0Ad3EyAElGHwAFBAcAQzBbADY0FwAJCQQAmW3PAMi/UwBQTjcAdFOeAFdUJQBYVCUA/PFpAJ+YQwA9PC8Az8ZWAE85bQCMhjsA4ODhAH5aqgC8hv4AQS9ZAEIvWQDd1FwAwcHFAMnCVACMjIwAKB03AG9sQABmSYsA0slXAHVwMQBALlcAQS5XALyG/wDNzc4Appq1APb29gBDL1oAwIn/AJZrywD06WUAaWUsAGplLAD///8AiGC9APnwaAD/92sAZ0mMAAEBAQBBLlgAVj51AHpzOwAyMSMA9exmAPbsZgBWUyUAzsVWACspFQAsKRUA6N5hAERAIwC4g/gAuYT7ADEvFQBLSCAA7+VkAGxrYwAyMSQAMSNCAJWPPgB9djwA9uxnAP3+/gAkGjEA5NpfAFpWJgC3g/kA0chXAHRvMQC4g/kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB7HXxfX19fcDiUTWUAAAAADAoTl5eaMoldZ15QemdqTGuIbGxsBClyGTdxRS0nQI9fl2xsbAU3Ly83cS19UzFgYDQFBQU8AQICATwFOwU0YD13IiIiJFoLC1okIiIidz2EfpEqOSiHbm6HKDkqkX6FIHglY0lmGxYXG2ZJYyV4IIIJeSxSYlxvb1xiUiwSRCANUQY+W2R2dnZ2ZFtzgS4gCEsHdCNYDnZ2DlgjK5CAHxqVhhA1DxiTkxgPM0eMQU4wkleZFUJ/bW2OdUo2aYtPVENhHoOYEY2NEWhZWVlGS0gmVlZWVhwDAxxWVlYhOop7FFUgIB+WPz+WHyAgVRR7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";

        public static bool SUExecuteSettings(this IInlineInvokeProxy CPH, string productName, List<StreamUpSetting> streamUpSettings, string introText, IDictionary<string, object> sbArgs)
        {
            List<string> sbActions = new List<string>();
            for (int i = 0; i < streamUpSettings.Count; i++) {
                StreamUpSetting item = streamUpSettings[i];
                if (item.Type == StreamUpSettingType.Action) {
                    sbActions = CPH.GetSBActions(sbArgs);
                    if (sbActions.Count == 0) {
                        return false;
                    }
                }
            }

            var settingsForm = new Form();
            settingsForm.Text = $"StreamUP | {productName}";
            settingsForm.Width = 540;
            settingsForm.Height = 720;
            settingsForm.FormBorderStyle = FormBorderStyle.Fixed3D;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;
            byte[] bytes = Convert.FromBase64String(iconString);
            using var ms = new MemoryStream(bytes);
            settingsForm.Icon = new Icon(ms);
            var description = new Label();
            description.Text = introText;
            description.MaximumSize = new System.Drawing.Size(498, 0);
            description.AutoSize = true;
            description.Dock = DockStyle.Fill;
            description.Padding = new Padding(0, 4, 0, 0);
            var settingsTable = new TableLayoutPanel();
            settingsTable.Dock = DockStyle.Fill;
            settingsTable.ColumnCount = 2;
            settingsTable.AutoScroll = true;
            settingsTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            settingsTable.Controls.Add(description);
            settingsTable.SetColumnSpan(description, 2);

            for (int i = 0; i < streamUpSettings.Count; i++) {
                StreamUpSetting item = streamUpSettings[i];

                if (item.Type == StreamUpSettingType.Action) {
                    CPH.AddActionSetting(toTable: settingsTable, withSetting: item, withActions: sbActions, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Boolean) {
                    CPH.AddBoolSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Colour) {
                    CPH.AddColorSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Double) {
                    CPH.AddDoubleSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Dropdown) {
                    CPH.AddDropdownSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Heading) {
                    CPH.AddHeadingSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Integer) {
                    CPH.AddIntSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Label) {
                    CPH.AddLabelSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Reward) {
                    CPH.AddRewardSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Ruler) {
                    CPH.AddRulerSettings(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Secret) {
                    CPH.AddSecretSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.Spacer) {
                    CPH.AddSpacerSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
                else if (item.Type == StreamUpSettingType.String) {
                    CPH.AddStringSetting(toTable: settingsTable, withSetting: item, atIndex: i);
                }
            }

            CPH.AddButtonControls(toTable: settingsTable, withParent: settingsForm, atIndex: streamUpSettings.Count + 1, streamUpSettings, sbArgs);

            settingsTable.AutoScroll = false;
            settingsTable.HorizontalScroll.Enabled = false;
            settingsTable.AutoScroll = true;

            settingsForm.Controls.Add(settingsTable);
            var statusBar = new StatusStrip();
            var statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "© StreamUP";
            statusBar.Items.Add(statusLabel);
            settingsForm.Controls.Add(statusBar);
            settingsForm.ShowDialog();

            return savePressed;
        }
        
        private static List<string> GetSBActions(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs)
        {
            ClientWebSocket ws = new ClientWebSocket();
            var sbActions = new List<string>();

            var wsuri = sbArgs["websocketURI"].ToString();
            try {
                ws.ConnectAsync(new Uri(wsuri), CancellationToken.None).Wait();
            }
            catch {
                System.Windows.Forms.MessageBox.Show("An error occurred while fetching Streamer.bot actions.\nPlease check your Streamer.bot websocket settings, make sure the internal websocket server is turned on and try again.\n\nThe websocket URL and port should match your Streamer.bot instance.", "StreamUP Settings UI - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sbActions;
            }

            string response = string.Empty;
            string json = string.Empty;
            string data = JsonConvert.SerializeObject(
                new {
                    request = "GetActions",
                    id = "123"
                }
            );

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var dataSegment = new ArraySegment<byte>(dataBytes);
            ws.SendAsync(dataSegment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            var buffer = new byte[8192];
            var bufferSegment = new ArraySegment<byte>(buffer);
            WebSocketReceiveResult result = ws.ReceiveAsync(bufferSegment, CancellationToken.None).Result;
            response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            json += response;

            if (result.EndOfMessage) {
                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }
            else {
                while (!result.EndOfMessage) {
                    result = ws.ReceiveAsync(bufferSegment, CancellationToken.None).Result;
                    response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
                    json = json + response;
                }

                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }

            JObject actionJson = JObject.Parse(json.ToString());
            JArray actionName = actionJson.Value<JArray>("actions");
            foreach (JObject item in actionName) {
                object name = item["name"].ToString();
                sbActions.Add(name.ToString());
            }

            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "No longer needed", CancellationToken.None).Wait();
            return sbActions;
        }

        private static void AddActionSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, List<string> withActions, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange(withActions.ToArray());
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            string currentValue = CPH.GetGlobalVar<string>(withSetting.Name);
            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void AddRewardSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var twitchRewards = CPH.TwitchGetRewards();
            List<string> titleList = new List<string>();

            foreach (var reward in twitchRewards) {
                // Extract the "Title" value from the TwitchReward object
                string title = reward.Title;

                // Add the name to the names list
                titleList.Add(title);
            }

            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange(titleList.ToArray());
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            string currentValue = CPH.GetGlobalVar<string>(withSetting.Name);
            string rewardTitle = null;
            foreach (var reward in twitchRewards) {
                if (reward.Id == currentValue) {
                    rewardTitle = reward.Title;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = rewardTitle;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void AddDropdownSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange((string[])withSetting.Data);
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            var clearButton = new Button();
            clearButton.Text = "Clear";
            clearButton.Click += (sender, e) => {
                dropdown.SelectedItem = null;
            };

            string currentValue = CPH.GetGlobalVar<string>(withSetting.Name);
            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void SetComboBoxWidth(ComboBox cb, int minWidth = 120, int maxWidth = 245)
        {
            int calculatedWidth = 0;
            int temp = 0;
            foreach (var obj in cb.Items) {
                temp = TextRenderer.MeasureText(obj.ToString(), cb.Font).Width;
                if (temp > calculatedWidth) {
                    calculatedWidth = temp;
                }
            }
            calculatedWidth += 20; // Add padding
            cb.Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));

            cb.MouseWheel += (sender, e) => {
                ((HandledMouseEventArgs)e).Handled = true;
            };
        }

        private static void AddSecretSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;
            textbox.UseSystemPasswordChar = true;

            string currentValue = CPH.GetGlobalVar<string>(withSetting.Name);
            if (!string.IsNullOrEmpty(currentValue)) {
                textbox.Text = currentValue;
            }
            else
            if (!string.IsNullOrEmpty(withSetting.Default)) {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex + 1);
        }

        private static void AddSpacerSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            toTable.Controls.Add(new Label(), 0, atIndex + 1);
            toTable.Controls.Add(new Label(), 1, atIndex + 1);
        }

        private static void AddRulerSettings(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            Label rulerLabel = new Label();
            rulerLabel.BorderStyle = BorderStyle.Fixed3D;
            rulerLabel.Height = 2;
            rulerLabel.Width = 496;
            rulerLabel.Margin = new Padding(10, 10, 10, 10);

            toTable.Controls.Add(rulerLabel, 0, atIndex + 1);
            toTable.SetColumnSpan(rulerLabel, 2);
        }

        private static void AddStringSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;
            string currentValue = CPH.GetGlobalVar<string>(withSetting.Name);
            if (!string.IsNullOrEmpty(currentValue)) {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex + 1);
        }

        private static void AddColorSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 8, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(270, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var button = new Button();
            button.Text = "Pick a colour";
            button.AutoSize = true;
            button.Name = withSetting.Name;
            var currentValue = CPH.GetGlobalVar<long>(withSetting.Name);
            if (currentValue != null && currentValue != 0) {
                byte a = (byte)((currentValue & 0xff000000) >> 24);
                byte b = (byte)((currentValue & 0x00ff0000) >> 16);
                byte g = (byte)((currentValue & 0x0000ff00) >> 8);
                byte r = (byte)(currentValue & 0x000000ff);

                var colour = System.Drawing.Color.FromArgb(a, r, g, b);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var colour = ColorTranslator.FromHtml(withSetting.Default);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }

            button.Click += (sender, e) => {
                var colorDialog = new ColorDialog();
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog() == DialogResult.OK) {
                    button.ForeColor = (colorDialog.Color.GetBrightness() < 0.5) ? Color.White : Color.Black;
                    button.BackColor = colorDialog.Color;
                }
            };
            toTable.Controls.Add(button, 1, atIndex + 1);
        }

        private static void AddBoolSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var checkbox = new CheckBox();
            checkbox.Name = withSetting.Name;
            var currentValue = CPH.GetGlobalVar<bool?>(withSetting.Name);
            if (currentValue != null) {
                checkbox.Checked = currentValue.Value;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = bool.TryParse(withSetting.Default, out bool defaultValue);
                checkbox.Checked = hasCorrectDefault ? defaultValue : false;
            }

            toTable.Controls.Add(checkbox, 1, atIndex + 1);
        }

        private static void AddIntSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var input = new NumericUpDown();
            input.Minimum = int.MinValue;
            input.Maximum = int.MaxValue;
            input.Name = withSetting.Name;
            input.Tag = withSetting.Type;
            var currentValue = CPH.GetGlobalVar<int>(withSetting.Name);
            if (currentValue != 0) {
                input.Value = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = int.TryParse(withSetting.Default, out int defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex + 1);
        }

        private static void AddLabelSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(498, 0);

            toTable.Controls.Add(label, 0, atIndex + 1);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddHeadingSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(498, 0);
            label.Font = new Font(label.Font.FontFamily, label.Font.Size + 2, System.Drawing.FontStyle.Bold);

            toTable.Controls.Add(label, 0, atIndex + 1);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddDoubleSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var input = new NumericUpDown();
            input.Minimum = decimal.MinValue;
            input.Maximum = decimal.MaxValue;
            input.DecimalPlaces = 2;
            input.Increment = 0.01m;
            input.Name = withSetting.Name;
            input.Tag = withSetting.Type;

            var currentValue = CPH.GetGlobalVar<double>(withSetting.Name);
            if (currentValue != 0) {
                input.Value = Convert.ToDecimal(currentValue);
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = decimal.TryParse(withSetting.Default, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex + 1);
        }

        private static void AddButtonControls(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, Form withParent, int atIndex, List<StreamUpSetting> streamUpSettings, IDictionary<string, object> sbArgs)
        {
            var resetButton = new Button();
            resetButton.Font = new Font("Segoe UI Emoji", 10);
            resetButton.Text = "🔄 Reset";
            resetButton.BackColor = Color.LightCoral;
            resetButton.Click += (sender, e) => {
                DialogResult result = System.Windows.Forms.MessageBox.Show("Are you sure you want to reset all settings?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes) {
                    foreach (var setting in streamUpSettings) {
                        if (!string.IsNullOrEmpty(setting.Name)) {
                            CPH.UnsetGlobalVar(setting.Name);
                        }
                    }
                    withParent.Close();
                    CPH.RunAction(sbArgs["actionName"].ToString(), false);
                }
            };

            var saveButton = new Button();
            saveButton.Font = new Font("Segoe UI Emoji", 10);
            saveButton.Text = "💾 Save";
            saveButton.BackColor = Color.LightGreen;
            saveButton.Click += (sender, e) => {
                var twitchRewards = CPH.TwitchGetRewards();
                List<Control> filteredControls = new List<Control>();
                foreach (Control control in toTable.Controls) {
                    if (!string.IsNullOrEmpty(control.Name)) {
                        filteredControls.Add(control);
                    }
                }

                foreach (var control in filteredControls) {
                    switch (control) {
                        case ComboBox:
                            if (control.Tag is StreamUpSettingType.Action) {
                                string actionValue = string.Empty;
                                if (((ComboBox)control).SelectedItem is not null) {
                                    actionValue = ((ComboBox)control).SelectedItem.ToString();
                                }
                                CPH.SetGlobalVar(control.Name, actionValue);
                            }
                            else if (control.Tag is StreamUpSettingType.Reward) {
                                string dropdownValue = string.Empty;
                                if (((ComboBox)control).SelectedItem is not null) {
                                    dropdownValue = ((ComboBox)control).SelectedItem.ToString();
                                }

                                string rewardId = null;

                                foreach (var reward in twitchRewards) {
                                    if (reward.Title == dropdownValue) {
                                        rewardId = reward.Id;
                                        break;
                                    }
                                }

                                CPH.SetGlobalVar(control.Name, rewardId);
                            }
                            else if (control.Tag is StreamUpSettingType.Dropdown) {
                                string dropdownValue = string.Empty;
                                if (((ComboBox)control).SelectedItem is not null) {
                                    dropdownValue = ((ComboBox)control).SelectedItem.ToString();
                                }
                                CPH.SetGlobalVar(control.Name, dropdownValue);
                            }

                            break;
                        case CheckBox:
                            bool checkboxValue = ((CheckBox)control).Checked;
                            CPH.SetGlobalVar(control.Name, checkboxValue);
                            break;
                        case Button:
                            long colourValue = ((long)control.BackColor.A << 24) | ((long)control.BackColor.B << 16) | ((long)control.BackColor.G << 8) | (long)control.BackColor.R;
                            CPH.SetGlobalVar(control.Name, colourValue);
                            break;
                        case NumericUpDown:
                            if (control.Tag is StreamUpSettingType.Integer) {
                                int integerValue = Convert.ToInt32(((NumericUpDown)control).Value);
                                CPH.SetGlobalVar(control.Name, integerValue);
                            }
                            else if (control.Tag is StreamUpSettingType.Double) {
                                double doubleValue = Convert.ToDouble(((NumericUpDown)control).Value);
                                CPH.SetGlobalVar(control.Name, doubleValue);
                            }
                            break;
                        case TextBox:
                            string stringValue = control.Text;
                            CPH.SetGlobalVar(control.Name, stringValue);
                            break;
                    }
                }
                savePressed = true;
                withParent.Close();
            };

            TableLayoutPanel innerTableLayoutPanel = new TableLayoutPanel();
            innerTableLayoutPanel.ColumnCount = 2; // Adjust the number of columns as needed
            innerTableLayoutPanel.RowCount = 1;
            innerTableLayoutPanel.Dock = DockStyle.Fill;

            toTable.Controls.Add(innerTableLayoutPanel, 0, atIndex + 1);
            toTable.SetColumnSpan(innerTableLayoutPanel, 2);

            innerTableLayoutPanel.Controls.Add(resetButton, 0, 0);
            innerTableLayoutPanel.Controls.Add(saveButton, 1, 0);
        }
    }

    public class StreamUpSetting
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public StreamUpSettingType Type { get; set; }

        public string Default { get; set; }

        public object Data { get; set; }
    }

    public enum StreamUpSettingType
    {
        Action,
        Boolean,
        Colour,
        Double,
        Dropdown,
        Heading,
        Integer,
        Label,
        Reward,
        Ruler,
        Secret,
        Spacer,
        String,
    }

   public static class ProductSettingsBuilder
    {
        public static StreamUpSetting SUSettingsCreateAction(this IInlineInvokeProxy CPH, string name, string description)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Action, };
        }
        public static StreamUpSetting SUSettingsCreateBoolean(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Boolean, Default = defaultValue,};
        }
        public static StreamUpSetting SUSettingsCreateColour(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Colour, Default = defaultValue,};
        }
        public static StreamUpSetting SUSettingsCreateDouble(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Double, Default = defaultValue,};
        }
        public static StreamUpSetting SUSettingsCreateDropdown(this IInlineInvokeProxy CPH, string name, string description, string[] data, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Dropdown, Data = data, Default = defaultValue};
        }
        public static StreamUpSetting SUSettingsCreateHeading(this IInlineInvokeProxy CPH, string description)
        {
            return new StreamUpSetting { Description = description, Type = StreamUpSettingType.Heading, };
        }
        public static StreamUpSetting SUSettingsCreateInteger(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Integer, };
        }
        public static StreamUpSetting SUSettingsCreateLabel(this IInlineInvokeProxy CPH, string description)
        {
            return new StreamUpSetting { Description = description, Type = StreamUpSettingType.Label, };
        }
        public static StreamUpSetting SUSettingsCreateReward(this IInlineInvokeProxy CPH, string name, string description)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Reward, };
        }
        public static StreamUpSetting SUSettingsCreateRuler(this IInlineInvokeProxy CPH)
        {
            return new StreamUpSetting {Type = StreamUpSettingType.Ruler, };
        }
        public static StreamUpSetting SUSettingsCreateSecret(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Secret, Default = defaultValue, };
        }
        public static StreamUpSetting SUSettingsCreateSpacer(this IInlineInvokeProxy CPH)
        {
            return new StreamUpSetting { Type = StreamUpSettingType.Spacer, };
        }
        public static StreamUpSetting SUSettingsCreateString(this IInlineInvokeProxy CPH, string name, string description, string defaultValue)
        {
            return new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.String, Default = defaultValue, };
        }
        
    }
}