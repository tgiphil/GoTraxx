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
using NUnit.Framework;

namespace GoTraxx
{
	[TestFixture]
	public class GTPCommandTest
	{

		/// <summary>
		/// Verifies the specified GTP string.
		/// </summary>
		/// <param name="gtpString">The GTP string.</param>
		/// <param name="commandNbr">The command NBR.</param>
		/// <param name="command">The command.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="comment">The comment.</param>
		/// <returns></returns>
		protected static bool Verify(string gtpString, int commandNbr, string command, string parameters, string comment)
		{
			GTPCommand lGTcommand = new GTPCommand(gtpString);

			if ((lGTcommand.Command != command) ||
				(lGTcommand.Parameters != parameters) ||
				(lGTcommand.Comment != comment) ||
				(lGTcommand.CommandNbr != commandNbr))
				return false;

			return true;
		}

		[Test]
		public void Tests()
		{
			Assert.IsTrue(Verify("99 ABC 12#testing", 99, "ABC", "12", "testing"));
			Assert.IsTrue(Verify(" ABC 1#testing", 0, "ABC", "1", "testing"));
			Assert.IsTrue(Verify("   ABC 987 654 # testing", 0, "ABC", "987 654", "testing"));
			Assert.IsTrue(Verify("ABC # testing", 0, "ABC", string.Empty, "testing"));
			Assert.IsTrue(Verify("ABC# testing", 0, "ABC", string.Empty, "testing"));
			Assert.IsTrue(Verify("ABC#testing", 0, "ABC", string.Empty, "testing"));
			Assert.IsTrue(Verify("ABC 987#testing", 0, "ABC", "987", "testing"));
			Assert.IsTrue(Verify("ABC 234908 230948243 0982340928  234 #testing", 0, "ABC", "234908 230948243 0982340928  234", "testing"));
			Assert.IsTrue(Verify("ABC 12#testing", 0, "ABC", "12", "testing"));
		}
	}
}
