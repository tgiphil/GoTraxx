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
	class SGFNode : ErrorManagement
	{
		public List<SGFProperty> Properties;

		public SGFNode()
		{
			Properties = new List<SGFProperty>();
		}

		public SGFNode(MemFile memFile)
		{
			Properties = new List<SGFProperty>();
			Read(memFile);
		}

		public bool Read(MemFile memFile)
		{
			char c = memFile.Get();

			if (c != ';')
				return SetErrorMessage("Expecting semi-colon, found: " + c.ToString());

			while (true)
			{
				if (memFile.EOF)
					return true;

				c = memFile.Peek();

				if (c == '(')
					break;	// variation

				if ((c == ';') || (c == ')'))
					break;

				if (Char.IsLetter(c))
				{
					SGFProperty lSGFProperty = new SGFProperty(memFile);

					if (lSGFProperty.IsError())
						return SetErrorMessage(lSGFProperty);

					AddProperty(lSGFProperty);

					while (true)
					{
						c = memFile.Peek();

						if (c == ')')
							break;

                        if (c == '(')
                            break;

						if (c == '[')
						{
							memFile.Get(); // eat this character	

							SGFProperty lSGFProperty2 = new SGFProperty(memFile, lSGFProperty.PropertyID);

							if (lSGFProperty2.IsError())
								return SetErrorMessage(lSGFProperty2); ;

							AddProperty(lSGFProperty2);
						}
						else
							if (!Char.IsLetter(c))
							{
								memFile.Get();	// eat this character	

								if (memFile.EOF)
									return SetErrorMessage("Unexpected EOF.");
							}
							else
								break;

					}
				}
				else
					memFile.Get();	// eat this character

				if (memFile.EOF)
					return SetErrorMessage("Unexpected EOF.");

			}

			return true;
		}

		public void AddProperty(SGFProperty sgfProperty)
		{
			Properties.Add(sgfProperty);
		}

		public void AddPropertyIfNotEmpty(SGFProperty sgfProperty)
		{
			if (!string.IsNullOrEmpty(sgfProperty.Text))
				AddProperty(sgfProperty);
		}

		public void Clear()
		{
			Properties.Clear();
		}

		public override string ToString()
		{
			StringBuilder lStringBuilder = new StringBuilder();
			lStringBuilder.Append(";");

			SGFProperty lLastSGFProperty = null;

			foreach (SGFProperty lSGFProperty in Properties)
			{
				if (lLastSGFProperty == null)
					lStringBuilder.Append(lSGFProperty.ToString());
				else
					if (lLastSGFProperty.PropertyID == lSGFProperty.PropertyID)
						lStringBuilder.Append(lSGFProperty.ToStringNoPropertyID());
					else
					{
						lStringBuilder.AppendLine();
						lStringBuilder.Append(lSGFProperty.ToString());
					}

				lLastSGFProperty = lSGFProperty;
			}

			lStringBuilder.AppendLine();

			return lStringBuilder.ToString();
		}

		public bool RetrieveGame(GameRecord gameRecord)
		{
			foreach (SGFProperty lSGFProperty in Properties)
				if (!lSGFProperty.RetrieveGame(gameRecord))
					return false;

			return true;
		}
	}

}
