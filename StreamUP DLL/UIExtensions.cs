using System;
using System.Collections.Generic;
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

        public static string TwitchWatchStreak = "%amount% = Current stream streak amount";
        
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
