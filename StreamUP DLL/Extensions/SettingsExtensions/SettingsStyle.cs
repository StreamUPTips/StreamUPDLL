using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public TableLayoutPanel settingsTable;
        private FlowLayoutPanel buttonPanel;
        private TabControl tabControl;
        private Button saveButton;
        private Button resetButton;
        private ToolTip toolTip = new ToolTip();
        private Random random = new Random();

        private readonly Color backColour1 = ColorTranslator.FromHtml("#121212");
        private readonly Color backColour2 = ColorTranslator.FromHtml("#676767");
        private readonly Color backColour3 = ColorTranslator.FromHtml("#212121");
        private readonly Color forecolour1 = Color.WhiteSmoke;
        private readonly Color forecolour2 = Color.SkyBlue;
        private readonly Color forecolour3 = Color.Black;
        private readonly Color linkColour = ColorTranslator.FromHtml("#FF86BD");
        private readonly Color boolTrueColor = Color.SeaGreen;
        private readonly Color boolFalseColor = Color.IndianRed;
        private readonly Font entryFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular);
        private readonly Font valueFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private readonly Font labelFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular);
        private readonly Font headingFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Bold);
        private readonly Font descriptionFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Italic);
        private readonly Font linkFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private readonly Font spaceFont = new Font("Segoe UI Emoji", 15.0F, FontStyle.Bold);
        private readonly Font buttonFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Bold);
        private readonly Font buttonUIFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private readonly Font buttonFileFont = new Font("Segoe UI Emoji", 15.0F, FontStyle.Bold);
        private readonly Font tabFont = new Font("Segoe UI Emoji", 11.0F, FontStyle.Bold);
        private readonly Font productInfoFont = new Font("Monospace", 8.0F, FontStyle.Bold);
        //private SplashScreen splashScreen;


    }
}