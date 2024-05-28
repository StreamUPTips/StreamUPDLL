using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Newtonsoft.Json;
using Streamer.bot.Common.Events;
using Streamer.bot.Plugin.Interface;
using Timer = System.Timers.Timer;

namespace StreamUP {

    public static class PollExtensions {

        public static ProductSettings productSettings = new ProductSettings();

        private static void SendMessageToChat(this IInlineInvokeProxy CPH, string message, ProductSettings productSettings, PollData pollData, Dictionary<string, object> args, int choiceIndex = -1)
        {
            CPH.Wait(50);
            var replacements = new Dictionary<string, string>
            {
                { "%user%", args.ContainsKey("user") ? args["user"].ToString() : "" },
                { "%duration%", pollData.Duration.ToString() },
                { "%title%", pollData.Title },
                { "%votesTotal%", pollData.VotesTotal.ToString() },
                { "%winnerCount%", pollData.Choices.Count(c => c.Win).ToString() },
                { "%choiceCount%", pollData.ChoiceCount.ToString() },
                { "%choiceTitle%", choiceIndex >= 0 && choiceIndex < pollData.Choices.Count ? pollData.Choices[choiceIndex].Title : "" },
                { "%choiceTotalVotes%", choiceIndex >= 0 && choiceIndex < pollData.Choices.Count ? pollData.Choices[choiceIndex].TotalVotes.ToString() : "" },
                { "%choicePercentage%", choiceIndex >= 0 && choiceIndex < pollData.Choices.Count 
                    ? double.IsNaN(pollData.Choices[choiceIndex].Percentage) 
                        ? "0%" 
                        : pollData.Choices[choiceIndex].Percentage.ToString("F2") + "%" 
                    : "" }
            };

            var sb = new StringBuilder(message);

            foreach (var replacement in replacements)
            {
                sb.Replace(replacement.Key, replacement.Value);
            }

            string finalMessage = sb.ToString();

            if (pollData.SendToTwitch)
            {
                CPH.SendMessage(finalMessage, productSettings.BotAccount);
            }
            if (pollData.SendToYouTube)
            {
                CPH.SendYouTubeMessage(finalMessage, productSettings.BotAccount);
            }
            CPH.Wait(50);
        }        

        public static void SUPollCreate(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber = "sup043")
        {
            LoadProductSettings(CPH, productNumber);

            if (CPH.GetGlobalVar<bool>($"{productNumber}_PollActive", false))
            {
                SendMessageToChat(CPH, productSettings.PollAlreadyActiveMessage, productSettings, null, args);
                CPH.SUWriteLog("Poll is currently active.");
                return;
            }

            CPH.UnsetGlobalVar($"{productNumber}_PollData", false);

            PollData pollData = new PollData();
            pollData.EventType = "StreamUPPollCreated";
            pollData.TimeStarted = DateTime.Now;

            // Create form
            var settingsForm = CreateForm();

            // Add main layout and controls
            var mainLayout = CreateMainLayout();
            settingsForm.Controls.Add(mainLayout);

            // Create settings table and add settings
            var settingsTable = CreateSettingsTable();
            AddSettings(settingsTable, pollData);

            mainLayout.Controls.Add(settingsTable, 0, 1);
            settingsForm.Controls.Add(mainLayout);

            // Add buttons and status bar
            var buttonPanel = CreateButtonPanel(settingsForm, pollData);
            settingsForm.Controls.Add(buttonPanel);
            var statusBar = CreateStatusBar();
            settingsForm.Controls.Add(statusBar);

            // Initialize and show form
            settingsForm.Focus();
            settingsForm.ShowDialog();

            // Check if the poll was started
            if (pollData.Started)
            {
                // Save poll data to global variable and trigger actions/events
                pollData.DurationMs = pollData.Duration * 1000;
                string pollDataJson = JsonConvert.SerializeObject(pollData);
                CPH.SetGlobalVar($"{productNumber}_PollData", pollDataJson, false);
                CPH.SetGlobalVar($"{productNumber}_PollActive", true, false);

                SendMessageToChat(CPH, productSettings.PollStartedMessage, productSettings, pollData, args);
                SendMessageToChat(CPH, productSettings.PollInstructionMessage, productSettings, pollData, args);

                int choiceNumber = 1;
                foreach (var choice in pollData.Choices)
                {
                    SendMessageToChat(CPH, productSettings.PollChoicesMessage, productSettings, pollData, args, choiceNumber - 1);
                    choiceNumber++;
                }

                CPH.SUPollStartCountdown(args, productNumber, pollData.Duration);
                CPH.TriggerEvent("StreamUP Poll Created", true);
            }
        }

