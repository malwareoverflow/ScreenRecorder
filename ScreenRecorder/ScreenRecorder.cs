using SharpAvi;
using SharpAvi.Codecs;
using SharpAvi.Output;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenRecorder
{
    public partial class ScreenRecorder : Form
    {
        public ScreenRecorder()
        {
            InitializeComponent();
        }
        public ScreenRecorder(string filename, int FrameRate, FourCC Encoder, int Quality)
        {
            FileName = filename;
            FramesPerSecond = FrameRate;
            Codec = Encoder;
            this.Quality = Quality;

            Height = Screen.PrimaryScreen.Bounds.Height;
            Width = Screen.PrimaryScreen.Bounds.Width;
        }

        string FileName;
        dynamic recorder;
        public string savepath;
        public string filename;
        public int FramesPerSecond, Quality;
        FourCC Codec;

        public int Height { get; private set; }
        public int Width { get; private set; }

        public AviWriter CreateAviWriter()
        {
            return new AviWriter(FileName)
            {
                FramesPerSecond = FramesPerSecond,
                EmitIndex1 = true,
            };
        }

       

    

    

        public IAviVideoStream CreateVideoStream(AviWriter writer)
        {
            // Select encoder type based on FOURCC of codec
            if (Codec == KnownFourCCs.Codecs.Uncompressed)
                return writer.AddUncompressedVideoStream(Width, Height);
            else if (Codec == KnownFourCCs.Codecs.MotionJpeg)
                return writer.AddMotionJpegVideoStream(Width, Height, Quality);
            else
            {
                return writer.AddMpeg4VideoStream(Width, Height, (double)writer.FramesPerSecond,
                    // It seems that all tested MPEG-4 VfW codecs ignore the quality affecting parameters passed through VfW API
                    // They only respect the settings from their own configuration dialogs, and Mpeg4VideoEncoder currently has no support for this
                    quality: Quality,
                    codec: Codec,
                    // Most of VfW codecs expect single-threaded use, so we wrap this encoder to special wrapper
                    // Thus all calls to the encoder (including its instantiation) will be invoked on a single thread although encoding (and writing) is performed asynchronously
                    forceSingleThreadedAccess: true);
            }
        }


        private void Start(object sender, EventArgs e)
        {
            if (int.Parse(qualityvalue.Text)<101)
            {
                if (savepath != null)
                {
                    recorder = new Recorder(new ScreenRecorder($"{savepath}\\{filename = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, Convert.ToInt32(qualityvalue.Text)));
                }
                else
                {
                    MessageBox.Show("Please select path");
                }

            }
            else
            {

                MessageBox.Show("Use less than 100");
            }



        }
      
        private void Browse(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
           
            savepath = folderBrowserDialog1.SelectedPath == null ? null : folderBrowserDialog1.SelectedPath;

          
            savepathurl.Text = folderBrowserDialog1.SelectedPath;
        }

        private void Quality_keypress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
          
            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void Stop(object sender, EventArgs e)
        {

     
            if (recorder.screenThread.IsAlive)
            {
              
                recorder.Dispose();
                MessageBox.Show($"Successfully saved in {savepath}");
                Process.Start($"{savepath}\\{filename}.avi");
            } 

        }
    }


}
