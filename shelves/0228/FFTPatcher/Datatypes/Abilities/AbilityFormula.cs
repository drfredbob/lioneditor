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

using System.Collections.Generic;

namespace FFTPatcher.Datatypes
{
    public class AbilityFormula
    {
        public byte Value { get; private set; }
        public string Formula { get; private set; }

        public static List<AbilityFormula> PSXAbilityFormulas { get; private set; }
        public static Dictionary<byte, AbilityFormula> PSXAbilityFormulaHash { get; private set; }

        public static List<AbilityFormula> PSPAbilityFormulas { get; private set; }
        public static Dictionary<byte, AbilityFormula> PSPAbilityFormulaHash { get; private set; }

        static AbilityFormula()
        {
            PSXAbilityFormulas = new List<AbilityFormula>( Resources.AbilityFormulas.Count );
            PSXAbilityFormulaHash = new Dictionary<byte, AbilityFormula>( Resources.AbilityFormulas.Count );

            PSPAbilityFormulas = new List<AbilityFormula>( Resources.AbilityFormulas.Count );
            PSPAbilityFormulaHash = new Dictionary<byte, AbilityFormula>( Resources.AbilityFormulas.Count );

            foreach (KeyValuePair<byte, string> kvp in Resources.AbilityFormulas)
            {
                AbilityFormula a = new AbilityFormula();
                a.Value = kvp.Key;
                a.Formula = kvp.Value;

                if( a.Value < 0x65 )
                {
                    PSXAbilityFormulas.Add( a );
                    PSXAbilityFormulaHash.Add( a.Value, a );
                }
                PSPAbilityFormulas.Add( a );
                PSPAbilityFormulaHash.Add( a.Value, a );
            }
        }

        private AbilityFormula()
        {
        }

        public override string ToString()
        {
            return string.Format( "{0:X2} {1}", Value, Formula );
        }
    }
}
