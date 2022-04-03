﻿using System;
using System.IO;
using System.Windows.Forms;
using Unmanaged;

namespace MusicBeePlugin
{
    public partial class FrmLyrics : Form
    {
        private SettingsObj _settings;
        private string _lyrics1Path;
        private string _lyrics2Path;

        public FrmLyrics(SettingsObj settings, string lyrics1Path, string lyrics2Path)
        {
            _settings = settings;
            _lyrics1Path = lyrics1Path;
            _lyrics2Path = lyrics2Path;
            
            InitializeComponent();
        }
        
        private void FrmLyrics_Load(object sender, EventArgs e)
        {
            var scrBounds = Screen.PrimaryScreen.Bounds;
            Width = scrBounds.Width;
            Height = 150;
            if (_settings.PosY < 0)
            {
                Top = scrBounds.Height - Height - 150;
                Left = 0;
            }
            else
            {
                Top = _settings.PosY;
                Left = _settings.PosX;
            }

            TopMost = true;

            UnmanagedHelper.SetTopMost(this);
            UpdateFromSettings(_settings);

            Move += (o, args) =>
            {
                _settings.PosX = Left;
                _settings.PosY = Top;
            };
            FormClosing += (o, args) =>
            {
                if (args.CloseReason == CloseReason.UserClosing)
                    args.Cancel = true;
            };
        }
        
        public void UpdateFromSettings(SettingsObj settings)
        {
            _settings = settings;
            LyricsRenderer.UpdateFromSettings(settings, Width);

            Redraw();
        }

        private void Redraw()
        {
            if (_line1 == null)
            {
                Clear();
                return;
            }

            if (_line2 == null)
            {
                DrawLyrics1Line(_line1);
                return;
            }
            DrawLyrics2Line(_line1, _line2);
        }

        public void Clear()
        {
            if (_line1 != "")
                DrawLyrics1Line("");
            _line1 = "";
        }

        private string _line1, _line2;
        public void UpdateLyrics(string line1, string line2)
        {
            _line1 = line1;
            _line2 = line2;
            File.WriteAllText(_lyrics1Path, line1);
            File.WriteAllText(_lyrics2Path, line2);
            Redraw();
        }

        public void DrawLyrics1Line(string lyrics)
        {
            var bitmap = LyricsRenderer.Render1LineLyrics(lyrics);
            if (Width != bitmap.Width) Width = bitmap.Width;
            if (Height != bitmap.Height) Height = bitmap.Height;

            GdiplusHelper.SetBitmap(bitmap, 255, Handle, Left, Top, Width, Height);
            bitmap.Dispose();
        }

        public void DrawLyrics2Line(string line1, string line2)
        {
            var bitmap = LyricsRenderer.Render2LineLyrics(line1, line2);
            if (Width != bitmap.Width) Width = bitmap.Width;
            if (Height != bitmap.Height) Height = bitmap.Height;

            GdiplusHelper.SetBitmap(bitmap, 255, Handle, Left, Top, Width, Height);

            bitmap.Dispose();
        }

        private void FrmLyrics_MouseDown(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                Unmanaged.Unmanaged.ReleaseCapture();
                if (e.Button != MouseButtons.Left) return;
                Unmanaged.Unmanaged.SendMessage(Handle, 0x00A1, new IntPtr(0x0002), null);
            }
        }

        private void FrmLyrics_MouseUp(object sender, MouseEventArgs e)
        {
            // This event is never fired...?
            //if (e.Button != MouseButtons.Left) return;
            //global::Unmanaged.Unmanaged.SendMessage(Handle, 0x00A2, new IntPtr(0x0002), null);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                cp.ExStyle |= 0x00000080;
                return cp;
            }
        }
    }
}