        private static void LoadProductSettings(this IInlineInvokeProxy CPH, string productNumber = "sup043")
        {
            productSettings = JsonConvert.DeserializeObject<ProductSettings>(CPH.GetGlobalVar<string>($"{productNumber}_ProductSettings", true));
        }

        private static Form CreateForm()
        {
            var settingsForm = new Form
            {
                Text = "StreamUP | Poll System",
                Width = 400,
                Height = 500,
                FormBorderStyle = FormBorderStyle.Fixed3D,
                MaximizeBox = false,
                MinimizeBox = true,
                StartPosition = FormStartPosition.CenterParent
            };

            byte[] bytes = Convert.FromBase64String(UIResources.supIconString);
            using var ms = new MemoryStream(bytes);
            settingsForm.Icon = new Icon(ms);

            return settingsForm;
        }

        private static TableLayoutPanel CreateMainLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                AutoScroll = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var description = new Label
            {
                Text = "You can create a poll for your stream below. Just give it a title and how long you want it to last.\n\nYou must have between 2 and 5 choices.",
                MaximumSize = new Size(380, 0),
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };
            mainLayout.Controls.Add(description, 0, 0);

            return mainLayout;
        }

        private static TableLayoutPanel CreateSettingsTable()
        {
            var settingsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoScroll = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Name = "settingsTable"
            };
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            return settingsTable;
        }

        private static void AddSettings(TableLayoutPanel settingsTable, PollData pollData)
        {
            int settingsIndex = 0;

            settingsIndex = SUPollAddSetting("ruler", new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Width = 496,
                Margin = new Padding(10, 10, 10, 10)
            }, settingsTable, settingsIndex);

            var questionTextBox = new TextBox
            {
                MaxLength = 60,
                Dock = DockStyle.Fill,
                Name = "PollQuestion"
            };
            settingsIndex = SUPollAddSetting("Poll Question:", questionTextBox, settingsTable, settingsIndex);

            var pollTimeInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1800,
                Value = 30,
                Dock = DockStyle.Fill,
                Name = "PollTime"
            };
            settingsIndex = SUPollAddSetting("Poll Time (seconds):", pollTimeInput, settingsTable, settingsIndex);

            for (int i = 1; i <= 5; i++)
            {
                var choiceTextBox = new TextBox
                {
                    MaxLength = 25,
                    Dock = DockStyle.Fill,
                    Name = $"Choice{i}"
                };
                settingsIndex = SUPollAddSetting($"Choice {i}:", choiceTextBox, settingsTable, settingsIndex);
            }

            settingsIndex = SUPollAddSetting("ruler", new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Width = 496,
                Margin = new Padding(10, 10, 10, 10)
            }, settingsTable, settingsIndex);

            var platformTwitchCheckBox = new CheckBox { Checked = true, Name = "SendToTwitch" };
            settingsIndex = SUPollAddSetting("Send Poll to Twitch:", platformTwitchCheckBox, settingsTable, settingsIndex);
            var platformYouTubeCheckBox = new CheckBox { Checked = true, Name = "SendToYouTube" };
            settingsIndex = SUPollAddSetting("Send Poll to YouTube:", platformYouTubeCheckBox, settingsTable, settingsIndex);
        }

        private static Panel CreateButtonPanel(Form settingsForm, PollData pollData)
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };
            SUPollAddButtonControls(buttonPanel, settingsForm, pollData);
            return buttonPanel;
        }

        private static StatusStrip CreateStatusBar()
        {
            var statusBar = new StatusStrip
            {
                SizingGrip = false
            };
            var statusLabel = new ToolStripStatusLabel
            {
                Text = "© StreamUP"
            };
            statusBar.Items.Add(statusLabel);
            return statusBar;
        }

        private static void SUPollAddButtonControls(Panel buttonPanel, Form withParent, PollData pollData)
        {
            var cancelButton = new Button
            {
                Font = new Font("Segoe UI Emoji", 10),
                Text = "❌ Cancel",
                BackColor = Color.LightGray,
                Width = 100,
                Dock = DockStyle.Right
            };
            cancelButton.Click += (sender, e) => {
                pollData.Started = false;
                withParent.Close();
            };

            var startButton = new Button
            {
                Font = new Font("Segoe UI Emoji", 10),
                Text = "✔️ Start",
                BackColor = Color.LightGreen,
                Width = 100,
                Dock = DockStyle.Right
            };
            startButton.Click += (sender, e) =>
            {
                // Gather data from the form controls
                pollData.Id = Guid.NewGuid().ToString();
                pollData.Title = ((TextBox)withParent.Controls.Find("PollQuestion", true)[0]).Text;
                pollData.Duration = (int)((NumericUpDown)withParent.Controls.Find("PollTime", true)[0]).Value;
                pollData.Choices = new List<PollChoiceData>();
                pollData.SendToTwitch = ((CheckBox)withParent.Controls.Find("SendToTwitch", true)[0]).Checked;
                pollData.SendToYouTube = ((CheckBox)withParent.Controls.Find("SendToYouTube", true)[0]).Checked;
                pollData.Started = true;

                int choiceNumber = 1;

                foreach (var control in ((TableLayoutPanel)withParent.Controls.Find("settingsTable", true)[0]).Controls)
                {
                    if (control is TextBox choiceTextBox && choiceTextBox.Name.StartsWith("Choice"))
                    {
                        if (!string.IsNullOrWhiteSpace(choiceTextBox.Text))
                        {
                            var choice = new PollChoiceData
                            {
                                Id = Guid.NewGuid().ToString(),
                                Title = $"{choiceNumber}: {choiceTextBox.Text}",
                                PremiumVotes = 0,
                                TotalVotes = 0,
                                StandardVotes = 0,
                                Win = false,
                                Percentage = 0.0
                            };

                            pollData.Choices.Add(choice);
                            choiceNumber++;
                        }
                    }
                }

                pollData.ChoiceCount = pollData.Choices.Count;

                if (pollData.ChoiceCount < 2)
                {
                    MessageBox.Show("Please fill out at least two choices.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                withParent.Close();
            };

            buttonPanel.Controls.Add(startButton);
            buttonPanel.Controls.Add(cancelButton);
        }

        private static int SUPollAddSetting(string labelText, Control control, TableLayoutPanel settingsTable, int settingsIndex)
        {
            settingsIndex++;
            var label = new Label
            {
                Text = labelText,
                Padding = new Padding(0, 4, 0, 0),
                AutoSize = true,
                MaximumSize = new Size(150, 0)
            };

            if (labelText == "ruler")
            {
                settingsTable.Controls.Add(control, 0, settingsIndex);
                settingsTable.SetColumnSpan(control, 2);
            }
            else
            {
                settingsTable.Controls.Add(label, 0, settingsIndex);
                settingsTable.Controls.Add(control, 1, settingsIndex);
            }

            return settingsIndex;
        }



        public static void SUPollUpdate(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber = "sup043")
        {
            if (!CPH.GetGlobalVar<bool>($"{productNumber}_PollActive", false))
            {
                CPH.SUWriteLog("Poll is not active.");
                return;
            }

            // Load current poll data
            PollData pollData = JsonConvert.DeserializeObject<PollData>(CPH.GetGlobalVar<string>($"{productNumber}_PollData", false));
            pollData.EventType = "StreamUPPollUpdated";

            string userType = CPH.SUSBTryGetArgOrDefault<string>("userType");
            if (bool.Parse(args["isTest"].ToString()))
            {
                Random random = new Random();
                string[] platforms = { "twitch", "youtube" };
                userType = platforms[random.Next(platforms.Length)];
            }

            switch (userType)
            {
                case "twitch":
                    if (!pollData.SendToTwitch)
                    {
                        CPH.SUWriteLog("Twitch user has voted but the poll wasn't sent to Twitch");
                        return;
                    }
                    break;
                case "youtube":
                    if (!pollData.SendToYouTube)
                    {
                        CPH.SUWriteLog("YouTube user has voted but the poll wasn't sent to YouTube");
                        return;
                    }
                    break;
            }

            string user = CPH.SUSBTryGetArgOrDefault<string>("user");
            if (string.IsNullOrEmpty(user))
            {
                CPH.SUWriteLog("User is not specified.");
                return;
            }

            // Create a unique identifier for the user
            string uniqueUser = $"{userType}_{user}";

            // Load the list of users who have voted
            List<string> usersVoted = CPH.GetGlobalVar<List<string>>($"{productNumber}_UsersVoted", false);
            if (usersVoted == null)
            {
                usersVoted = new List<string>();
            }

            if (usersVoted.Contains(uniqueUser))
            {
                SendMessageToChat(CPH, productSettings.PollAlreadyVotedMessage, productSettings, pollData, args);
                CPH.SUWriteLog($"{user} ({userType}) has already voted.");
                return;
            }

            // Check if the vote choice is parsable
            string voteNumber = "0";
            if (bool.Parse(args["isTest"].ToString()))
            {
                Random random = new Random();
                int randomVoteNumber = random.Next(1, pollData.ChoiceCount + 1);
                voteNumber = randomVoteNumber.ToString();
            }
            else
            {
                voteNumber = CPH.SUSBTryGetArgOrDefault<string>("input0");
            }

            if (!int.TryParse(voteNumber, out int number))
            {
                // Not a valid vote number
                CPH.SUWriteLog("The input is not a valid number.");
                return;
            }
            CPH.SUWriteLog($"number={number}, choiceCount={pollData.ChoiceCount}");
            if (number > pollData.ChoiceCount || number <= 0)
            {
                // Not a valid vote number
                CPH.SUWriteLog("The input is not within the valid range.");
                return;
            }

            // Increment the vote count for the chosen choice
            PollChoiceData chosenChoice = pollData.Choices[number - 1];
            chosenChoice.TotalVotes++;
            chosenChoice.StandardVotes++;
            pollData.VotesTotal++;
            pollData.VotesStandardTotal++;

            if (userType == "twitch")
            {
                chosenChoice.TwitchTotalVotes++;
                pollData.TwitchTotalVotes++;
            }
            else if (userType == "youtube")
            {
                chosenChoice.YouTubeTotalVotes++;
                pollData.YouTubeTotalVotes++;
            }

            // Recalculate the percentage for each choice
            foreach (var choice in pollData.Choices)
            {
                choice.Percentage = (double)choice.TotalVotes / pollData.VotesTotal * 100;
            }

            // Get duration left
            pollData.DurationRemaining = CPH.GetGlobalVar<int>($"{productNumber}_TimeRemaining", false);
            pollData.DurationRemainingMs = pollData.DurationRemaining * 1000;

            // Update the global variable with the new poll data
            string updatedPollDataJson = JsonConvert.SerializeObject(pollData);
            CPH.SetGlobalVar($"{productNumber}_PollData", updatedPollDataJson, false);

            // Add the unique user identifier to the list of users who have voted
            usersVoted.Add(uniqueUser);
            CPH.SetGlobalVar($"{productNumber}_UsersVoted", usersVoted, false);

            CPH.SUWriteLog("POLL UPDATED");

            // Trigger event if voteNumber is a valid number within range
            CPH.TriggerEvent("StreamUP Poll Updated", true);
        }

        public static void SUPollTerminate(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber = "sup043")
        {
            if (!CPH.GetGlobalVar<bool>($"{productNumber}_PollActive",false))
            {
                CPH.SUWriteLog("Poll is not active.");
                return;
            }

            CPH.SetGlobalVar($"{productNumber}_PollActive", false, false);

            // Stop the timer and set the remaining time to 0
            StopPollTimer(CPH, productNumber);

            // Load current poll data
            PollData pollData = JsonConvert.DeserializeObject<PollData>(CPH.GetGlobalVar<string>($"{productNumber}_PollData", false));
            pollData.EventType = "StreamUPPollTerminated";
            pollData.TimeEnded = new DateTime();
            pollData.DurationRemaining = 0;
            pollData.DurationRemainingMs = 0;

            string updatedPollDataJson = JsonConvert.SerializeObject(pollData);
            CPH.SetGlobalVar($"{productNumber}_PollData", updatedPollDataJson, false);
            SendMessageToChat(CPH, productSettings.PollTerminatedMessage, productSettings, pollData, args);
            CPH.SUWriteLog("POLL TERMINATED");
            CPH.TriggerEvent("StreamUP Poll Terminated", true);
            CPH.UnsetGlobalVar("sup043_UsersVoted", false);
        }

        public static void SUPollComplete(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber = "sup043")
        {
            if (!CPH.GetGlobalVar<bool>($"{productNumber}_PollActive", false))
            {
                CPH.SUWriteLog("Poll is not active.");
                return;
            }

            CPH.SetGlobalVar($"{productNumber}_PollActive", false, false);

            // Stop the timer and set the remaining time to 0
            StopPollTimer(CPH, productNumber);

            // Load current poll data
            PollData pollData = JsonConvert.DeserializeObject<PollData>(CPH.GetGlobalVar<string>($"{productNumber}_PollData", false));
            pollData.EventType = "StreamUPPollCompleted";
            pollData.TimeEnded = DateTime.Now;
            pollData.DurationRemaining = 0;
            pollData.DurationRemainingMs = 0;

            // Recalculate the percentage for each choice
            foreach (var choice in pollData.Choices)
            {
                choice.Percentage = (double)choice.TotalVotes / pollData.VotesTotal * 100;
            }

            // Determine the poll winner(s) and check for a draw
            int maxVotes = pollData.Choices.Max(choice => choice.TotalVotes);
            var winners = pollData.Choices.Where(choice => choice.TotalVotes == maxVotes).ToList();


            SendMessageToChat(CPH, productSettings.PollEndedMessage, productSettings, pollData, args);

            if (winners.Count > 1)
            {
                // It's a draw
                CPH.SUWriteLog("The poll ended in a draw.");
                foreach (var winner in winners)
                {
                    winner.Win = true;
                    SendMessageToChat(CPH, productSettings.PollWinnerMessage, productSettings, pollData, args, pollData.Choices.IndexOf(winner));
                    CPH.SUWriteLog($"One of the poll winners is: {winner.Title}");
                }
            }
            else
            {
                // There is a single winner
                var winner = winners.First();
                winner.Win = true;
                SendMessageToChat(CPH, productSettings.PollWinnerMessage, productSettings, pollData, args, pollData.Choices.IndexOf(winner));
                CPH.SUWriteLog($"The poll winner is: {winner.Title}");
            }

            // Update the global variable with the new poll data
            string updatedPollDataJson = JsonConvert.SerializeObject(pollData);
            CPH.SetGlobalVar($"{productNumber}_PollData", updatedPollDataJson, false);

            CPH.SUWriteLog("POLL COMPLETE");
            // Trigger events
            CPH.TriggerEvent("StreamUP Poll Completed", true);
            ShowPollResults(CPH);
            CPH.UnsetGlobalVar("sup043_UsersVoted", false);
        }

        public static void ShowPollResults(this IInlineInvokeProxy CPH, string productNumber = "sup043")
        {
            PollData pollData = JsonConvert.DeserializeObject<PollData>(CPH.GetGlobalVar<string>($"{productNumber}_PollData", false));

            var resultsForm = new Form
            {
                Text = "StreamUP | Poll Results",
                Width = 500,
                Height = 740,
                FormBorderStyle = FormBorderStyle.Fixed3D,
                MaximizeBox = false,
                MinimizeBox = true,
                StartPosition = FormStartPosition.CenterParent
            };

            byte[] bytes = Convert.FromBase64String(UIResources.supIconString);
            using var ms = new MemoryStream(bytes);
            resultsForm.Icon = new Icon(ms);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = pollData.Choices.Count - 1,
                ColumnCount = 2,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single 
                
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            int rowIndex = 0;
            var title = new Label
            {
                Text = $"Poll Results\n\"{pollData.Title}\"",
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            mainLayout.Controls.Add(title, 0, rowIndex);
            mainLayout.SetColumnSpan(title, 2);
            rowIndex++;

            foreach (var choice in pollData.Choices)
            {
                string percentage = double.IsNaN(choice.Percentage) ? "0%" : $"{choice.Percentage:F2}%";
                var choiceLabel = new Label
                {
                    Text = $"{choice.Title}\n{choice.TotalVotes} votes ({percentage})",
                    AutoSize = true,
                    Padding = new Padding(0, 5, 0, 5),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular)
                };
                mainLayout.Controls.Add(choiceLabel, 0, rowIndex);
                mainLayout.SetColumnSpan(choiceLabel, 2);
                rowIndex++;

                var twitchChoiceLabel = new Label
                {
                    Text = pollData.SendToTwitch ? $"Twitch Votes: {choice.TwitchTotalVotes}" : "Twitch Votes: 0",
                    AutoSize = true,
                    Padding = new Padding(0, 5, 0, 5),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    Anchor = AnchorStyles.None
                };
                mainLayout.Controls.Add(twitchChoiceLabel, 0, rowIndex);

                var youtubeChoiceLabel = new Label
                {
                    Text = pollData.SendToYouTube ? $"YouTube Votes: {choice.YouTubeTotalVotes}" : "YouTube Votes: 0",
                    AutoSize = true,
                    Padding = new Padding(0, 5, 0, 5),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    Anchor = AnchorStyles.None
                };
                mainLayout.Controls.Add(youtubeChoiceLabel, 1, rowIndex);
                rowIndex++;
            }

            var totalVotesHeader = new Label
            {
                Text = "Votes Totals",
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            mainLayout.Controls.Add(totalVotesHeader, 0, rowIndex);
            mainLayout.SetColumnSpan(totalVotesHeader, 2);
            rowIndex++;

            var twitchTotalLabel = new Label
            {
                Text = pollData.SendToTwitch ? $"Twitch Total Votes: {pollData.TwitchTotalVotes}" : "Twitch Total Votes: 0",
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Anchor = AnchorStyles.None
            };
            mainLayout.Controls.Add(twitchTotalLabel, 0, rowIndex);

            var youtubeTotalLabel = new Label
            {
                Text = pollData.SendToYouTube ? $"YouTube Total Votes: {pollData.YouTubeTotalVotes}" : "YouTube Total Votes: 0",
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Anchor = AnchorStyles.None
            };
            mainLayout.Controls.Add(youtubeTotalLabel, 1, rowIndex);
            rowIndex++;

            var finalTotalLabel = new Label
            {
                Text = $"Final Total Votes\n{pollData.VotesTotal}",
                AutoSize = true,
                Padding = new Padding(0, 3, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
            };
            mainLayout.Controls.Add(finalTotalLabel, 0, rowIndex);
            mainLayout.SetColumnSpan(finalTotalLabel, 2);
            rowIndex++;

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            var closeButton = new Button
            {
                Font = new Font("Segoe UI Emoji", 10),
                Text = "❌ Close",
                BackColor = Color.LightGray,
                Width = 100,
                Dock = DockStyle.Right,
                Padding = new Padding(10)
            };
            closeButton.Click += (sender, e) => resultsForm.Close();

            buttonPanel.Controls.Add(closeButton);

            resultsForm.Controls.Add(mainLayout);
            resultsForm.Controls.Add(buttonPanel);
            resultsForm.Controls.Add(CreateStatusBar());
            resultsForm.ShowDialog();
        }


        public static PollData SUSBProcessPoll(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, ProductInfo productInfo, PollSource pollSource)
        {
            string logName = $"{productInfo.ProductNumber}::SUSBProcessPoll";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            PollData pollData = new PollData();

            switch (pollSource)
            {
                case PollSource.StreamUPUI:
                    pollData = JsonConvert.DeserializeObject<PollData>(CPH.GetGlobalVar<string>($"sup043_PollData", false));     
                    break;
                case PollSource.StreamUPChatMessage:
                    break;
                case PollSource.Twitch:
                    pollData = SUSBProcessTwitchPollVars(CPH, sbArgs);
                    break;

            }

            // Work out percentages
            foreach (var choice in pollData.Choices)
            {
                choice.Percentage = (pollData.VotesTotal > 0) ? (double)choice.TotalVotes / pollData.VotesTotal * 100 : 0;
            }

            // Determine winner for TwitchPollCompleted event
            if (CPH.GetEventType() == EventType.TwitchPollCompleted)
            {
                int highestVotes = -1;
                foreach (var choice in pollData.Choices)
                {
                    if (choice.TotalVotes > highestVotes)
                    {
                        highestVotes = choice.TotalVotes;
                    }
                }

                foreach (var choice in pollData.Choices)
                {
                    choice.Win = (choice.TotalVotes == highestVotes);
                }
            }

            return pollData;
        }
            
        private static PollData SUSBProcessTwitchPollVars(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs)
        {
            // Load baseInfo var
            PollData pollData = new PollData();

            // Load misc vars
            string triggerType = CPH.GetEventType().ToString();
            pollData.EventType = triggerType;

            pollData.Duration = CPH.SUSBTryGetArgOrDefault<int>("poll.Duration");
            pollData.DurationMs = pollData.Duration * 1000;
            pollData.DurationRemainingMs = CPH.SUSBTryGetArgOrDefault<int>("poll.DurationRemaining");
            pollData.DurationRemaining = pollData.DurationRemainingMs / 1000;
            pollData.Id = CPH.SUSBTryGetArgOrDefault<string>("poll.Id");
            pollData.Title = CPH.SUSBTryGetArgOrDefault<string>("poll.Title");
            pollData.TimeStarted = CPH.SUSBTryGetArgOrDefault<DateTime>("poll.StartedAt");
            pollData.TimeEnded = CPH.SUSBTryGetArgOrDefault<DateTime>("poll.endedAt");
            pollData.ChoiceCount = CPH.SUSBTryGetArgOrDefault<int>("poll.choices.count");
            pollData.VotesPremiumTotal = CPH.SUSBTryGetArgOrDefault<int>("poll.rewardVotes");
            pollData.VotesStandardTotal = CPH.SUSBTryGetArgOrDefault<int>("poll.votes");
            pollData.VotesTotal = CPH.SUSBTryGetArgOrDefault<int>("poll.totalVotes");

            // Loop to add choices dynamically
            pollData.Choices = new List<PollChoiceData>();
            for (int i = 0; i < pollData.ChoiceCount; i++)
            {
                var choice = new PollChoiceData
                {
                    Id = CPH.SUSBTryGetArgOrDefault<string>($"poll.choice{i}.id"),
                    Title = CPH.SUSBTryGetArgOrDefault<string>($"poll.choice{i}.title"),
                    PremiumVotes = CPH.SUSBTryGetArgOrDefault<int>($"poll.choice{i}.rewardVotes"),
                    TotalVotes = CPH.SUSBTryGetArgOrDefault<int>($"poll.choice{i}.totalVotes"),
                    StandardVotes = CPH.SUSBTryGetArgOrDefault<int>($"poll.choice{i}.votes"),
                    Win = false
                };
                pollData.Choices.Add(choice);
            }
            return pollData;
        }

        private static Timer pollCountdownTimer;
        private static int pollRemainingSeconds;

        public static void SUPollStartCountdown(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber, int duration)
        {
            pollRemainingSeconds = duration;

            CPH.SUWriteLog($"Poll timer started! [{duration} seconds]");
            pollCountdownTimer = new Timer(1000);
            pollCountdownTimer.Elapsed += (sender, e) => OnTimerElapsed(CPH, args, productNumber, sender, e);
            pollCountdownTimer.AutoReset = true;
            pollCountdownTimer.Enabled = true;

            CPH.SetGlobalVar($"{productNumber}_TimeRemaining", pollRemainingSeconds, false);
        }

        private static void OnTimerElapsed(this IInlineInvokeProxy CPH, Dictionary<string, object> args, string productNumber, object sender, ElapsedEventArgs e)
        {
            if (pollRemainingSeconds > 0)
            {
                pollRemainingSeconds--;

                CPH.SetGlobalVar($"{productNumber}_TimeRemaining", pollRemainingSeconds, false);
            }
            else
            {
                CPH.UnsetGlobalVar($"{productNumber}_TimeRemaining", false);
                CPH.SUPollComplete(args, productNumber);
                StopPollTimer(CPH, productNumber);
            }
        }

        private static void StopPollTimer(IInlineInvokeProxy CPH, string productNumber)
        {
            if (pollCountdownTimer != null)
            {
                pollCountdownTimer.Stop();
                pollCountdownTimer.Dispose();
                pollCountdownTimer = null;
            }

            pollRemainingSeconds = 0;
            CPH.SetGlobalVar($"{productNumber}_TimeRemaining", pollRemainingSeconds, false);
        }
    
    
    }



    [Serializable]
    public class PollData
    {
        public int Duration { get; set; }
        public int DurationMs { get; set; }
        public int DurationRemaining { get; set; }
        public int DurationRemainingMs { get; set; }
        public string EventType { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public int VotesPremiumTotal { get; set; }
        public int VotesStandardTotal { get; set; }
        public int VotesTotal { get; set; }
        public int ChoiceCount { get; set; }
        public List<PollChoiceData> Choices { get; set; }
        public bool SendToTwitch { get; set; }
        public bool SendToYouTube { get; set; }
        public int TwitchTotalVotes { get; set; }
        public int YouTubeTotalVotes { get; set; }
        public bool Started { get; set; }
        public override string ToString()
        {
            var choicesString = string.Join(Environment.NewLine, Choices.Select((c, index) => $"    PollChoiceData {index + 1}:\n{c}"));
            return $"PollData: \n" +
                $"  Duration: {Duration}\n" +
                $"  DurationRemaining: {DurationRemaining}\n" +
                $"  EventType: {EventType}\n" +
                $"  Id: {Id}\n" +
                $"  TimeStarted: {TimeStarted}\n" +
                $"  TimeEnded: {TimeEnded}\n" +
                $"  VotesPremiumTotal: {VotesPremiumTotal}\n" +
                $"  VotesStandardTotal: {VotesStandardTotal}\n" +
                $"  VotesTotal: {VotesTotal}\n" +
                $"  ChoiceCount: {ChoiceCount}\n" +
                $"  Choices: \n{choicesString}";
        }    
    }

    public class PollChoiceData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int PremiumVotes { get; set; }
        public int TotalVotes { get; set; }
        public int StandardVotes { get; set; }
        public bool Win { get; set; }
        public double Percentage { get; set; }
        public int TwitchTotalVotes { get; set; }
        public int YouTubeTotalVotes { get; set; }

        public override string ToString()
        {
            return  $"      Id: {Id}\n" +
                    $"      Title: {Title}\n" +
                    $"      PremiumVotes: {PremiumVotes}\n" +
                    $"      TotalVotes: {TotalVotes}\n" +
                    $"      StandardVotes: {StandardVotes}\n" +
                    $"      Win: {Win}\n" +
                    $"      Percentage: {Percentage}";
        }
    }

    public enum PollSource
    {
        StreamUPUI = 0,
        StreamUPChatMessage = 1,        
        Twitch = 2
    }

    public class ProductSettings
    {
        public string PollAlreadyActiveMessage { get; set; }
        public string PollStartedMessage { get; set; }
        public string PollChoicesMessage { get; set; }
        public string PollInstructionMessage { get; set; }
        public string PollEndedMessage { get; set; }
        public string PollWinnerMessage { get; set; }
        public string PollTerminatedMessage { get; set; }
        public string PollAlreadyVotedMessage { get; set; }
        public bool BotAccount { get; set; }
        public bool ResultsOutputUI { get; set; }
    }

}