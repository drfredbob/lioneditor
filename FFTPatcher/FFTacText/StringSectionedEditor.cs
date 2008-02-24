﻿/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    FFTPatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FFTPatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FFTPatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FFTPatcher.TextEditor.Files;

namespace FFTPatcher.TextEditor
{
    public partial class StringSectionedEditor : UserControl
    {
        private IStringSectioned strings;
        private bool ignoreChanges = false;
        private bool error = false;

        public IStringSectioned Strings
        {
            get { return strings; }
            set
            {
                if( strings != value )
                {
                    strings = value;
                    AddSections();
                    sectionComboBox.SelectedIndex = 0;
                    UpdateCurrentStringListBox();
                    currentStringListBox.SelectedIndex = 0;
                    UpdateCurrentString();
                    UpdateLengthLabels();
                }
            }
        }

        private void AddSections()
        {
            sectionComboBox.Items.Clear();
            for( int i = 0; i < strings.Sections.Count; i++ )
            {
                sectionComboBox.Items.Add( string.Format( "{0} {1}", i + 1, strings.SectionNames[i] ) );
            }
        }

        private void UpdateCurrentStringListBox()
        {
            currentStringListBox.Items.Clear();
            for( int i = 0; i < strings.Sections[Math.Max( 0, sectionComboBox.SelectedIndex )].Count; i++ )
            {
                currentStringListBox.Items.Add( string.Format( "{0} {1}", i + 1, strings.EntryNames[sectionComboBox.SelectedIndex][i] ) );
            }
            currentStringListBox.SelectedIndex = 0;
            UpdateCurrentString();
        }

        private void UpdateCurrentString()
        {
            ignoreChanges = true;
            currentString.Text = strings.Sections[Math.Max( 0, sectionComboBox.SelectedIndex )][Math.Max( 0, currentStringListBox.SelectedIndex )];
            ignoreChanges = false;
        }

        public StringSectionedEditor()
        {
            InitializeComponent();
            sectionComboBox.SelectedIndexChanged += sectionComboBox_SelectedIndexChanged;
            currentStringListBox.SelectedIndexChanged += new EventHandler( currentStringListBox_SelectedIndexChanged );
            currentString.TextChanged += new EventHandler( currentString_TextChanged );
            currentString.Validating += new CancelEventHandler( currentString_Validating );
            currentString.Font = new Font( "Arial Unicode MS", 10 );
        }

        private void currentString_Validating( object sender, CancelEventArgs e )
        {
            e.Cancel = error;
        }

        private void UpdateLengthLabels()
        {
            try
            {
                lengthLabel.Text = string.Format( "Length: {0} bytes", strings.Length );
                maxLengthLabel.Text = string.Format( "Max: {0} bytes", strings.MaxLength );
                error = false;
                errorLabel.Visible = false;
            }
            catch( Exception )
            {
                error = true;
                errorLabel.Visible = true;
            }
        }

        private void currentString_TextChanged( object sender, EventArgs e )
        {
            if( !ignoreChanges && (sectionComboBox.SelectedIndex > -1) && (currentStringListBox.SelectedIndex > -1) )
            {
                strings.Sections[sectionComboBox.SelectedIndex][currentStringListBox.SelectedIndex] = currentString.Text;
                UpdateLengthLabels();
            }
        }

        private void currentStringListBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateCurrentString();
        }

        private void sectionComboBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateCurrentStringListBox();
        }
    }
}