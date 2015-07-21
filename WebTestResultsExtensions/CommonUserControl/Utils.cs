using Microsoft.VisualStudio.TestTools.WebTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebTestResultsExtensions
{
    public static class responseHelper
    {
        #region Public Methods

        /// <summary>
        /// Gets the cols.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string[] getCols(string input)
        {
            string pattern = @"\/\%\/";
            string[] cols = Regex.Split(input, pattern);
            return cols;
        }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string[] getRows(string input)
        {
            //string pattern = @"\/\%\/\|\%\|";
            //char[] delimiters = new char[] {'/','%','/','|','%','|' };
            //string[] rows2 = Regex.Split(input, pattern,RegexOptions.ExplicitCapture);
            string[] rows = input.Split(new string[] { "/%/|%|" }, StringSplitOptions.None);
            if (rows.Count() > 0)
            {
                string[] rowsFinal = new string[rows.Count() - 1];
                Array.Copy(rows, rowsFinal, rows.Count() - 1);
                return rowsFinal;
            }
            else
            {
                string[] rowsFinal = new string[0];
                return rowsFinal;
            }
        }

        #endregion Public Methods
    }

    public static class TestUtils
    {
        #region Public Methods

        /// <summary>
        /// Adds the context menu.
        /// </summary>
        /// <param name="rtb">The RTB.</param>
        public static void AddContextMenu(this RichTextBox rtb)
        {
            if (rtb.ContextMenuStrip == null)
            {
                ContextMenuStrip cms = new ContextMenuStrip { ShowImageMargin = false };

                ToolStripMenuItem tsmiCopy = new ToolStripMenuItem("Copy");
                tsmiCopy.Click += (sender, e) => rtb.Copy();
                cms.Items.Add(tsmiCopy);
                ToolStripMenuItem tsmiPaste = new ToolStripMenuItem("Paste");
                tsmiPaste.Click += (sender, e) => rtb.Paste();
                cms.Items.Add(tsmiPaste);
                rtb.ContextMenuStrip = cms;
            }
        }

        /// <summary>
        /// Gets the request string body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public static string GetRequestStringBody(IHttpBody body)
        {
            string result = string.Empty;

            if (body != null)
            {
                StringHttpBody b = (StringHttpBody)body;

                result = b.BodyString;
            }

            return result;
        }

        /// <summary>
        /// Gets the test results.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="urlWithQueryString">The URL with query string.</param>
        /// <param name="requestHttpVersion">The request HTTP version.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        public static string GetTestResults(string method, string urlWithQueryString, string requestHttpVersion, WebTestRequestHeaderCollection requestHeaders, StringHttpBody requestBody, WebTestResponse response)
        {
            string methodAndUrlWithQueryString = method + " " + urlWithQueryString + " " + "HTTP/" + requestHttpVersion;
            string reqHeaders = GetRequestHeader(requestHeaders);
            string reqBody = GetRequestStringBody(requestBody);
            string respHeaders = GetResponseHeader(response);
            string respBody = GetResponseStringBody(response);

            string finalResult = ">>> REQUEST <<<\n" + methodAndUrlWithQueryString + "\n" + reqHeaders + "\n" + reqBody + "\n\n>>> RESPONSE <<<\n" + respHeaders + "\n" + respBody;

            return finalResult;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the HTTP code.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        static string GetHttpCode(string description)
        {
            string code = string.Empty;

            switch (description)
            {
                case "OK":
                    code = "200";
                    break;

                case "Created":
                    code = "201";
                    break;

                case "Accepted":
                    code = "202";
                    break;

                case "NoContent":
                    code = "204";
                    break;

                case "BadRequest":
                    code = "400";
                    break;

                case "Unauthorized":
                    code = "401";
                    break;

                case "NotFound":
                    code = "404";
                    break;

                case "InternalServerError":
                    code = "500";
                    break;

                default:
                    code = "UNKNOWN CODE";
                    break;
            }

            return code;
        }

        /// <summary>
        /// Gets the request header.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        static string GetRequestHeader(WebTestRequestHeaderCollection headers)
        {
            string requestHeader = string.Empty;
            for (int i = 0; i < headers.Count; i++)
            {
                requestHeader = requestHeader + headers[i].Name + ": " + headers[i].Value + "\n";
            }

            return requestHeader;
        }

        /// <summary>
        /// Gets the response header.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        static string GetResponseHeader(WebTestResponse response)
        {
            string code = GetHttpCode(response.StatusCode.ToString());

            string responseHeader = "HTTP/" + response.ProtocolVersion + " " + code + " " + response.StatusDescription + "\n";
            for (int i = 0; i < response.Headers.Count; i++)
            {
                responseHeader = responseHeader + response.Headers.AllKeys[i] + ": " + response.Headers[i] + "\n";
            }

            return responseHeader;
        }

        /// <summary>
        /// Gets the response string body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        static string GetResponseStringBody(WebTestResponse body)
        {
            return body.BodyString;
        }

        #endregion Private Methods
    }
}