using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using MetroSuite;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;

public partial class MainForm : MetroForm
{
    private bool _recording, _paused;
    private WaveIn _waveIn;
    private WaveFileWriter _waveFileWriter;
    private WasapiLoopbackCapture _wasapiLoopbackCapture;

    public MainForm()
    {
        InitializeComponent();

        for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
        {
            guna2ComboBox1.Items.Add(WaveIn.GetCapabilities(waveInDevice).ProductName);
        }

        for (int waveOutDevice = 0; waveOutDevice < WaveOut.DeviceCount; waveOutDevice++)
        {
            guna2ComboBox2.Items.Add(WaveOut.GetCapabilities(waveOutDevice).ProductName);
        }

        guna2ComboBox1.SelectedIndex = 0;
        guna2ComboBox2.SelectedIndex = 0;
    }

    private void guna2RadioButton1_CheckedChanged(object sender, EventArgs e)
    {
        guna2ComboBox1.Enabled = guna2RadioButton1.Checked;
        guna2ComboBox2.Enabled = !guna2RadioButton1.Checked;
        guna2Button4.DisabledState.FillColor = guna2RadioButton1.Checked ? ColorTranslator.FromHtml("#005B99") : ColorTranslator.FromHtml("#007ACC");
        guna2Button5.DisabledState.FillColor = guna2RadioButton1.Checked ? ColorTranslator.FromHtml("#007ACC") : ColorTranslator.FromHtml("#005B99");
    }

    private void guna2Button2_Click(object sender, EventArgs e)
    {
        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            guna2TextBox1.Text = saveFileDialog1.FileName;
        }
    }

    private void guna2Button1_Click(object sender, EventArgs e)
    {
        if (!guna2Button1.Text.Equals("Stop recording") && !Path.GetExtension(guna2TextBox1.Text).ToLower().Equals(".wav"))
        {
            MessageBox.Show("Please, choose a valid output path for your audio recorded file in order to start a new recording.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!guna2Button1.Text.Equals("Stop recording") && File.Exists(guna2TextBox1.Text))
        {
            try
            {
                File.Delete(guna2TextBox1.Text);
            }
            catch
            {
                MessageBox.Show("Since the recorded audio file already exists in your computer, it can't be deleted, so the program can not start a new recording. Please, be sure to remove that file or change the saving path.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        _recording = !_recording;
        guna2Button3.Enabled = _recording;
        guna2Button1.Image = _recording ? RawAudioRecorder.Properties.Resources.stop : RawAudioRecorder.Properties.Resources.record;
        guna2Button1.Text = _recording ? "Stop recording" : "Start recording";
        guna2Button2.Enabled = !_recording;

        if (!_recording)
        {
            _paused = false;
            guna2Button3.Text = "Pause recording";
            guna2Button3.Image = RawAudioRecorder.Properties.Resources.pause;
        }

        if (_recording)
        {
            if (guna2RadioButton1.Checked)
            {
                _waveIn = new WaveIn();
                _waveIn.DeviceNumber = guna2ComboBox1.SelectedIndex;
                _waveIn.WaveFormat = new WaveFormat(384000, 32, 2);
                _waveFileWriter = new WaveFileWriter(guna2TextBox1.Text, _waveIn.WaveFormat);

                _waveIn.DataAvailable += (s, e1) =>
                {
                    if (!_paused)
                    {
                        _waveFileWriter.Write(e1.Buffer, 0, e1.BytesRecorded);
                        _waveFileWriter.Flush();
                    }
                };

                _waveIn.StartRecording();
            }
            else
            {
                MMDeviceCollection devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach (MMDevice mmDevice in devices)
                {
                    if (mmDevice.FriendlyName.ToLower().StartsWith(guna2ComboBox2.SelectedItem.ToString().ToLower())
                        || guna2ComboBox2.SelectedItem.ToString().ToLower().StartsWith(mmDevice.FriendlyName.ToLower()))
                    {
                        _wasapiLoopbackCapture = new WasapiLoopbackCapture(mmDevice);
                        break;
                    }
                }

                _wasapiLoopbackCapture.WaveFormat = new WaveFormat(384000, 32, 2);
                _waveFileWriter = new WaveFileWriter(guna2TextBox1.Text, _wasapiLoopbackCapture.WaveFormat);

                _wasapiLoopbackCapture.DataAvailable += (s, e1) =>
                {
                    if (!_paused)
                    {
                        _waveFileWriter.Write(e1.Buffer, 0, e1.BytesRecorded);
                        _waveFileWriter.Flush();
                    }
                };

                _wasapiLoopbackCapture.StartRecording();
            }
        }
        else
        {
            if (guna2RadioButton1.Checked)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
            }
            else
            {
                _wasapiLoopbackCapture.StopRecording();
                _wasapiLoopbackCapture.Dispose();
            }

            _waveFileWriter.Close();
            _waveFileWriter.Dispose();
        }
    }

    private void guna2Button6_Click(object sender, EventArgs e)
    {
        Process.Start("https://github.com/GabryB03/RawAudioRecorder/");
    }

    private void guna2Button3_Click(object sender, EventArgs e)
    {
        _paused = !_paused;
        guna2Button3.Text = _paused ? "Resume recording" : "Pause recording";
        guna2Button3.Image = _paused ? RawAudioRecorder.Properties.Resources.resume : RawAudioRecorder.Properties.Resources.pause;
    }
}