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
	class GTPCommand
	{

		public int CommandNbr;
		public string Command;
		public string Parameters;
		public string Comment;
		public string RawCommand;
		public List<string> ParameterParts;

		public GTPCommand()
		{
			Clear();
		}

		public GTPCommand(string str)
		{
			ParseCommand(str);
		}

		public void Clear()
		{
			CommandNbr = 0;
			RawCommand = string.Empty;
			Command = string.Empty;
			Parameters = string.Empty;
			Comment = string.Empty;
			ParameterParts = new List<string>();
		}

		public int GetParameterCount()
		{
			return ParameterParts.Count;
		}

		public string GetParameter(int i)
		{
			return ParameterParts[i];
		}

		public bool GetParameter(int i, ref int p)
		{
			if (i >= ParameterParts.Count)
				return false;

			string lParameter = ParameterParts[i];

			if (lParameter.Length == 0)
				return false;

			return Int32.TryParse(lParameter, out p);
		}

		public bool GetParameter(int i, ref double p)
		{
			if (i >= ParameterParts.Count)
				return false;

			string lParameter = ParameterParts[i];

			if (lParameter.Length == 0)
				return false;

			return Double.TryParse(lParameter, out p);
		}

		public bool GetParameter(int i, ref Color color)
		{
			if (i >= ParameterParts.Count)
				return false;

			string lParameter = ParameterParts[i];

			if (lParameter.Length == 0)
				return false;

			if (lParameter.Trim().ToLower() == "both")
			{
				color = Color.Both;
				return true;
			}

			if (!Color.IsValidColor(lParameter))
				return false;

			color = Color.ToColor(lParameter);

			return true;
		}

        public bool GetParameter(int i, ref bool boolean)
        {
            if (i >= ParameterParts.Count)
                return false;

            string lParameter = ParameterParts[i];

            if (lParameter.Length == 0)
                return false;

            string lParam = lParameter.Trim().ToLower();

            if ((lParam == "t") || (lParam == "true") || (lParam == "y") || (lParam == "yes"))
            {
                boolean = true;
                return true;
            }

            if ((lParam == "f") || (lParam == "false") || (lParam == "n") || (lParam == "no"))
            {
                boolean = false;
                return true;
            }

            return false;
        }

		public void ParseCommand(string command)
		{
			Clear();

			RawCommand = command.Trim();

			if (RawCommand.Length == 0)
				return;

			string lCmd = RawCommand;

			// get comment
			int lCommentStart = RawCommand.IndexOf('#');

			if (lCommentStart >= 0)
			{
				Comment = lCmd.Substring(lCommentStart + 1).Trim();
				lCmd = lCmd.Substring(0, lCommentStart).Trim();
			}

			if (lCmd.Length == 0)
				return;

			// get command number
			if (Char.IsDigit(lCmd[0]))
			{
				int lEnd = lCmd.IndexOf(' ');

				if (!Int32.TryParse((lEnd <= 0) ? lCmd : lCmd.Substring(0, lEnd).Trim(), out CommandNbr))
					CommandNbr = 0;

				if (lEnd <= 0)
					return;

				lCmd = lCmd.Substring(lEnd).Trim();
			}

			string[] lWords = lCmd.Split(' ');

			foreach (string lWord in lWords)
				if (String.IsNullOrEmpty(Command))
					Command = lWord;
				else
				{
					ParameterParts.Add(lWord);

					Parameters = Parameters + ((Parameters.Length == 0) ? lWord : " " + lWord);
				}

			return;
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();

			lStringBuilder.Append(CommandNbr.ToString());
			lStringBuilder.Append(" ");
			lStringBuilder.Append(Command);

			foreach (string lParam in ParameterParts)
				lStringBuilder.Append(" " + lParam);

			if (!string.IsNullOrEmpty(Comment))
				lStringBuilder.Append(" # " + Comment);

			return lStringBuilder.ToString();
		}

	}
}
