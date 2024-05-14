﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class UIExtensions {

        public static DialogResult SUUIShowErrorOKMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message,
                "StreamUP Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return result;
        }  

        public static DialogResult SUUIShowWarningYesNoMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message, 
                "StreamUP Warning", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning
                );
            return result;
        }

        public static DialogResult SUUIShowInformationOKMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message, 
                "StreamUP Notification", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
                );
            return result;
        }

        public static void SUUIShowToastNotification(this IInlineInvokeProxy CPH, string title, string message)
        {
            CPH.ShowToastNotification(title, message);
        }

        public static void SUUIShowSettingsLoadingMessage(this IInlineInvokeProxy CPH, string title)
        {
            Thread thread = new Thread(() =>
            {
                Form messageForm = new Form();
                messageForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                messageForm.StartPosition = FormStartPosition.CenterScreen;
                messageForm.MaximizeBox = false;
                messageForm.MinimizeBox = false;
                messageForm.Text = title;

                byte[] iconBytes = Convert.FromBase64String(UIResources.supIconString);
                using (var msIcon = new MemoryStream(iconBytes))
                {
                    messageForm.Icon = new Icon(msIcon);
                }

                byte[] imageBytes = Convert.FromBase64String(UIResources.supSettingsLoadingBGString);

                int formWidth = 0;
                int formHeight = 0;

                using (var msImage = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(msImage);

                    // Create a PictureBox to display the image
                    var description = new PictureBox();
                    description.Image = image;
                    description.SizeMode = PictureBoxSizeMode.AutoSize;
                    description.Dock = DockStyle.Top;
                    formWidth += image.Width;
                    formHeight += image.Height;
                    
                    // Add the PictureBox to the form
                    messageForm.Controls.Add(description);
                }

                ProgressBar progressBar = new ProgressBar
                {
                    Dock = DockStyle.Bottom,
                    Style = ProgressBarStyle.Continuous,
                    Minimum = 0,
                    Maximum = UIResources.streamUpSettingsCount,
                    Value = 0
                };
                messageForm.Controls.Add(progressBar);
                formHeight += progressBar.Height;

                // Set the form's size to match the image size
                messageForm.ClientSize = new Size(formWidth, formHeight);

                // Attach a timer to check the condition
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 200;
                timer.Tick += (sender, args) =>
                {
                    progressBar.Value = UIResources.streamUpSettingsProgress;
                    if (UIResources.closeLoadingWindow)
                    {
                        messageForm.Close();
                        timer.Stop();
                    }
                };
                timer.Start();

                messageForm.FormClosing += (sender, args) => Application.ExitThread();

                Application.Run(messageForm);
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static (DialogResult, bool) SUUIShowObsPluginsUpdateMessage(this IInlineInvokeProxy CPH)
        {
            using (var form = new Form()
            {
                Text = "StreamUP Warning",
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                Size = new System.Drawing.Size(400, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                AcceptButton = new Button(),
                CancelButton = new Button()
            })
            {

                var pictureBoxIcon = new PictureBox()
                {
                    Image = SystemIcons.Warning.ToBitmap(),
                    Location = new System.Drawing.Point(10, 20),
                    Size = new System.Drawing.Size(32, 32),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };               
                
                var labelMessage = new Label()
                {
                    Text = "OBS has plugins that are required that are missing or out of date.\nYou can use the StreamUP Pluginstaller to download them all in one go.\n\nWould you like to open the download page now?",
                    AutoSize = false, 
                    Size = new System.Drawing.Size(330, 100),
                    Location = new System.Drawing.Point(50, 10),
                    Padding = new Padding(10),
                };

                var checkBoxOption = new CheckBox()
                {
                    Text = "Don't ask me again\nIf this is checked the action will try to continue to run.",
                    Location = new System.Drawing.Point(50, 110),
                    Size = new System.Drawing.Size(300, 50)
                };

                var buttonYes = new Button()
                {
                    Text = "Yes",
                    DialogResult = DialogResult.Yes,
                    Location = new System.Drawing.Point(75, 160),
                    Size = new System.Drawing.Size(100, 25)
                };

                var buttonNo = new Button()
                {
                    Text = "No",
                    DialogResult = DialogResult.No,
                    Location = new System.Drawing.Point(225, 160),
                    Size = new System.Drawing.Size(100, 25)
                };

                form.Controls.Add(pictureBoxIcon);
                form.Controls.Add(labelMessage);
                form.Controls.Add(buttonYes);
                form.Controls.Add(buttonNo);
                form.Controls.Add(checkBoxOption);
                
                form.Focus();
                var result = form.ShowDialog();
                bool checkBoxChecked = !checkBoxOption.Checked;

                return (result, checkBoxChecked);
            }
        }
    
        public static string SUUIShowSaveScreenshotDialog(this IInlineInvokeProxy CPH, string sourceName, string dateTime)
        {
            string defaultFileName = $"{sourceName}_{dateTime}";
            string input = CPH.SUUIShowInputBox("Enter a file name to save your screenshot as:", "Save Screenshot (.png)", defaultFileName);

            if (!string.IsNullOrWhiteSpace(input))
            {
                string fileName = input;
                return fileName;
            }
            else
            {
                return null;
            }
        }

        public static string SUUIShowInputBox(this IInlineInvokeProxy CPH, string message, string caption, string defaultValue)
        {
            using (Form form = new Form())
            {
                form.Text = caption;
                form.Width = 300;
                form.Height = 150;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AutoSize = true;
                form.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                Label label = new Label() { Left = 20, Top = 20, Text = message, AutoSize = true };
                TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 250, Text = defaultValue };
                Button buttonOk = new Button() { Text = "OK", DialogResult = DialogResult.OK, Left = 100, Width = 100, Top = 80 };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.AcceptButton = buttonOk;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    
    }
    

    public static class UIResources
    {
        public static bool closeLoadingWindow = false;
        public static int streamUpSettingsCount = 0;
        public static int streamUpSettingsProgress = 0;
        public static readonly string supSettingsLoadingBGString = "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";
        public static readonly string supIconString = "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39/f39/f39/f39/f3+fn5/79/v////////79/ufn59/f39/f39/f39/f39/f3+Li4qampgwMDAQEAp2WQv/3a//0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAsKBcO6Uv/3a//0av/0av/0av/0av/0av/0av/1au/mZENAHQAAAFlZWff39/////////////////////////////////////////////////////////////////f391lZWQAAAENAHe/mZP/1av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAgIBL21UP/4bP/0av/0av/0av/0av/0av/0av/0av/3bLCpSwoKBAsLDLKysv///////////////////////////////////////////////////////////////7KysgsLDAoKBLCpSv/3bP/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAJONPv/4bP/0av/0av/0av/0av/0av/0av/0av/1avbtZ1VSJAAAAEVFRfDw8P////////////////////////////////////////////////////////Dw8EVFRQAAAFVSJPbtZ//1av/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAADg2GOXcYP/3a//0av/0av/0av/0av/0av/0av/0av/3a8G6UhMSCAUFBZ+fn////////////////////////////////////////////////////////5+fnwUFBRMSCMG6Uv/3a//0av/0av/0av/0av/0av/0av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAAEBAFdUJdzSXP71a//4bP/4bP/4bP/4bP/4bP/4bP/4bPz1a2xoLgAAADU1Nefn5////////////////////////////////////////////////+fn5zU1NQAAAGxoLvz1a//4bP/4bP/4bP/4bP/4bP/4bP/1av/0av/0av/3a8O6UgsKBQAAAAAAAAAAAAAAAAEBACYkEGllLYiCOYqEOoqEOoqEOoqEOoqEOoqEOoyGO2VgKwUFAgEBAYuLi////////////////////////////////////////////////4uLiwEBAQUFAmVgK4yGO4qEOoqEOoqEOoqEOomDOZmSQOrgYv/1av/0av/3a8O6UgsKBQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACYmJtvb2////////////////////////////////////////9vb2yYmJgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEBAZ2WQv/4bP/0av/3a8O6UgsKBQAAAAAAAAMDAjw5Gk9MIk5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5KIU5LIU1JIRkYCwAAAHd3d/7+/v////////////////////////////////7+/nd3dwAAABkYC01JIU5LIU5KIU5KIU5KIU5KIU5KIU1KIV1ZKNPKWf/2a//0av/3a8O7UgsKBQAAAAAAAAsKBcC3Uf7zavvwafvwafvwafvwafvwafvwafvwafvwafvwafvwafvwaf3yapCKPQEBABoaGs3Nzf///////////////////////////////83NzRoaGgEBAJCKPf3yavvwafvwafvwafvwafvwafvwafvwafzyaf/1a//0av/0av/4bLOsSwYFAwAAAAAAAAsKBcO6Uv/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a+ngYjk3GQAAAGNjY/r6+v////////////////////////r6+mNjYwAAADk3GengYv/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av71anBrMAAAAAAAAAAAAAsKBcO6Uv/3a//0av/0av/0av/1av/1av/1av/1av/1av/1av/1av/1av/1av/5bKWfRgcGAhAQEL29vf///////////////////////729vRAQEAYGAqWfRv/5bP/1av/1av/1av/1av/1av/1av/1av/1av/1av/1a//3a//3a7SsTBYVCQAAAAAAAAAAAAsKBcO6Uv/3a//0av/0av70avHnZeziY+ziY+ziY+ziY+ziY+ziY+ziYuziYu3iY+DWXj88GwAAAFBQUPT09P////////////////T09FBQUAAAAD88G9/VXuziY+vhYuvhYuvhYuvhYuvhYuvhYuvhYuvhYuvhYurgYtjPW4yGOxwbDAAAAAAAAAAAAAAAAAsKBcO6Uv/3a//0av/3a8O7Ujo4GS0qEy0rEy0rEy0rEy0rEy0rEy0rEy0rEy0rEy4sExYVCQAAAAkJCaurq////////////////6urqwkJCQAAABUUCS0rFCwqEywqEywqEywqEywqEywpEywpEywpEyspEywpEyspEhkYCwMCAQAAAAAAAAAAAAAAAAAAAAsKBcO6Uv/3a//0av/4bKWeRQcGAwEBAAEBAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQMDAQMDAQAAAD8/P+zs7P///////+zs7D8/PwAAAAMDAQMDAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQICAQMDAQUEAgUFAgQEAgAAAAAAAAAAAAsKBcO6Uv/3a//0av/1avTpZry0T6+nSbCoSrCoSrCoSrCoSrCoSrCpSrCpSrCpSrGpSrKqS5iSQBcVCgIDA5eXl////////5eXlwIDAxcWCpmSQLOrS7KqSrKqSrKqSrKqSrKqSrKqSrKqSrKrSrKrSrKrS7OrS7OrS7OrS7OrS7WtTImDOggHAwAAAAAAAAsKBcO6Uv/3a//0av/0av/1av/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP31a3FsMAAAAC8vL+Tk5OTk5C8vLwAAAHFsMP31a//4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/4bP/8bcO+UwsKBQAAAAAAAAoJBMC4Uf/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a9bOWiIhDwAAAISEhISEhAAAACIhD9bOWv/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/4a8K6UgoKBQAAAAAAAAICAaSeRv/4bP/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a4WAOAAAABQUFRQUFQAAAIV/Of/2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/4bKagRgMDAQAAAAAAAAAAAFNPI/XsZ//1a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/2a+PaXzAuFQAAAAAAADAuFePaX//2a//0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/0av/1a/btZ1VRJAAAAAAAAAAAAAAAAAcHA4aAOfbsZ//4bP/4bP/3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//6bZuWQgUEAgUEApuWQv/6bf/3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//3a//4bP/4bPbtZ4iCOggIBAAAAAAAAAAAAAAAAAAAAAgIBFNPJKWeRsG5UcO7UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsS8UrOsTC4sFS4sFbOsTMS8UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO6UsO7UsG5UaafRlRQJAkIBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMDAQoJBAsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQwLBQcHAwcHAwwLBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQsKBQoJBAMDAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgAAAAgAAAAQAAAAAEACAAAAAAAAAQAAAAAAAAAAAAAAAEAAAAAAAAAAAAAZltzAD4sVAD/+m0Ajog8AFNTUwBWPXQAa02RAL6I/wC/iP8A0NDRABscHQDCi/8AMiNDAH95NADDjv8Ab29wAAsIDwCwqEoAg303AF9EgAAPDgYAISEjAMma/wC3r00AiIQ6ABISEgCJhDoA//ZrAF1ZJwBSO28AkGfDAD4sVQAwLhQAVDxyANCm/wBWP3IAqqRIAFlFcgAcHB4ACgoEAAoHDQDGvVMA17L/APXrZgCEgoUAsH7sALF+7ABrZy0ADw4HAJ1w1QCyf+8AhYA4ABIRBwD88mkAiYM4AHNuMABROm0A//lsAGVlZgClnUYA08tZALyG+wADAwIA48r/AL2H/gBXP3MAamF1AKp55wDCuVEAgl2wABsbHABaRXMAODUYAAsKBQDq1v8AXUN/ACAXKwAlIxAAVFEjAK6urgBgRIIA49lfABIRCAC2rkwA8eL/ANDHVwD+9WoA//VqAI9mwgD//G0AGRgLAKd34gAtID0AvYf/AGtMkQD47v8AHRsLAL6H/wDYzloABgYDABoaGgDBiv8Agl2xABsbHQCDXbEAIB8OAAkGDAAeFikAIyIOAN7VXQD++v8Agnw3ACEXLAAODQYAsapKALKqSgCyf/EAysNVAO7u7gAUEwYA5txgABINGACJgzoA/vVrAFxYJwD/9WsAZ0mJANPKWADg4OAAPz9AADY0FwAJCQQA9OpmANex/wD16mYAHxYqAFxKcgCYkUAAOzgaALKqSwA+OxoAdFOeAPrxaQBYVCUA3b3/AHFtMABkSIcAZkmKAKR13gD/+GwAGBcKAF9bKAC8hv4AvYb+AOTk5ADDu1EAZmIrAB0VKAAeFSgA+/v7ADo3GABdSnMADQwFAJuUQQCampoASzZlAE03aACHgjkAiII5ABQTCABkSIgA//RqAAEBAAD/+20A39/fAEhFHgC8hv8A1s1aAL2G/wDXzVoAwIn/AMO7UgA4NhYAxLtSANrUXQCtfOsAHhUpAK586wAjIQ4Av7/AAGllLABqZSwACwsMAA0MBgCxqUoAc1KdAPjtaAAODQkA////AEs2ZgAPDgwAh4eHAHVToACLY70A2traAGFiYgAUEwkAZUiJAIuFOgD/92sAFxYJANPJWAABAQEApZ5FAAMCBABWPXIAa2V0ACIgDAB/ejUArnzsAMa+UwBva3QAx75TADw6GgAPDwcAYE9yABAPBwAQCxYAn3LYAGJVcgBjR4cAcWwwAGZIigD/92wA1cxZAPX19QDCulEA0NDQAGxldQAvLzAAw7pRAGVhKwBmYSsAOTYYAG9rdQCEg4YAh2C2AGBPcwARCxcAYlVzAP3zagCKhDkALSsTABMTFAC4g/kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHrIq9DQ0NDQ0NDQ0NDQlF3kTOnQ54gAAAAAAAAAAAB695oJCQkJCQkJCQkJCQlm2GwutQk+OWxxcU161QAAAKZBsbGxsbGxsbGxsV5esWIin7xesbF1ury8ujKn1wAAkwmxsbGxsbGxsbH/B8xiYiKfvF6xsWK1tbViCTMNAADQCbGxsbGxsbGxXtyIBggIIp+8XrGxXFlZH0SzCRQAANAJsbGxsbGxsbFevLtCDw8kn7xesZkeaxEpUV4J0AAA0AmxsbGxsbGxsV7cu0gXFyafvF6xseVGZ2mVXgnQAADQCbGxsbGxsbGxXty7oiMjiZ+8XrGxYgkJCV6xCdAAANAJsbGxsbGxsbFe3Lv4K4binrxesbGxsbGxsbEJ0AAAfwwJCQkJCQkJCbUvu/qRkea7L7UJCQkJCQkJCQx/AAACy8TExMTExMTEjl/5AUBAAflfjsTExMTExMTEyyAAAErPqqqqqqqqqqrPUwBDS0tDAFPPqqqqqqqqqqrPSgAA6LLU1NSA67S0tGPd2tlVVe/aKmO0tLTrgNTU1LLoAAAbrtLS0tK20fw3/A543mBg9XgO/Df80bbS0tLSrhsAAHuWrKysrDAA/idoC2Utb2/2ZQtoJ/4AMKysrKyWewAAe5asrKzSuDXOgYGBr3fHx3evgYGBzjW40qysrJZ7AAA0lqysrKw2wEcKx8fHx8fHx8fHx+5HwDasrKyslnsAAJCPWKysrNI9YTugx8fHx8fHx8egO2E90qysrKyWewAAcoosrKysrH7bGr7Hx8fHx8fHx74a236srKx+rJZ7AAAAhI19HR0dmLCtBezHx8fHx8fsBa2wmB0dHcMclnsAAGrg9ElJSUlJoSHJUMfHx8fHx1DJIaFJSUmDpByWewAAE8WFhYWFhYUsUreCd8fHx8d3grdSLIWFhYc2rNKSAAB7OlgcOjo6OjpaJcalx8fHx6XGJVo6Ojo6OpY6Vk4AAHuWWG6Mc3Nzw3TWbfCbx8eb8L08wxISEhISEgT9AAAAe5YccMIVFRUVFeOjP8rHx8o/o+MxMTEx4eExKIRkAAB7llh57UVFRe3t7fFPFs3NFk+cnJycnJycnLi4378AAKiWrH7Slurq6urqA3bTEBDTdgPS0tLS0tLS0tKuGQAA8lesrKysrKysrKys+zjBwTj7rKysrKysrKysrHydAACXVFeWlpaWlpaWlpYDuU5OuQOWlpaWlpaWlpZ8GJcAAACX8ql7e3t7e3t7e3t7i4t7e3t7e3t7e3t7qfNbAAAAAAAAAAAAAAAAAAAAAACtrQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKAAAABAAAAAgAAAAAQAIAAAAAAAAAQAAAAAAAAAAAAAAAQAAAAAAAAAAAABrTI4Ak3ezAGJeLAC+iP8Av4j/APPoZQBoZCwADw4GAPjvaACdcNQAnImzACMZLwAoJxEA2dnaAKCZQgDn3WAAo5xCAP/2awC4g/oAMC4UAHdyMgC4tL4AubS+AH5+eQCDXrIAUk4iAE1LNAD262YAIxkwAM3EVgBYVSUAWVUlAP3yaQCMY8EAXFglAI5kxAD/+WwA1MtZAKd34QB4cTkAgl2wAHx1PABnYysA3tVcAJtv0wD57mcAiWezAFlVJgC1gfYAtoH2AOrgYgC6hfkA6+BiAHZxMQBpS44ALSA9AHhxOgDXzloAvof/AMGK/wAxIkMA9OplADs4GQCvfesA3tVdAGpmLAD37mgA+e5oALJ/8QDMw1UA5txgAC8tFABdWi0Ad3EyAElGHwAFBAcAQzBbADY0FwAJCQQAmW3PAMi/UwBQTjcAdFOeAFdUJQBYVCUA/PFpAJ+YQwA9PC8Az8ZWAE85bQCMhjsA4ODhAH5aqgC8hv4AQS9ZAEIvWQDd1FwAwcHFAMnCVACMjIwAKB03AG9sQABmSYsA0slXAHVwMQBALlcAQS5XALyG/wDNzc4Appq1APb29gBDL1oAwIn/AJZrywD06WUAaWUsAGplLAD///8AiGC9APnwaAD/92sAZ0mMAAEBAQBBLlgAVj51AHpzOwAyMSMA9exmAPbsZgBWUyUAzsVWACspFQAsKRUA6N5hAERAIwC4g/gAuYT7ADEvFQBLSCAA7+VkAGxrYwAyMSQAMSNCAJWPPgB9djwA9uxnAP3+/gAkGjEA5NpfAFpWJgC3g/kA0chXAHRvMQC4g/kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB7HXxfX19fcDiUTWUAAAAADAoTl5eaMoldZ15QemdqTGuIbGxsBClyGTdxRS0nQI9fl2xsbAU3Ly83cS19UzFgYDQFBQU8AQICATwFOwU0YD13IiIiJFoLC1okIiIidz2EfpEqOSiHbm6HKDkqkX6FIHglY0lmGxYXG2ZJYyV4IIIJeSxSYlxvb1xiUiwSRCANUQY+W2R2dnZ2ZFtzgS4gCEsHdCNYDnZ2DlgjK5CAHxqVhhA1DxiTkxgPM0eMQU4wkleZFUJ/bW2OdUo2aYtPVENhHoOYEY2NEWhZWVlGS0gmVlZWVhwDAxxWVlYhOop7FFUgIB+WPz+WHyAgVRR7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";
    }

    public static class AlertMessageVariableLists
    {
        // Misc
        public static string Donation = "%amountCurrency% = Donation Amount\n" +
                                        "%message% = Donation Message\n" +
                                        "%user% = Donators Name";


        // Twitch
        public static string TwitchCheer =  "%amount% = Cheer Amount\n" +
                                            "%message% = Cheer Message\n" +
                                            "%user% = Cheerers Name";

        public static string TwitchFollow = "%user% = Followers Name";

        public static string TwitchRaid =   "%amount% = Viewer Amount\n" +	
                                            "%user% = Raiders Name";

        public static string TwitchSub =    "%message% = Subscribers Message\n" +
							                "%tier% = Sub Tier\n" +
                                            "%user% = Subscribers Name";

        public static string TwitchReSub = 	"%message% = Sub Message\n" +
							                "%monthsTotal% = Total Months Subscribed\n" +
                                            "%monthsStreak% = Current Sub Streak\n" +
                                            "%tier% = Sub Tier\n" +
                                            "%user% = Subscribers Name";

        public static string TwitchGiftSub =    "%monthsTotal% = Total Months Subbed\n" +
								                "%monthsGifted% = Amount Of Months Gifted\n" +
                                                "%receiver% = Sub Receiver\n" +
                                                "%tier% = Sub Tier\n" +
                                                "%totalAmount% = Total Subs Gifted\n" +
                                                "%user% = Gifters Name";

        public static string TwitchGiftBomb =   "%amount% = Amount Of Subs Gifted\n" +
								                "%tier% = Sub Tier\n" +
								                "%totalAmount% = Total Subs Gifted\n" +
                                                "%user% = Gifters Name";

        public static string TwitchFirstWords = "%message% = Chat Message\n" +
                                                "%user% = Chatters Name";


        public static string TwitchRewardRedemption =   "%message% = Chat Message\n" +
                                                        "%user% = Chatters Name";

        public static string TwitchShoutoutCreated =    "%receiver% = User Being Shoutout\n" +
                                                        "%user% = User Executing Shoutout";

        public static string TwitchUserBanned = "%reason% = Reason For Ban\n" +
							                    "%receiver% = User That Is Banned\n" +
                                                "%user% = User Executing The Ban";

        public static string TwitchUserTimedOut =   "%duration% = Duration Of Time Out\n" +
							                        "%reason% = Reason For Time Out\n" +
                                                    "%receiver% = User That Is Banned\n" +
                                                    "%user% = User Executing The Timeout";

                                            
        // YouTube                                    
        public static string YouTubeFirstWords =    "%message% = Chat Message\n" +
                                                    "%user% = Chatters Name";

        public static string YouTubeNewSubscriber = "%user% = Username";

        public static string YouTubeNewSponsor =    "%tier% = Membership tier\n" +
                                                    "%user% = Username";

        public static string YouTubeMemberMileStone =   "%message% = Chat Message\n" +
                                                        "%monthsTotal% = Total Months Being A Member\n" +
                                                        "%tier% = Membership Tier\n" +
                                                        "%user% = Username";

        public static string YouTubeMembershipGift =    "%amount% = Amount Of Memberships Gifted\n" +
							                            "%tier% = Membership Tier\n" +
                                                        "%user% = Username";

        public static string YouTubeSuperChat = "%amountCurrency% = SuperChat Amount\n" +
                                                "%message% = Chat Message\n" +
                                                "%user% = Username";

        public static string YouTubeSuperSticker =  "%amountCurrency% = SuperSticker Amount\n" +
                                                    "%user% = Username";

        public static string YouTubeUserBanned =    "%duration% = Duration Of Ban\n" +
                                                    "%banType% = Ban Type\n" +
                                                    "%user% = Username";
    }

}
