using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public void ShowSettingsSplashScreen(string title)
        {
            Thread thread = new(() =>
            {
                Form messageForm = new()
                {
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterScreen,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    Text = title
                };

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
                    var description = new PictureBox
                    {
                        Image = image,
                        SizeMode = PictureBoxSizeMode.AutoSize,
                        Dock = DockStyle.Top
                    };
                    formWidth += image.Width;
                    formHeight += image.Height;

                    // Add the PictureBox to the form
                    messageForm.Controls.Add(description);
                }

                ProgressBar progressBar = new()
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
                System.Windows.Forms.Timer timer = new()
                {
                    Interval = 200
                };
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
            })
            {
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    
    
    }
}