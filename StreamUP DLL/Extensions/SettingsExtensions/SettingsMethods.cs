using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        //! Headings And Misc
         public List<Control> AddHeading(string headingText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var heading = new Label
            {
                Text = headingText,
                Font = headingFont,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                ForeColor = forecolour2,
                Tag = tabName
            };

            settings.Add(heading); // Add the label directly to the list
            return settings;
        }
        public List<Control> AddLabel(string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Font = labelFont,
                ForeColor = forecolour1,
                Tag = tabName

            };

            settings.Add(label);
            return settings;
        }

        public List<Control> AddInfo(string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var label = new Label
            {
                Name = "thisisjustaline",
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Font = labelFont,
                ForeColor = forecolour1,
                Tag = tabName

            };

            settings.Add(label);
            return settings;
        }

        public List<Control> AddLink(string labelText, string url, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            LinkLabel linkLabel = new LinkLabel
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                LinkColor = linkColour,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Font = linkFont,
                Tag = tabName
            };

            linkLabel.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(url);

            settings.Add(linkLabel);
            return settings;
        }
        public List<Control> AddLine(string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var rule = new Label
            {
                Name = "thisisjustaline",
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Dock = DockStyle.Top,
                ForeColor = forecolour1,
                Tag = tabName,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = false,
            };

            settings.Add(rule);
            return settings;

        }
        public List<Control> AddDescription(string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();


            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Font = descriptionFont,
                Tag = tabName
            };

            settings.Add(label); // Add control to the list
            return settings;
        }
        public List<Control> AddSpace(string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var space = new Label
            {
                Name = "thisisjustaline",
                Text = Environment.NewLine,
                Font = spaceFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Tag = tabName,


            };

            settings.Add(space);
            return settings;

        }
        public List<Control> AddEnd(string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var space = new Label
            {

                Text = Environment.NewLine,
                Font = spaceFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Tag = tabName,


            };

            settings.Add(space);
            return settings;

        }


        //! TEXT Based
        public List<Control> AddTextBox(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));


            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };



            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);


            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddMultiLineTextBox(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(0),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Multiline = true,
                Height = 100,
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,


            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddLimitText(string description, string defaultValue, int textLimit, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                MaxLength = textLimit,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,



            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddList(string description, List<string> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("Items", "Items");
            List<string> rowsToAdd = GetSetting<List<string>>(saveName, defaultValue);
            foreach (string entry in rowsToAdd)
            {
                input.Rows.Add(entry); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddPassword(string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName



            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, ""),
                UseSystemPasswordChar = true,
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                Height = 23,

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddDropDown(string description, string[] dropdown, string defaultValue, string saveName, string tabName = "General")
        {

            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };

            var input = new ComboBox
            {
                Name = saveName,
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,


            };

            input.Items.AddRange(dropdown);
            input.SelectedItem = GetSetting<string>(saveName, defaultValue);

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;



        }
        public List<Control> AddStringDictionary(string description, string columnOneName, string columnTwoName, Dictionary<string, string> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = true,
                RowHeadersVisible = true,
                RowHeadersWidth = 25,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("columnOne", columnOneName);

            input.Columns.Add("columnTwo", columnTwoName);
            Dictionary<string, string> rowsToAdd = GetSetting<Dictionary<string, string>>(saveName, defaultValue);
            foreach (KeyValuePair<string, string> entry in rowsToAdd)
            {
                input.Rows.Add(entry.Key, entry.Value); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 0, 1);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        //! NUMBERS
        public List<Control> AddInt(string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = GetSetting<int>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Left,


            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddDecimal(string description, double defaultValue, int decimalPlaces, double increments, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                DecimalPlaces = decimalPlaces,
                Increment = (decimal)increments,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                Value = GetSetting<decimal>(saveName, (decimal)defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddTrackbar(string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column3Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column4Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var input = new TrackBar
            {

                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = GetSetting<int>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TickFrequency = (max - min) / 2, // Adjust according to your preference
                TickStyle = TickStyle.TopLeft

            };


            var valueLabel = new Label
            {
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                Text = input.Value.ToString(),
                ForeColor = forecolour2,
                Font = valueFont,
                TextAlign = ContentAlignment.BottomCenter,

            };

            input.ValueChanged += (sender, e) =>
            {
                valueLabel.Text = input.Value.ToString();
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddIntDictionary(string description, string columnOneName, string columnTwoName, Dictionary<string, int> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = true,
                RowHeadersVisible = true,
                RowHeadersWidth = 25,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("columnOne", columnOneName);

            input.Columns.Add("columnTwo", columnTwoName);
            Dictionary<string, int> rowsToAdd = GetSetting<Dictionary<string, int>>(saveName, defaultValue);
            foreach (KeyValuePair<string, int> entry in rowsToAdd)
            {
                input.Rows.Add(entry.Key, entry.Value); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 0, 1);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        //! BOOLS
        public List<Control> AddTrueFalse(string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                // Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = GetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = GetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = GetSetting<bool>(saveName, defaultValue) ? "True" : "False",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "True" : "False";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddYesNo(string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = GetSetting<bool>(saveName, defaultValue),
                // Padding = new Padding(0),
                // Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = GetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = GetSetting<bool>(saveName, defaultValue) ? "Yes" : "No",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "Yes" : "No";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddOnOff(string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,

                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = GetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = GetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = GetSetting<bool>(saveName, defaultValue) ? "On" : "Off",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "On" : "Off";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddCustomBool(string description, bool defaultValue, string trueName, string trueColor, string falseName, string falseColor, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            Color boolCustomTrueColor = ColorTranslator.FromHtml(trueColor);
            Color boolCustomFalseColor = ColorTranslator.FromHtml(falseColor);
            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                // Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = GetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolCustomFalseColor,
                BoolTrueColor = boolCustomTrueColor,
                BackColor = GetSetting<bool>(saveName, defaultValue) ? boolCustomTrueColor : boolCustomFalseColor,
                Text = GetSetting<bool>(saveName, defaultValue) ? trueName : falseName,
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolCustomTrueColor : boolCustomFalseColor;
                input.Text = input.Checked ? trueName : falseName;
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddChecklist(string description, Dictionary<string, bool> checkListItems, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new CheckedListBox
            {
                Name = saveName,
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour1,
                ForeColor = forecolour1,
                BorderStyle = BorderStyle.None,

                Width = 280,
                CheckOnClick = true,
                Font = entryFont,

            };



            Dictionary<string, bool> checkedItemsDict = GetSetting<Dictionary<string, bool>>(saveName, checkListItems);


            foreach (KeyValuePair<string, bool> checkItem in checkedItemsDict)
            {

                input.Items.Add(checkItem.Key, checkItem.Value);

            }
            input.Height = checkedItemsDict.Count * 20;





            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;

        }
        //! OTHERS
        public List<Control> AddColour(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column3Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column4Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new Label
            {
                Name = saveName + "OBS",
                AutoSize = true,
                Margin = new Padding(0, 6, 10, 0),
                Text = GetSetting<string>(saveName + "OBS", _CPH.ObsConvertColorHex(defaultValue).ToString()),
                ForeColor = forecolour2,
                Font = valueFont,
                TextAlign = ContentAlignment.BottomCenter,

            };
            var defaultColour = GetSetting<string>(saveName + "HTML", defaultValue);
            var input = new RoundedButton
            {
                Name = saveName + "HTML",
                Text = defaultColour,
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                BackColor = ColorTranslator.FromHtml(defaultColour),
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(30, 30),
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
                CornerRadius = 8,
                Cursor = Cursors.Hand
            };
            input.FlatAppearance.BorderSize = 0;

            // Event handler for button click to open color picker
            input.Click += (sender, e) =>
            {
                // Here, you should implement code to open a color picker dialog
                // and update the button text and value label with the selected color
                // For example:
                var colorDialog = new ColorDialog();
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedColor = colorDialog.Color;
                    input.BackColor = selectedColor;
                    input.ForeColor = selectedColor.GetBrightness() < 0.5 ? forecolour1 : forecolour3;
                    int alpha = selectedColor.A;
                    int green = selectedColor.G;
                    int blue = selectedColor.B;
                    int red = selectedColor.R;
                    long obsColour =  _CPH.ObsConvertRgb(alpha,red,green,blue);
                    valueLabel.Text = $"{obsColour}";
                    string hexValue = selectedColor.R.ToString("X2") + selectedColor.G.ToString("X2") + selectedColor.B.ToString("X2");
                    input.Text = "#" + hexValue;
                    Console.WriteLine($"Selected color: ARGB({selectedColor.A}, {selectedColor.R}, {selectedColor.G}, {selectedColor.B})");
                }
            };



            valueLabel.Click += (sender, e) =>
            {

                var text = valueLabel.Text;
                Thread thread = new Thread(() => Clipboard.SetText(text));
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end
            };



            toolTip.SetToolTip(input, "Pick a Colour");
            toolTip.SetToolTip(valueLabel, "Click to Copy");

            SetButtonColor(input, defaultColour);

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddColor(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();


            settings.AddRange(AddColour(description, defaultValue, saveName, tabName));


            return settings;
        }
        public List<Control> AddFile(string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column3Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column4Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, ""),
                //Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };

            var input = new RoundedButton
            {
                Text = "🗁",
                AutoSize = true,
                //Margin = new Padding(10, 10, 0, 0),
                Size = new System.Drawing.Size(20, 20),
                ForeColor = forecolour2,
                Font = buttonFileFont,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.TopLeft,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK // Set DialogResult
            };
            input.FlatAppearance.BorderSize = 0;
            //input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            //input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            toolTip.SetToolTip(input, "Click to open explorer");

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFileDialogAsync();
                if (filePath != null)
                {
                    valueLabel.Text = filePath;
                    Console.WriteLine(filePath);
                }
            };


            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(valueLabel, 1, 0);
            settingsTable.Controls.Add(input, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddFolder(string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column3Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column4Percent));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new CustomTextBox
            {
                Name = saveName,
                Text = GetSetting<string>(saveName, ""),
                //Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };

            var input = new RoundedButton
            {
                Text = "🗁",
                AutoSize = true,
                //Margin = new Padding(10, 10, 0, 0),
                Size = new System.Drawing.Size(20, 20),
                ForeColor = forecolour2,
                Font = buttonFileFont,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.TopLeft,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK // Set DialogResult
            };
            input.FlatAppearance.BorderSize = 0;
            //input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            //input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            toolTip.SetToolTip(input, "Click to open explorer");

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFolderDialogAsync();
                if (filePath != null)
                {
                    valueLabel.Text = filePath;
                    Console.WriteLine(filePath);
                }
            };


            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(valueLabel, 1, 0);
            settingsTable.Controls.Add(input, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddFont(string description, string defaultName, string defaultSize, string defaultStyle, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column3Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column4Percent));



            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };


            var input = new RoundedButton
            {
                Name = saveName + "Name",
                Text = GetSetting<string>(saveName + "Name", defaultName),
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = buttonFont,
                Size = new System.Drawing.Size(280, 30),
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            input.FlatAppearance.BorderSize = 0;
            input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            var size = new Label
            {
                Name = saveName + "Size",
                Text = GetSetting<string>(saveName + "Size", defaultSize),
                AutoSize = true,
                //Margin = new Padding(10),
                Font = valueFont,
                ForeColor = forecolour2,

            };
            var style = new Label
            {
                Name = saveName + "Style",
                Text = GetSetting<string>(saveName + "Style", defaultStyle),
                AutoSize = true,
                //Margin = new Padding(10),
                Font = valueFont,
                ForeColor = forecolour2,

            };

            // Event handler for button click to open color picker
            input.Click += (sender, e) =>
            {
                // Here, you should implement code to open a color picker dialog
                // and update the button text and value label with the selected color
                // For example:
                var fontDialog = new FontDialog();
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {

                    input.Text = fontDialog.Font.Name.ToString();
                    size.Text = fontDialog.Font.Size.ToString();
                    style.Text = fontDialog.Font.Style.ToString();

                }
            };



            toolTip.SetToolTip(input, "Select a font");

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(size, 2, 1);
            settingsTable.Controls.Add(style, 1, 1);


            settings.Add(settingsTable);

            return settings;
        }
        public List<Control> AddRunAction(string description, string buttonText, string actionName, bool runImmediately, string tabName = "General")
        {


            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                //AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };



            var input = new RoundedButton
            {
                Text = $"{buttonText}",
                //Margin = new Padding(0, 10, 0, 0),
                Size = new System.Drawing.Size(280, 30),
                Font = buttonFont,
                //AutoSize = true,
                ForeColor = forecolour1,
                BackColor = backColour2,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
            };
            input.FlatAppearance.BorderSize = 0;
            input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            input.FlatAppearance.MouseDownBackColor = backColour1; // Click color
            input.Click += (sender, e) =>
            {
                RunActionButton(actionName, runImmediately);
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            toolTip.SetToolTip(input, "Run the Action");
            // Add the table layout to the list of controls
            settings.Add(settingsTable);
            return settings;



        }

         public List<Control> AddRunMethod(string description, string buttonText, string executeCode, string methodName, string tabName = "General")
        {


            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column1Percent));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, column2Percent));

            var label = new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                //AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };



            var input = new RoundedButton
            {
                Text = $"{buttonText}",
                //Margin = new Padding(0, 10, 0, 0),
                Size = new System.Drawing.Size(280, 30),
                Font = buttonFont,
                //AutoSize = true,
                ForeColor = forecolour1,
                BackColor = backColour2,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
            };
            input.FlatAppearance.BorderSize = 0;
            input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            input.FlatAppearance.MouseDownBackColor = backColour1; // Click color
            input.Click += (sender, e) =>
            {
                _CPH.ExecuteMethod(executeCode,methodName);
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            toolTip.SetToolTip(input, "Run the Action");
            // Add the table layout to the list of controls
            settings.Add(settingsTable);
            return settings;



        }

        public List<Control> AddActionDrop(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<ActionData> actions = _CPH.GetActions();
            List<string> actionDropDown = new List<string>();
            foreach (ActionData action in actions)
            {
                actionDropDown.Add(action.Name);
            }

            string[] actionsArray = actionDropDown.ToArray();
            Array.Sort(actionsArray);
            List<Control> settings = new List<Control>();
            settings.AddRange(AddDropDown(description, actionsArray, defaultValue, saveName, tabName));


            return settings;
        }
        public List<Control> AddRewardDrop(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<TwitchReward> rewards = _CPH.TwitchGetRewards();
            List<string> rewardDropdown = new List<string>();
            foreach (TwitchReward reward in rewards)
            {
                rewardDropdown.Add(reward.Title);
            }

            string[] rewardArray = rewardDropdown.ToArray();
            Array.Sort(rewardArray);
            List<Control> settings = new List<Control>();
            settings.AddRange(AddDropDown(description, rewardArray, defaultValue, saveName, tabName));


            return settings;
        }
        
        public List<Control> AddGroupsDrop(string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<string> groups = _CPH.GetGroups();
            string[] groupsArray = groups.ToArray();
            Array.Sort(groupsArray);
            List<Control> settings = new List<Control>();
            settings.AddRange(AddDropDown(description, groupsArray, defaultValue, saveName, tabName));
            return settings;
        }

    }
}