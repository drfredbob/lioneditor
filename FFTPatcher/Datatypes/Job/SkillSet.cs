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

using System.Collections.Generic;
using PatcherLib;
using PatcherLib.Datatypes;
using PatcherLib.Utilities;

namespace FFTPatcher.Datatypes
{
    /// <summary>
    /// Represents the set of <see cref="Ability"/> a <see cref="Job"/> can use.
    /// </summary>
    public class SkillSet : IChangeable, ISupportDigest
    {
		#region Instance Variables (3) 

        private static readonly string[] digestableProperties = new string[22] {
            "Action1", "Action2", "Action3", "Action4", "Action5", "Action6", "Action7", "Action8", 
            "Action9", "Action10", "Action11", "Action12", "Action13", "Action14", "Action15", "Action16", 
            "TheRest1", "TheRest2", "TheRest3", "TheRest4", "TheRest5", "TheRest6" };
        private static SortedDictionary<byte, SkillSet> pspEventSkills;
        private static SortedDictionary<byte, SkillSet> psxEventSkills;

		#endregion Instance Variables 

		#region Public Properties (36) 

        public Ability Action1 { get { return Actions[0]; } }

        public Ability Action10 { get { return Actions[9]; } }

        public Ability Action11 { get { return Actions[10]; } }

        public Ability Action12 { get { return Actions[11]; } }

        public Ability Action13 { get { return Actions[12]; } }

        public Ability Action14 { get { return Actions[13]; } }

        public Ability Action15 { get { return Actions[14]; } }

        public Ability Action16 { get { return Actions[15]; } }

        public Ability Action2 { get { return Actions[1]; } }

        public Ability Action3 { get { return Actions[2]; } }

        public Ability Action4 { get { return Actions[3]; } }

        public Ability Action5 { get { return Actions[4]; } }

        public Ability Action6 { get { return Actions[5]; } }

        public Ability Action7 { get { return Actions[6]; } }

        public Ability Action8 { get { return Actions[7]; } }

        public Ability Action9 { get { return Actions[8]; } }

        public Ability[] Actions { get; private set; }

        public string CorrespondingJobs
        {
            get
            {
                List<string> result = new List<string>();
                foreach( Job j in FFTPatch.Jobs.Jobs )
                {
                    if( j.SkillSet.Value == Value )
                    {
                        result.Add( j.ToString() );
                    }
                }

                return string.Join( ", ", result.ToArray() );
            }
        }

        public SkillSet Default { get; private set; }

        public IList<string> DigestableProperties
        {
            get { return digestableProperties; }
        }

        public static SkillSet[] DummySkillSets
        {
            get
            {
                return FFTPatch.Context == Context.US_PSP ? PSPSkills : PSXSkills;
            }
        }

