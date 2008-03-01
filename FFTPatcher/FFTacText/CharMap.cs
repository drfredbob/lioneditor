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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using FFTPatcher.Datatypes;
using System.Runtime.InteropServices;

namespace FFTPatcher.TextEditor
{
    public abstract class GenericCharMap : Dictionary<int, string>
    {
        private Dictionary<string, int> BuildValueToKeyMapping()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach( KeyValuePair<int, string> kvp in this )
            {
                if( !result.ContainsKey( kvp.Value ) )
                {
                    result.Add( kvp.Value, kvp.Key );
                }
                else if( kvp.Key < result[kvp.Value] )
                {
                    result[kvp.Value] = kvp.Key;
                }
            }

            return result;
        }

        private Dictionary<string, int> reverse = null;
        public Dictionary<string, int> Reverse
        {
            get
            {
                if( reverse == null )
                {
                    reverse = BuildValueToKeyMapping();
                }

                return reverse;
            }
        }

        public string GetNextChar( IList<byte> bytes, ref int pos )
        {
            int resultPos = pos + 1;
            byte val = bytes[pos];
            int key = val;
            if( (val >= 0xD0 && val <= 0xDA) || (val == 0xE2) || (val == 0xE3) )
            {
                byte nextVal = bytes[pos + 1];
                resultPos++;
                key = val * 256 + nextVal;
            }
            else if( val >= 0xF0 && val <= 0xF3 && (pos + 2) < bytes.Count )
            {
                resultPos += 2;
                key = val * 256 * 256 + bytes[pos + 1] * 256 + bytes[pos + 2];
            }

            string result = string.Format( "{{0x{0:X2}", key ) + @"}";
            if( this.ContainsKey( key ) )
            {
                result = this[key];
            }

            pos = resultPos;

            return result;
        }

        private static Regex regex = new Regex( @"{0x([0-9A-Fa-f]+)}" );

