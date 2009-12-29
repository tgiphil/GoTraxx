/*
 * Copyright (c) 2007 Philipp Garcia (phil@gotraxx.org)
 * 
 * This file is part of GoTraxx (www.gotraxx.org).
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * This license governs use of the accompanying software. If you use the software, you 
 * accept this license. If you do not accept the license, do not use the software.
 * 
 * Permission is granted to anyone to use this software for any noncommercial purpose, 
 * and to alter it and redistribute it freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not claim that 
 *    you wrote the original software. 
 * 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 *    misrepresented as being the original software.
 * 
 * 3. If you bring a patent claim against the original author or any contributor over 
 *    patents that you claim are infringed by the software, your patent license from 
 *    such contributor to the software ends automatically.
 * 
 * 4. This software may not be used in whole, nor in part, to enter any competition 
 *    without written permission from the original author. 
 * 
 * 5. This notice may not be removed or altered from any source distribution.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	class SGFProperty : ErrorManagement
    {
        public string PropertyID;
        public string Text;

        public SGFProperty()
        {
            PropertyID = string.Empty;
            Text = string.Empty;
        }

        public SGFProperty(string propertyID, string text)
        {
            PropertyID = propertyID;
            Text = text;
        }

        public SGFProperty(string propertyID, int pInteger)
        {
            PropertyID = propertyID;
            Text = pInteger.ToString();
        }

        public SGFProperty(MemFile memFile)
        {
            PropertyID = string.Empty;
            Text = string.Empty;
            Read(memFile);
        }

        public SGFProperty(MemFile memFile, string propertyID)
        {
            Read(memFile, propertyID);
        }

        public bool Read(MemFile memFile)
        {
            PropertyID = string.Empty;
            Text = string.Empty;

            while (true)
            {
                char c = memFile.Get();

                if (c == '[')
                    break;

                if (Char.IsLetter(c))
                    PropertyID = PropertyID + Char.ToUpper(c);

                if (memFile.EOF)
                    return SetErrorMessage("Unexpected EOF.");
            }

            while (true)
            {
                char c = memFile.Get();

                if (c == ']')
                    break;

                if (c == '\\')
                    c = memFile.Get();

                Text = Text + c;

                if (memFile.EOF)
                    return SetErrorMessage("Unexpected EOF.");
            }

            return true;
        }

        public bool Read(MemFile memFile, string propertyID)
        {
            PropertyID = propertyID;
            Text = string.Empty;

            while (true)
            {
                char c = memFile.Get();

                if (c == ']')
                    break;

                if (c == '\\')
                    c = memFile.Get();

                Text = Text + c;

                if (memFile.EOF)
                    return SetErrorMessage("Unexpected EOF.");
            }

            return true;
        }

        public override string ToString()
        {
            return PropertyID + '[' + Text + ']';
        }

        public string ToStringNoPropertyID()
        {
            return '[' + Text + ']';
        }

        public bool RetrieveGame(GameRecord gameRecord)
        {
            switch (PropertyID)
            {
                case "S":
                    {
                        string lMoves = Text;

                        // Note: SmartGo includes \n and \r characters in the list of moves
                        while (lMoves.IndexOf('\n') >= 0)
                            lMoves = lMoves.Remove(lMoves.IndexOf('\n'));

                        while (lMoves.IndexOf('\r') >= 0)
                            lMoves = lMoves.Remove(lMoves.IndexOf('\r'));
                        
                        // Compressed SFG
                        if (lMoves.Length % 2 != 0)
                            return false;

                        for (int i = 0; i < lMoves.Length / 2; i++)
                        {
                            string lText = lMoves.Substring(i * 2, 2);

                            gameRecord.PlayStone(CoordinateSystem.AtFromSGF(lText, gameRecord.BoardSize));
                        }

                        return true;
                    }

                case "W":
                    {
                        if (Text.Length == 0)
                        {
                            gameRecord.PlayStone(Color.White, CoordinateSystem.PASS);
                            return true;
                        }

                        if (Text.Length != 2)
                            return false;

                        gameRecord.PlayStone(Color.White, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                case "B":
                    {
                        if (Text.Length == 0)
                        {
                            gameRecord.PlayStone(Color.Black, CoordinateSystem.PASS);
                            return true;
                        }

                        if (Text.Length != 2)
                            return false;

                        gameRecord.PlayStone(Color.Black, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                case "AW":
                    {
                        if (Text.Length != 2)
                            return false;

                        gameRecord.SetupStone(Color.White, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                case "AB":
                    {
                        if (Text.Length != 2)
                            return false;

                        gameRecord.SetupStone(Color.Black, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                case "PL":
                    {
                        if (Text.Length == 0)
                            return false;

                        if (!Color.IsValidColor(Text))
                            return false;

                        gameRecord.NextPlayer = Color.ToColor(Text);

                        return true;
                    }
                case "AE":
                    {
                        return false;	// clear points - not implemented
                    }
                case "DT":
                    {
                        gameRecord.Date = Text;
                        return true;
                    }

                case "PB":
                    {
                        gameRecord.BlackPlayerName = Text;
                        return true;
                    }

                case "PW":
                    {
                        gameRecord.WhitePlayerName = Text;
                        return true;
                    }
                //				case "RS":
                //					{
                //						gameRecord.Result = Text;
                //						return true;
                //					}
                case "GN":
                    {
                        gameRecord.GameName = Text;
                        return true;
                    }
                case "ID":
                    {
                        gameRecord.Identification = Text;
                        return true;
                    }
                case "C":
                    {
                        gameRecord.Comment = Text;
                        return true;
                    }
                case "RU":
                    {
                        gameRecord.Rules = Text;
                        return true;
                    }
                case "RE":
                    {
                        gameRecord.Result = Text;
                        return true;
                    }
                case "PC":
                    {
                        gameRecord.Place = Text;
                        return true;
                    }
                case "BR":
                    {
                        gameRecord.BlackRank = Text;
                        return true;
                    }
                case "WR":
                    {
                        gameRecord.WhiteRank = Text;
                        return true;
                    }
                case "HE":
                    {
                        int lHandicapStones = 0;

                        if (!Int32.TryParse(Text.Trim(), out lHandicapStones))
                            return false;

                        gameRecord.HandicapStones = lHandicapStones;

                        return true;
                    }
                case "SZ":
                    {
                        int lBoardSize = 0;

                        if (!Int32.TryParse(Text.Trim(), out lBoardSize))
                            return false;

                        if ((lBoardSize > 19) && (lBoardSize > 1))
                            return false;

                        gameRecord.BoardSize = lBoardSize;

                        return true;
                    }
                case "TW":
                    {
                        if (Text.Length != 2)
                            return false;

                        gameRecord.SetTerritory(Color.White, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                case "TB":
                    {
                        if (Text.Length != 2)
                            return false;

                        gameRecord.SetTerritory(Color.Black, CoordinateSystem.AtFromSGF(Text, gameRecord.BoardSize));

                        return true;
                    }
                default:
                    return true;
            }
        }
    }
}
