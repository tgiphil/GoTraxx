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
	class GTPEngine
	{
		protected GTPCommBase Out;
		protected GTPGoBoard GTPGoBoard;
		protected StringBuilder InputBuffer;

		public GTPEngine(GTPGoBoard gtpGoBoard, GTPCommBase outComm)
		{
			GTPGoBoard = gtpGoBoard;
            Out = outComm;

			InputBuffer = new StringBuilder(250);
            outComm.SetGTPEngine(this);
		}

		public void Receive(char c)
		{
			if ((c == '\n') || (c == '\r'))
				ExecuteCommand();
			else
				if (Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsSeparator(c) || Char.IsSymbol(c) || (c == ' '))
					InputBuffer.Append(c);
				else
					if (c == '\t')	// remove tab with space
						InputBuffer.Append(' ');
					else
						if (c == 8) // backspace character
							if (InputBuffer.Length >= 1)
								InputBuffer.Remove(InputBuffer.Length - 1, 1);			
		}

		public void Receive(string str)
		{
			foreach (char lChar in str)
				Receive(lChar);
		}

		protected void SendResponse(bool success, int commandNbr, string str)
		{
			StringBuilder lString = new StringBuilder(str.Length + 20);

			lString.Append(success ? '=' : '?');

			if (commandNbr > 0)
				lString.Append(commandNbr);

			if (str.Length > 0)
			{
				lString.Append(' ');
				lString.Append(str);
			}

			lString.Append("\n\n");

			// send it to the client 
			Out.SendToClient(lString.ToString());
		}

		protected void ExecuteCommand()
		{
			if (InputBuffer.Length==0)
				return;

			GTPCommand lGTPCommand = new GTPCommand(InputBuffer.ToString());

			if (lGTPCommand.Command.Length == 0)
				return;

			InputBuffer = new StringBuilder(250);

            GTPInternalResponse lGTPInternalResponse = GTPGoBoard.ExecuteComand(lGTPCommand);

			SendResponse(lGTPInternalResponse.IsOk(),lGTPCommand.CommandNbr,lGTPInternalResponse.Message);
		}

	}
}