        public byte[] StringToByteArray( string s )
        {
            List<byte> result = new List<byte>( s.Length );

            for( int i = 0; i < s.Length; i++ )
            {
                int val = 0;
                if( s[i] == '{' )
                {
                    int j = s.IndexOf( '}', i );
                    string key = s.Substring( i, j - i + 1 );
                    if( Reverse.ContainsKey( key ) )
                    {
                        val = Reverse[key];
                    }
                    else
                    {
                        Match match = regex.Match( key );
                        if( match.Success )
                        {
                            result.AddRange( IntToOneOrTwoOrThreeBytes( Convert.ToInt32( match.Groups[1].Value, 16 ) ) );
                            val = -1;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    i = j;
                }
                else if( s[i] == '\r' && (i + 1) < s.Length && s[i + 1] == '\n' )
                {
                    i += 1;
                    val = Reverse["\r\n"];
                }
                else
                {
                    val = Reverse[s[i].ToString()];
                }
                if( val >= 0 )
                {
                    result.AddRange( IntToOneOrTwoOrThreeBytes( val ) );
                }
            }

            return result.ToArray();
        }

        private byte[] IntToOneOrTwoOrThreeBytes( int i )
        {
            if( i < 256 )
            {
                return new byte[] { (byte)i };
            }
            else if( i < 65536 )
            {
                return new byte[] { (byte)((i & 0xFF00) >> 8), (byte)(i & 0xFF) };
            }
            else
            {
                return new byte[] { (byte)((i & 0xFF000) >> 16), (byte)((i & 0xFF00) >> 8), (byte)(i & 0xFF) };
            }
        }
    }

    public class PSPCharMap : GenericCharMap
    {
    }

    public class PSXCharMap : GenericCharMap
    {
    }

    public static class TextUtilities
    {
        public enum CharMapType
        {
            PSX,
            PSP
        }

        public static PSXCharMap PSXMap { get; private set; }
        public static PSPCharMap PSPMap { get; private set; }

        private static IDictionary<int, int> BuildDict()
        {
            Dictionary<int, int> result = new Dictionary<int, int>( 3792 );
            for( int i = 0; i < 3900; i++ )
            {
                int x = i - (i / 256) * 2;
                if( !result.ContainsKey( x ) )
                {
                    result.Add( x, i );
                }
                else
                {
                    result[x] = i;
                }
            }

            return result;
        }

        private static IDictionary<int, int> CompressionJumps { get; set; }

        static TextUtilities()
        {
            CompressionJumps = BuildDict();

            PSXMap = new PSXCharMap();

            for( int i = (int)'a'; i <= (int)'z'; i++ )
            {
                PSXMap.Add( i - (ushort)'a' + 0x24, ((char)i).ToString() );
                PSXMap.Add( i - (ushort)'a' + 0x24 + 0xD000, ((char)i).ToString() );
            }
            PSXMap.Add( 0x40, "?" );
            PSXMap.Add( 0xD040, "?" );
            PSXMap.Add( 0xD9C9, "?" );
            PSXMap.Add( 0xB2, "\u266A" );
            PSXMap.Add( 0xD0B2, "\u266A" );
            PSXMap.Add( 0xD0E7, "\u2014" );
            PSXMap.Add( 0xD117, "\u2014" );
            PSXMap.Add( 0xD0E8, "\u300C" );
            PSXMap.Add( 0xD118, "\u300C" );
            PSXMap.Add( 0xD0EB, "\u22EF" );
            PSXMap.Add( 0xD11B, "\u22EF" );
            PSXMap.Add( 0xD0EF, "\xD7" );
            PSXMap.Add( 0xD11F, "\xD7" );
            PSXMap.Add( 0xD120, "\xF7" );
            PSXMap.Add( 0xD121, "\u2229" );
            PSXMap.Add( 0xD122, "\u222A" );
            PSXMap.Add( 0xD123, "=" );
            PSXMap.Add( 0xDA70, "=" );
            PSXMap.Add( 0xD0F4, "\u2260" );
            PSXMap.Add( 0xD124, "\u2260" );
            PSXMap.Add( 0xD9B5, "\u221E" );
            PSXMap.Add( 0xD9B7, "&" );
            PSXMap.Add( 0xD9B8, "%" );
            PSXMap.Add( 0xD9B9, "\u25CB" );
            PSXMap.Add( 0xD9BA, "\u2190" );
            PSXMap.Add( 0xD9BB, "\u2192" );
            PSXMap.Add( 0xD9C2, "\u300E" );
            PSXMap.Add( 0xD9C3, "\u300F" );
            PSXMap.Add( 0xD9C4, "\u300D" );
            PSXMap.Add( 0xD9C5, "\uFF5E" );
            PSXMap.Add( 0xD9C7, "\u25B3" );
            PSXMap.Add( 0xD9C8, "\u25A1" );
            PSXMap.Add( 0xD9CA, "\u2665" );
            PSXMap.Add( 0xD9CB, "\u2160" );
            PSXMap.Add( 0xD9CC, "\u2161" );
            PSXMap.Add( 0xD9CD, "\u2162" );
            PSXMap.Add( 0xD9CE, "\u2163" );
            PSXMap.Add( 0xD9CF, "\u2164" );
            PSXMap.Add( 0xDA00, "\u2648" );
            PSXMap.Add( 0xDA01, "\u2649" );
            PSXMap.Add( 0xDA02, "\u264A" );
            PSXMap.Add( 0xDA03, "\u264B" );
            PSXMap.Add( 0xDA04, "\u264C" );
            PSXMap.Add( 0xDA05, "\u264D" );
            PSXMap.Add( 0xDA06, "\u264E" );
            PSXMap.Add( 0xDA07, "\u264F" );
            PSXMap.Add( 0xDA08, "\u2650" );
            PSXMap.Add( 0xDA09, "\u2651" );
            PSXMap.Add( 0xDA0A, "\u2652" );
            PSXMap.Add( 0xDA0B, "\u2653" );
            PSXMap.Add( 0xDA0C, "{Serpentarius}" );
            PSXMap.Add( 0xDA71, "$" );
            PSXMap.Add( 0xDA72, "\xA5" );
            PSXMap.Add( 0xDA74, "," );
            PSXMap.Add( 0xDA75, ";" );

            PSXMap.Add( 0xD0ED, "-" );
            PSXMap.Add( 0xD11D, "-" );

            PSXMap.Add( 0x42, "+" );
            PSXMap.Add( 0xD042, "+" );
            PSXMap.Add( 0xD0EE, "+" );
            PSXMap.Add( 0xD11E, "+" );

            PSXMap.Add( 0x46, ":" );
            PSXMap.Add( 0xD046, ":" );
            PSXMap.Add( 0xD9BD, ":" );

            PSXMap.Add( 0x8D, "(" );
            PSXMap.Add( 0xD08D, "(" );
            PSXMap.Add( 0xD9BE, "(" );

            PSXMap.Add( 0x8E, ")" );
            PSXMap.Add( 0xD08E, ")" );
            PSXMap.Add( 0xD9BF, ")" );

            PSXMap.Add( 0x91, "\"" );
            PSXMap.Add( 0xD091, "\"" );
            PSXMap.Add( 0xD9C0, "\"" );
            PSXMap.Add( 0xDA77, "\"" );

            PSXMap.Add( 0x93, "'" );
            PSXMap.Add( 0xD093, "'" );
            PSXMap.Add( 0xD9C1, "'" );
            PSXMap.Add( 0xDA76, "'" );

            PSXMap.Add( 0x8B, "\xB7" );
            PSXMap.Add( 0xD08B, "\xB7" );
            PSXMap.Add( 0xD9BC, "\xB7" );

            PSXMap.Add( 0x44, "/" );
            PSXMap.Add( 0xD044, "/" );
            PSXMap.Add( 0xD9C6, "/" );

            PSXMap.Add( 0xD0F5, ">" );
            PSXMap.Add( 0xD125, ">" );

            PSXMap.Add( 0xD0F6, "<" );
            PSXMap.Add( 0xD126, "<" );

            PSXMap.Add( 0xD0F7, "\u2267" );
            PSXMap.Add( 0xD127, "\u2267" );

            PSXMap.Add( 0xD128, "\u2266" );

            PSXMap.Add( 0xFA, " " );
            PSXMap.Add( 0xD0FA, " " );
            PSXMap.Add( 0xD12A, " " );
            PSXMap.Add( 0xDA73, " " );

            PSXMap.Add( 0x5F, "." );
            PSXMap.Add( 0xD05F, "." );
            PSXMap.Add( 0xD0E9, "." );
            PSXMap.Add( 0xD119, "." );
            PSXMap.Add( 0xD0EC, "." );
            PSXMap.Add( 0xD11C, "." );
            PSXMap.Add( 0xD9B6, "." );

            PSXMap.Add( 0x3E, "!" );
            PSXMap.Add( 0xD03E, "!" );
            PSXMap.Add( 0xD0EA, "!" );
            PSXMap.Add( 0xD11A, "!" );

            PSXMap.Add( 0xB5, "*" );
            PSXMap.Add( 0xD0B5, "*" );
            PSXMap.Add( 0xD0E1, "*" );
            PSXMap.Add( 0xD111, "*" );
            PSXMap.Add( 0xD0F9, "*" );
            PSXMap.Add( 0xD129, "*" );
            PSXMap.Add( 0xD0FB, "*" );
            PSXMap.Add( 0xD12B, "*" );
            PSXMap.Add( 0xD0FC, "*" );
            PSXMap.Add( 0xD12C, "*" );
            PSXMap.Add( 0xD0FD, "*" );
            PSXMap.Add( 0xD12D, "*" );
            PSXMap.Add( 0xD0FE, "*" );
            PSXMap.Add( 0xD12E, "*" );
            PSXMap.Add( 0xD0FF, "*" );
            PSXMap.Add( 0xD12F, "*" );
            PSXMap.Add( 0xD130, "*" );
            PSXMap.Add( 0xD131, "*" );
            PSXMap.Add( 0xD132, "*" );
            PSXMap.Add( 0xE0, "{Ramza}" );
            PSXMap.Add( 0xF8, "\r\n" );
            PSXMap.Add( 0xFB, "{Begin List}" );
            PSXMap.Add( 0xFC, "{End List}" );
            PSXMap.Add( 0xFE, "{END}" );
            PSXMap.Add( 0xFF, "{Close}" );

            for( int i = 0; i < 10; i++ )
            {
                PSXMap.Add( i, i.ToString() );
                PSXMap.Add( i + 0xD000, i.ToString() );
            }
            for( int i = (int)'A'; i <= (int)'Z'; i++ )
            {
                PSXMap.Add( i - (ushort)'A' + 0x0A, ((char)i).ToString() );
                PSXMap.Add( i - (ushort)'A' + 0x0A + 0xD000, ((char)i).ToString() );
            }

            for( int i = 0; i < 256; i++ )
            {
                // HACK
                PSXMap.Add( 0xE200 + i, string.Format( "{{Delay {0:X2}", i ) + @"}" );
                PSXMap.Add( 0xE300 + i, string.Format( "{{Color {0:X2}", i ) + @"}" );
            }

            PSXMap.Add( 0x3F, "\u3042" );
            PSXMap.Add( 0x41, "\u3044" );
            PSXMap.Add( 0x43, "\u3046" );
            PSXMap.Add( 0x45, "\u3048" );
            PSXMap.Add( 0xD03F, "\u3042" );
            PSXMap.Add( 0xD041, "\u3044" );
            PSXMap.Add( 0xD043, "\u3046" );
            PSXMap.Add( 0xD045, "\u3048" );

            for( int i = 0x47; i <= 0x5E; i++ )
            {
                PSXMap.Add( i, ((char)(i - 0x47 + 0x304A)).ToString() );
                PSXMap.Add( i + 0xD000, ((char)(i - 0x47 + 0x304A)).ToString() );
            }
            for( int i = 0x60; i <= 0x8A; i++ )
            {
                PSXMap.Add( i, ((char)(i - 0x60 + 0x3063)).ToString() );
                PSXMap.Add( i + 0xD000, ((char)(i - 0x60 + 0x3063)).ToString() );
            }

            PSXMap.Add( 0x8C, "\u308F" );
            PSXMap.Add( 0xD08C, "\u308F" );
            PSXMap.Add( 0x8F, "\u3092" );
            PSXMap.Add( 0xD08F, "\u3092" );
            PSXMap.Add( 0x90, "\u3093" );
            PSXMap.Add( 0xD090, "\u3093" );
            PSXMap.Add( 0x92, "\u30A2" );
            PSXMap.Add( 0xD092, "\u30A2" );
            
            for( int i = 0x94; i <= 0xB1; i++ )
            {
                PSXMap.Add( i, ((char)(i - 0x94 + 0x30A4)).ToString() );
                PSXMap.Add( i + 0xD000, ((char)(i - 0x94 + 0x30A4)).ToString() );
            }

            PSXMap.Add( 0xB3, "\u30C3" );
            PSXMap.Add( 0xD0B3, "\u30C3" );
            PSXMap.Add( 0xB4, "\u30C4" );
            PSXMap.Add( 0xD0B4, "\u30C4" );

            for( int i = 0xB6; i <= 0xCF; i++ )
            {
                PSXMap.Add( i, ((char)(i - 0xB6 + 0x30C6)).ToString() );
                PSXMap.Add( i + 0xD000, ((char)(i - 0xB6 + 0x30C6)).ToString() );
            }

            for( int i = 0xD0; i <= 0xDB; i++ )
            {
                PSXMap.Add( i + 0xD000, ((char)(i - 0xD0 + 0x30E0)).ToString() );
                PSXMap.Add( i - 0xD0 + 0xD100, ((char)(i - 0xD0 + 0x30E0)).ToString() );
            }

            PSXMap.Add( 0xD10C, "\u30EC" );
            PSXMap.Add( 0xD10D, "\u30ED" );
            PSXMap.Add( 0xD0DE, "\u30EE" );
            PSXMap.Add( 0xD10E, "\u30EE" );
            PSXMap.Add( 0xD0DF, "\u30EF" );
            PSXMap.Add( 0xD10F, "\u30EF" );

            for( int i = 0xE2; i <= 0xE6; i++ )
            {
                PSXMap.Add( i + 0xD000, ((char)(i - 0xE2 + 0x30F2)).ToString() );
                PSXMap.Add( i - 0xE2 + 0xD112, ((char)(i - 0xE2 + 0x30F2)).ToString() );
            }

            PSPMap = new PSPCharMap();
            foreach( KeyValuePair<int, string> kvp in PSXMap )
            {
                PSPMap.Add( kvp.Key, kvp.Value );
            }
            PSPMap[0x95] = " ";
            PSPMap.Add( 0xDA60, "\xE1" );
            PSPMap.Add( 0xDA61, "\xE0" );
            PSPMap.Add( 0xDA62, "\xE9" );
            PSPMap.Add( 0xDA63, "\xE8" );
            PSPMap.Add( 0xDA64, "\xED" );
            PSPMap.Add( 0xDA65, "\xFA" );
            PSPMap.Add( 0xDA66, "\xF9" );
        }

        public static List<string> ProcessList( IList<byte> bytes, CharMapType type )
        {
            GenericCharMap charmap = type == CharMapType.PSP ? PSPMap as GenericCharMap : PSXMap as GenericCharMap;

            List<IList<byte>> words = bytes.Split( (byte)0xFE );

            List<string> result = new List<string>( words.Count );

            foreach( IList<byte> word in words )
            {
                StringBuilder sb = new StringBuilder();
                int pos = 0;
                while( pos < word.Count )
                {
                    sb.Append( charmap.GetNextChar( word, ref pos ) );
                }

                result.Add( sb.ToString() );
            }

            return result;
        }

        private static void ProcessPointer( IList<byte> bytes, out int length, out int jump )
        {
            length = ((bytes[0] & 0x03) << 3) + ((bytes[1] & 0xE0) >> 5) + 4;
            int j = ((bytes[1] & 0x1F) << 8) + bytes[2];
            jump = j - (j / 256) * 2;
        }

        public static IList<byte> Decompress( IList<byte> allBytes, IList<byte> sectionBytes, int sectionStart )
        {
            IList<byte> result = new List<byte>();

            for( int i = 0; i < sectionBytes.Count; i++ )
            {
                if( sectionBytes[i] >= 0xF0 && sectionBytes[i] <= 0xF3 )
                {
                    int length;
                    int jump;
                    ProcessPointer( new byte[] { sectionBytes[i], sectionBytes[i + 1], sectionBytes[i + 2] }, out length, out jump );
                    //ProcessPointer( new byte[] { bytes[i], bytes[i + 1], bytes[i + 2] }, out length, out jump );
                    if( (i + sectionStart - jump) < 0 || (i + sectionStart - jump + length) >= allBytes.Count )
                    {
                        result.AddRange( new byte[] { sectionBytes[i], sectionBytes[i + 1], sectionBytes[i + 2] } );
                    }
                    else
                    {
                        result.AddRange( new SubArray<byte>( allBytes, i + sectionStart - jump, i + sectionStart - jump + length - 1 ) );
                    }
                    i += 2;
                }
                else
                {
                    result.Add( sectionBytes[i] );
                }
            }

            return result;
        }

        [DllImport( "MakeTempFilenameDLL2005.dll" )]
        static extern void Compress( byte[] bytes, int inputLength, byte[] output, ref int outputLength );

        public static IList<byte> Recompress( IList<byte> bytes )
        {
            byte[] output = new byte[bytes.Count];
            int outputLength = 0;
            Compress( 
                //new SubArray<byte>(bytes, 0, 0x8000-1).ToArray(), 
                bytes.ToArray(), 
                //0x8000, 
                bytes.Count, 
                output, 
                ref outputLength );

            byte[] result = new byte[outputLength];
            Array.Copy( output, result, outputLength );
            return result;
            //def compress(bytes, windowSize):
            //  d=buildDict()
            //  m=max(d.keys())
            //  result=""
            //  i = 0
            //  while i < len(bytes):
            //    #if i%1000 == 0: print i
            //    if bytes[i]== "\xfe":
            //      result += bytes[i]
            //      i+=1
            //      continue
            //    fe = bytes.find("\xfe",i,i+35)
            //    if fe == -1: fe=i+35
            //    size, loc = findBestSubStringInWindow(result[0-m:], bytes[i:fe])
            //    if size!=-1 and loc!=-1:# and not (bytes[i]=="\xfa" and bytes[i+size-1]=="\xfa"):
            //      a,b,c=buildPointer(d, loc, size)
            //      result += chr(a)+chr(b)+chr(c)
            //      i += size
            //    else:
            //      result += bytes[i]
            //      i+=1
            //  return result
            //List<byte> result = new List<byte>();
            //List<byte> bytesList = new List<byte>( bytes );
            
            //for (int i =0; i < bytes.Count; i++)
            //{
            //    if( bytes[i] == 0xFE )
            //    {
            //        result.Add( bytes[i] );
            //        continue;
            //    }
            //    int fe = bytesList.IndexOf( 0xFE, i, Math.Min( 35, bytes.Count - i ) );
            //    if( fe == -1 )
            //    {
            //        fe = i + 35;
            //    }
            //    int loc = 0;
            //    int size = 0;
            //    GetPositionOfMaxSubArray(
            //        new SubArray<byte>( result, Math.Max( 0, result.Count - 3792 ) ), 
            //        new SubArray<byte>( bytes, i, fe - 1 ), 
            //        out loc, out size );
            //    if( size != -1 && loc != -1 )
            //    {
            //        result.AddRange( BuildJump( loc, size ) );
            //        i += size - 1;
            //    }
            //    else
            //    {
            //        result.Add( bytes[i] );
            //    }
            //}

            //return result;
        }

        public static string BytesToString( IList<byte> bytes )
        {
            StringBuilder sb = new StringBuilder( bytes.Count );
            foreach( byte b in bytes )
            {
                sb.Append( (char)b );
            }
            return sb.ToString();
        }

        private static IList<byte> BuildJump( int jump, int length )
        {
            byte[] result = new byte[] { 0, 0, 0 };
            int l = length - 4;
            int j = CompressionJumps[jump];

            result[0] = (byte)(0xF0 | (byte)((l & 0x18) >> 3));
            result[1] = (byte)((l & 0x07) << 5);
            result[1] |= (byte)((j & 0x1F00) >> 8);
            result[2] = (byte)(j & 0xFF);

            return result;
        }

        private static void GetPositionOfMaxSubArray( IList<byte> window, IList<byte> bytes, out int position, out int maxLength )
        {
            if( bytes.Count >= 4 )
            {
                string windowString = GetString( window );
                string bytesString = GetString( bytes );

                for( int i = bytes.Count; i >= 4; i-- )
                {
                    string sub = bytesString.Substring( 0, i );
                    int loc = windowString.IndexOf( sub );
                    if( loc > -1 )
                    {
                        maxLength = i;
                        position = window.Count - loc;
                        return;
                    }
                }
            }

            maxLength = -1;
            position = -1;
        }

        private static string GetString( IList<byte> bytes )
        {
            StringBuilder sb = new StringBuilder( bytes.Count );
            foreach( byte b in bytes )
            {
                sb.Append( (char)b );
            }
            return sb.ToString();
        }
    }
}