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
using System.IO;

namespace GoTraxx
{
	public class MemFile : ErrorManagement
	{
		protected StringBuilder Data;
		protected int Index;
		protected int LineCnt;

		public MemFile()
		{
			Index = 0;
			Data = new StringBuilder(1024);
			LineCnt = 0;
		}

		public MemFile(string filename)
		{
			Index = 0;
			Data = new StringBuilder(1024);
			LineCnt = 0;
			LoadFile(filename);
		}

		public bool EOF
		{
			get
			{
				return (Index >= Data.Length);
			}
		}

		public int LineNbr
		{
			get
			{
				return LineCnt;
			}
		}

		public void Reset()
		{
			Index = 0;
			LineCnt = 0;
		}

		public char Get()
		{
			if (EOF)
				return '\0';

			return Data[Index++];
		}

		public char Peek()
		{
			if (EOF)
				return '\0';

			return Data[Index];
		}

		public void Get(ref char c)
		{
			c = Get();
		}

		public void PushBack()
		{
			if (Index != 0)
				Index--;
		}

		public string ReadLine(char eol, int max)
		{
			int lStart = Index;
			LineCnt++;

			while (((Index - lStart) < max) && (!EOF) && (Get() != eol)) ;

			return Data.ToString(lStart, Index - lStart);
		}

		public string ReadLine(char eol)
		{
			int lStart = Index;
			LineCnt++;

			while ((!EOF) && (Get() != eol)) ;

			return Data.ToString(lStart, Index - lStart);
		}

		public string ReadLine()
		{
			return ReadLine('\n');
		}

		public string ReadPart(char seperator, char eol)
		{
			int lStart = Index;

			while ((!EOF))
			{
				char c = Get();
				if (c == eol)
				{
					LineCnt++;
					break;
				}
				if (c == seperator)
					break;
			}

			return Data.ToString(lStart, Index - lStart - 1);
		}

		public string ReadPart(char seperator)
		{
			return ReadPart(seperator, '\n');
		}

		public void Write(char c)
		{
			Data.Append(c);
		}

		public void WriteLine()
		{
			Data.Append('\n');
		}

		public void WriteLine(char c)
		{
			Data.Append(c);
			Data.Append('\n');
		}

		public void Write(string str)
		{
			Data.Append(str);
		}

		public void Write(string str, int pLen)
		{
			Data.Append(str.Substring(0, pLen));
		}

		public void WriteLine(string str)
		{
			Data.Append(str);
			Data.Append('\n');
		}

		public void Clear()
		{
			Data = new StringBuilder(1024);
		}


		public override string ToString()
		{
			return Data.ToString();
		}

		public bool SaveFile(string filename)
		{
			ClearErrorMessages();

			try
			{
				// Create an instance of StreamWriter to write to the file.
				// The using statement also closes the StreamWriter.
				using (StreamWriter sw = new StreamWriter(filename))
				{
					sw.Write(Data);
				}

				return true;
			}
			catch (Exception e)
			{
				// Let the user know what went wrong.
				return SetErrorMessage("ERROR: " + e.Message);
			}
		}

		public bool LoadFile(string filename)
		{
			ClearErrorMessages();
			LineCnt = 0;
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(filename))
				{
					string lData = sr.ReadToEnd();
					Data = new StringBuilder(lData);
				}

				return true;
			}
			catch (Exception e)
			{
				// Let the user know what went wrong.
				return SetErrorMessage("ERROR: " + e.Message);
			}
		}

		public bool ReadConsole()
		{
			ClearErrorMessages();
			LineCnt = 0;
			Data.Length = 0;

			try
			{
				string lLine = string.Empty;

				while ((lLine = Console.ReadLine()) != null)
					Data.Append(lLine);

				return true;

			}
			catch (Exception e)
			{
				// Let the user know what went wrong.
				return SetErrorMessage("ERROR: " + e.Message);
			}
		}

		public bool OutputConsole()
		{
			Console.Write(Data);
			return true;
		}

	}
}
