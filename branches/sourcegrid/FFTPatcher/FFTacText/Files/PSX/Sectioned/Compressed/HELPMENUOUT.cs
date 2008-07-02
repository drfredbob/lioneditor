/*
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
using System.Collections.Generic;

namespace FFTPatcher.TextEditor.Files.PSX
{
    /// <summary>
    /// Represents the text in the HELPMENU.OUT file.
    /// </summary>
    public class HELPMENUOUT : BasePSXCompressedFile
    {

		#region Static Fields (1) 

        private static Dictionary<int, long> locations;

		#endregion Static Fields 

		#region Fields (2) 

        private const string filename = "HELPMENU.OUT";
        private const int numberOfSections = 21;

		#endregion Fields 

		#region Properties (4) 


        /// <summary>
        /// Gets the number of sections.
        /// </summary>
        /// <value>The number of sections.</value>
        protected override int NumberOfSections { get { return numberOfSections; } }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value></value>
        public override string Filename { get { return filename; } }

        /// <summary>
        /// Gets the filenames and locations for this file.
        /// </summary>
        /// <value></value>
        public override IDictionary<int, long> Locations
        {
            get
            {
                if( locations == null )
                {
                    locations = new Dictionary<int, long>();
                    locations.Add( (int)PsxIso.Sectors.EVENT_HELPMENU_OUT, 0x1B30 );
                    locations.Add( (int)PsxIso.Sectors.WORLD_WORLD_BIN, 0x777E0 );
                }

                return locations;
            }
        }

        /// <summary>
        /// Gets the maximum length of this file as a byte array.
        /// </summary>
        /// <value></value>
        public override int MaxLength { get { return 0x169C0; } }


		#endregion Properties 

		#region Constructors (2) 

        private HELPMENUOUT()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HELPMENUOUT"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public HELPMENUOUT( IList<byte> bytes )
            : base( bytes )
        {
        }

		#endregion Constructors 

		#region Methods (2) 


        /// <summary>
        /// Gets a list of bytes that represent this file in its on-disc form.
        /// </summary>
        /// <returns></returns>
        protected override IList<byte> ToFinalBytes()
        {
            return Compress();
        }

        /// <summary>
        /// Gets a list of indices for named sections.
        /// </summary>
        public override IList<NamedSection> GetNamedSections()
        {
            var result = base.GetNamedSections();
            result.Add( new NamedSection( this, SectionType.JobDescriptions, 12 ) );
            result.Add( new NamedSection( this, SectionType.ItemDescriptions, 13 ) );
            result.Add( new NamedSection( this, SectionType.AbilityDescriptions, 15 ) );
            result.Add( new NamedSection( this, SectionType.SkillsetDescriptions, 19 ) );
            return result;
        }


		#endregion Methods 

    }
}