        public static SortedDictionary<byte, SkillSet> EventSkillSets
        {
            get { return FFTPatch.Context == Context.US_PSP ? pspEventSkills : psxEventSkills; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has changed.
        /// </summary>
        /// <value></value>
        public bool HasChanged
        {
            get
            {
                if( Default != null )
                {
                    for( int i = 0; i < Actions.Length; i++ )
                    {
                        if( Actions[i].Offset != Default.Actions[i].Offset )
                            return true;
                    }
                    for( int i = 0; i < TheRest.Length; i++ )
                    {
                        if( TheRest[i].Offset != Default.TheRest[i].Offset )
                            return true;
                    }
                }

                return false;
            }
        }

        public string Name { get; private set; }

        public static string[] PSPNames { get; private set; }

        public static SkillSet[] PSPSkills { get; private set; }

        public static string[] PSXNames { get; private set; }

        public static SkillSet[] PSXSkills { get; private set; }

        public Ability[] TheRest { get; private set; }

        public Ability TheRest1 { get { return TheRest[0]; } }

        public Ability TheRest2 { get { return TheRest[1]; } }

        public Ability TheRest3 { get { return TheRest[2]; } }

        public Ability TheRest4 { get { return TheRest[3]; } }

        public Ability TheRest5 { get { return TheRest[4]; } }

        public Ability TheRest6 { get { return TheRest[5]; } }

        public byte Value { get; private set; }

		#endregion Public Properties 

		#region Constructors (4) 

        static SkillSet()
        {
            PSPSkills = new SkillSet[0xE3];
            pspEventSkills = new SortedDictionary<byte, SkillSet>();

            PSPNames = PatcherLib.Utilities.Utilities.GetStringsFromNumberedXmlNodes(
                PSPResources.SkillSets,
                "/SkillSets/SkillSet[@byte='{0:X2}']/@name",
                0xE3 );
            PSXNames = PatcherLib.Utilities.Utilities.GetStringsFromNumberedXmlNodes(
                PSXResources.SkillSets,
                "/SkillSets/SkillSet[@byte='{0:X2}']/@name",
                0xE0 );
            for( int i = 0; i < 0xE3; i++ )
            {
                string n = PSPNames[i];
                PSPSkills[i] = new SkillSet( n, (byte)(i & 0xFF) );
                pspEventSkills.Add( (byte)i, PSPSkills[i] );
            }

            SkillSet random = new SkillSet( "<Random>", 0xFE );
            SkillSet equal = new SkillSet( "<Job's>", 0xFF );
            pspEventSkills.Add( 0xFE, random );
            pspEventSkills.Add( 0xFF, equal );

            PSXSkills = new SkillSet[0xE0];
            psxEventSkills = new SortedDictionary<byte, SkillSet>();
            for( int i = 0; i < 0xE0; i++ )
            {
                string n = PSXNames[i];
                PSXSkills[i] = new SkillSet( n, (byte)(i & 0xFF) );
                psxEventSkills.Add( (byte)i, PSXSkills[i] );
            }
            psxEventSkills.Add( 0xFE, random );
            psxEventSkills.Add( 0xFF, equal );
        }

        public SkillSet( byte value, IList<byte> bytes )
            : this( DummySkillSets[value].Name, value )
        {
            List<bool> actions = new List<bool>( 16 );
            actions.AddRange( PatcherLib.Utilities.Utilities.BooleansFromByteMSB( bytes[0] ) );
            actions.AddRange( PatcherLib.Utilities.Utilities.BooleansFromByteMSB( bytes[1] ) );
            List<bool> theRest = new List<bool>( PatcherLib.Utilities.Utilities.BooleansFromByteMSB( bytes[2] ) );

            Actions = new Ability[16];
            TheRest = new Ability[6];

            for( int i = 0; i < 16; i++ )
            {
                Actions[i] = AllAbilities.DummyAbilities[(actions[i] ? (bytes[3 + i] + 0x100) : (bytes[3 + i]))];
            }
            for( int i = 0; i < 6; i++ )
            {
                TheRest[i] = AllAbilities.DummyAbilities[(theRest[i] ? (bytes[19 + i] + 0x100) : (bytes[19 + i]))];
            }
        }

        private SkillSet( string name, byte value )
        {
            Name = name;
            Value = value;
        }

        public SkillSet( byte value, IList<byte> bytes, SkillSet defaults )
            : this( value, bytes )
        {
            Default = defaults;
        }

		#endregion Constructors 

		#region Public Methods (4) 

        public static void Copy( SkillSet source, SkillSet destination )
        {
            for( int i = 0; i < 16; i++ )
            {
                destination.Actions[i] = source.Actions[i];
            }
            for( int i = 0; i < 6; i++ )
            {
                destination.TheRest[i] = source.TheRest[i];
            }
        }

        public void CopyTo( SkillSet destination )
        {
            Copy( this, destination );
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[25];
            result[0] = PatcherLib.Utilities.Utilities.ByteFromBooleans(
                Actions[0].Offset > 0xFF,
                Actions[1].Offset > 0xFF,
                Actions[2].Offset > 0xFF,
                Actions[3].Offset > 0xFF,
                Actions[4].Offset > 0xFF,
                Actions[5].Offset > 0xFF,
                Actions[6].Offset > 0xFF,
                Actions[7].Offset > 0xFF );
            result[1] = PatcherLib.Utilities.Utilities.ByteFromBooleans(
                Actions[8].Offset > 0xFF,
                Actions[9].Offset > 0xFF,
                Actions[10].Offset > 0xFF,
                Actions[11].Offset > 0xFF,
                Actions[12].Offset > 0xFF,
                Actions[13].Offset > 0xFF,
                Actions[14].Offset > 0xFF,
                Actions[15].Offset > 0xFF );
            result[2] = PatcherLib.Utilities.Utilities.ByteFromBooleans(
                TheRest[0].Offset > 0xFF,
                TheRest[1].Offset > 0xFF,
                TheRest[2].Offset > 0xFF,
                TheRest[3].Offset > 0xFF,
                TheRest[4].Offset > 0xFF,
                TheRest[5].Offset > 0xFF,
                false,
                false );
            for( int i = 0; i < 16; i++ )
            {
                result[3 + i] = (byte)(Actions[i].Offset & 0xFF);
            }
            for( int i = 0; i < 6; i++ )
            {
                result[19 + i] = (byte)(TheRest[i].Offset & 0xFF);
            }

            return result;
        }

        public override string ToString()
        {
            return (HasChanged ? "*" : "") + Value.ToString( "X2" ) + " " + Name;
        }

		#endregion Public Methods 
    }

    public class AllSkillSets : PatchableFile, IXmlDigest
    {
		#region Public Properties (2) 

        /// <summary>
        /// Gets a value indicating whether this instance has changed.
        /// </summary>
        /// <value></value>
        public override bool HasChanged
        {
            get
            {
                foreach( SkillSet s in SkillSets )
                {
                    if( s.HasChanged )
                        return true;
                }

                return false;
            }
        }

        public SkillSet[] SkillSets { get; private set; }

		#endregion Public Properties 

		#region Constructors (3) 

        public AllSkillSets( IList<byte> bytes )
            : this( Context.US_PSP, bytes, PSPResources.SkillSetsBin )
        {
        }

        public AllSkillSets( Context context, IList<byte> bytes )
            : this( context, bytes, null )
        {
        }

        public AllSkillSets( Context context, IList<byte> bytes, IList<byte> defaultBytes )
        {
            List<SkillSet> tempSkills = new List<SkillSet>( 179 );
            for( int i = 0; i < 176; i++ )
            {
                tempSkills.Add( new SkillSet( (byte)i, bytes.Sub( 25 * i, 25 * i + 24 ),
                    new SkillSet( (byte)i, defaultBytes.Sub( 25 * i, 25 * i + 24 ) ) ) );
            }

            if( context == Context.US_PSP )
            {
                for( int i = 0xE0; i <= 0xE2; i++ )
                {
                    tempSkills.Add( new SkillSet( (byte)i, bytes.Sub( 25 * (i - 0xE0 + 176), 25 * (i - 0xE0 + 176) + 24 ),
                        new SkillSet( (byte)i, defaultBytes.Sub( 25 * (i - 0xE0 + 176), 25 * (i - 0xE0 + 176) + 24 ) ) ) );
                }
            }

            SkillSets = tempSkills.ToArray();
        }

		#endregion Constructors 

		#region Public Methods (5) 

        public List<string> GenerateCodes()
        {
            if( FFTPatch.Context == Context.US_PSP )
            {
                return Codes.GenerateCodes( Context.US_PSP, PSPResources.SkillSetsBin, this.ToByteArray(), 0x2799E4 );
            }
            else
            {
                return Codes.GenerateCodes( Context.US_PSX, PSXResources.SkillSetsBin, this.ToByteArray( Context.US_PSX ), 0x064A94 );
            }
        }

        public override IList<PatchedByteArray> GetPatches( Context context )
        {
            var result = new List<PatchedByteArray>( 2 );

            var bytes = ToByteArray( context );
            if ( context == Context.US_PSX )
            {
                result.Add(PatcherLib.Iso.PsxIso.SkillSets.GetPatchedByteArray(bytes));
            }
            else if ( context == Context.US_PSP )
            {
                PatcherLib.Iso.PspIso.SkillSets.ForEach(kl => result.Add(kl.GetPatchedByteArray(bytes)));
            }

            return result;
        }

        public byte[] ToByteArray()
        {
            return ToByteArray( Context.US_PSP );
        }

        public byte[] ToByteArray( Context context )
        {
            List<byte> result = new List<byte>( 0x117B );
            foreach( SkillSet s in SkillSets )
            {
                result.AddRange( s.ToByteArray() );
            }

            while( (context == Context.US_PSP) && (result.Count < 0x117B) )
            {
                result.Add( 0x00 );
            }

            return result.ToArray();
        }

        public void WriteXml( System.Xml.XmlWriter writer )
        {
            if( HasChanged )
            {
                writer.WriteStartElement( this.GetType().Name );
                writer.WriteAttributeString( "changed", HasChanged.ToString() );
                foreach( SkillSet s in SkillSets )
                {
                    if( s.HasChanged )
                    {
                        writer.WriteStartElement( s.GetType().Name );
                        writer.WriteAttributeString( "value", s.Value.ToString( "X2" ) );
                        writer.WriteAttributeString( "name", s.Name );
                        DigestGenerator.WriteXmlDigest( s, writer, false, false );
                        writer.WriteElementString( "CorrespondingJobs", s.CorrespondingJobs );
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
        }

		#endregion Public Methods 
    }
}
