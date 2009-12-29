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
	class PatternOperand
	{
		public PatternToken Token;

		// depending on the token:
		public int Integer;
		public char Variable;
		public PatternFunction Function;

		protected string Source;	// used for debugging

		public PatternOperand()
		{
			Token = PatternToken.UNKNOWN;
		}

		public PatternOperand(PatternToken token)
		{
			Token = token;
		}

		public PatternOperand(PatternToken token, string tokenString, int paramCount)
		{
			Token = token;
			Source = tokenString; // used for debugging

			if (token == PatternToken.INTEGER)
				Integer = Convert.ToInt32(tokenString);
			else if (token == PatternToken.VARIABLE)
				Variable = tokenString[0];
			else if (token == PatternToken.UNKNOWN)
				return;
			else if (token == PatternToken.FUNCTION)
			{
				Integer = paramCount;
				Function = PatternFunctions.GetFunction(tokenString, paramCount);
			}
		}

		public override string ToString()
		{
			return Source;

			/*
			if (mToken == PatternToken.INTEGER)
				return mInteger.ToString();

			if (mToken == PatternToken.VARIABLE)
				return mVariable.ToString();
			
			if (mToken == PatternToken.FUNCTION)
				return mFunction.ToString();			

			if (mToken == PatternToken.UNKNOWN)
				return "[ UNKNOWN TOKEN ]";

			return mToken.ToString();
			  
			*/
		}


	}
}
