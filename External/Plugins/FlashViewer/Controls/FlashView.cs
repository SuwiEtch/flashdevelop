using System;
using System.Windows.Forms;
using AxShockwaveFlashObjects;
using PluginCore.Managers;
using System.IO;
using System.Xml;

namespace FlashViewer.Controls
{
    public class FlashView : UserControl
    {
        string moviePath;
        AxShockwaveFlash flashMovie;

        public FlashView(string file)
        {
            this.moviePath = file;
            this.InitializeComponent();
        }

        #region Component Designer Generated Code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlashView));
            this.flashMovie = new AxShockwaveFlashObjects.AxShockwaveFlash();
            ((System.ComponentModel.ISupportInitialize)(this.flashMovie)).BeginInit();
            this.SuspendLayout();
            // 
            // flashMovie
            // 
            this.flashMovie.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flashMovie.Enabled = true;
            this.flashMovie.Location = new System.Drawing.Point(0, 0);
            this.flashMovie.Name = "flashMovie";
            this.flashMovie.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("flashMovie.OcxState")));
            this.flashMovie.Size = new System.Drawing.Size(571, 367);
            this.flashMovie.TabIndex = 0;
            // 
            // FlashView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flashMovie);
            this.Name = "FlashView";
            this.Load += this.FlashViewLoad;
            this.Size = new System.Drawing.Size(571, 367);
            ((System.ComponentModel.ISupportInitialize)(this.flashMovie)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        
        #region Methods And Event Handlers

        /// <summary>
        /// Accessor for the movie file path
        /// </summary>
        public string MoviePath
        {
            get => this.moviePath;
            set 
            { 
                this.moviePath = value;
                this.flashMovie.LoadMovie(0, this.moviePath); 
            }
        }

        /// <summary>
        /// Accessor for the flash movie
        /// </summary>
        public AxShockwaveFlash FlashMovie => this.flashMovie;

        /// <summary>
        /// Initializes the control
        /// </summary>
        void FlashViewLoad(object sender, EventArgs e)
        {
            this.flashMovie.FSCommand += this.FlashMovieFSCommand;
            this.flashMovie.FlashCall += this.FlashMovieFlashCall;
            if (this.moviePath != null)
            {
                this.flashMovie.LoadMovie(0, this.moviePath);
            }
        }

        /// <summary>
        /// Handles the FSCommand event
        /// </summary>
        void FlashMovieFSCommand(object sender, _IShockwaveFlashEvents_FSCommandEvent e)
        {
            try
            {
                if (e.command == "trace")
                {
                    int state = 1;
                    string message = e.args;
                    if (message.Length > 2 && message[1] == ':' && char.IsDigit(message[0]))
                    {
                        if (int.TryParse(message[0].ToString(), out state))
                        {
                            message = message.Substring(2);
                        }
                    }
                    TraceManager.Add(message, state);
                }
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ex);
            }
        }

        /// <summary>
        /// Handles the FlashCall event
        /// </summary>
        void FlashMovieFlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent e)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(new StringReader(e.request));
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.MoveToContent();
                if (reader.Name == "invoke" && reader.GetAttribute("name") == "trace")
                {
                    reader.Read();
                    if (reader.Name == "arguments")
                    {
                        reader.Read();
                        if (reader.Name == "string")
                        {
                            reader.Read();
                            int state = 1;
                            string message = reader.Value;
                            if (message.Length > 2 && message[1] == ':' && char.IsDigit(message[0]))
                            {
                                if (int.TryParse(message[0].ToString(), out state))
                                {
                                    message = message.Substring(2);
                                }
                            }
                            TraceManager.Add(message, state);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorManager.ShowError(ex);
            }
        }

        #endregion

    }

}
