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
	public class ErrorManagement
	{
		protected List<string> ErrorMessages;

		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorManagement"/> class.
		/// </summary>
		protected ErrorManagement()
		{
		}

		/// <summary>
		/// Clears the error messages.
		/// </summary>
		protected void ClearErrorMessages()
		{
			ErrorMessages = null;
		}

		/// <summary>
		/// Sets the error message.
		/// </summary>
		/// <param name="error">Error Message</param>
		/// <returns></returns>
		public bool SetErrorMessage(string error)
		{
			if (ErrorMessages == null)
				ErrorMessages = new List<string>();

			ErrorMessages.Add(error);

			return false;
		}

		/// <summary>
		/// Sets the error message.
		/// </summary>
		/// <param name="errorManagement">Error Management</param>
		/// <returns></returns>
		public bool SetErrorMessage(ErrorManagement errorManagement)
		{
			if (errorManagement.ErrorMessages == null)
				return false;

			if (ErrorMessages == null)
				ErrorMessages = new List<string>();

			ErrorMessages.AddRange(errorManagement.ErrorMessages);

			return false;
		}

		/// <summary>
		/// Sets the error message.
		/// </summary>
		/// <param name="exception">Exception</param>
		/// <returns></returns>
		public bool SetErrorMessage(Exception exception)
		{
			if (exception == null)
				return false;

			if (ErrorMessages == null)
				ErrorMessages = new List<string>();

			ErrorMessages.Add(exception.ToString());

			return false;
		}

		/// <summary>
		/// Sets the error message.
		/// </summary>
		/// <param name="error">Error Message</param>
		/// <param name="errorManagement">Error Management</param>
		/// <returns></returns>
		protected bool SetErrorMessage(string error, ErrorManagement errorManagement)
		{
			SetErrorMessage(error);
			SetErrorMessage(errorManagement);

			return false;
		}

		/// <summary>
		/// Sets the error message.
		/// </summary>
		/// <param name="error">Error Message</param>
		/// <param name="exception">Exception</param>
		/// <returns></returns>
		protected bool SetErrorMessage(string error, Exception exception)
		{
			SetErrorMessage(error);
			SetErrorMessage(exception);

			return false;
		}

		/// <summary>
		/// Determines whether this instance is error.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is error; otherwise, <c>false</c>.
		/// </returns>
		public bool IsError()
		{
			return (ErrorMessages != null);
		}

		/// <summary>
		/// Determines whether this instance is ok.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is ok; otherwise, <c>false</c>.
		/// </returns>
		public bool IsOk()
		{
			return (ErrorMessages == null);
		}

		/// <summary>
		/// Gets the error message.
		/// </summary>
		/// <returns></returns>
		public string GetErrorMessage()
		{
			if (ErrorMessages == null)
				return String.Empty;

			return ErrorMessages[0];
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			return GetErrorMessage();
		}

	}
}
