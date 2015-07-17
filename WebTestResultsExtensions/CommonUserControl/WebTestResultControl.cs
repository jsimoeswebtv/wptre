using Microsoft.VisualStudio.TestTools.WebTesting;
using Microsoft.VisualStudio.TestTools.WebTesting.Rules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebTestResultsExtensions
{
    public partial class WebTestResultControl : UserControl
    {
        #region Private Fields

        private const string INDENT_STRING = "    ";

        #endregion Private Fields

        #region Public Constructors

        public WebTestResultControl()
        {
            InitializeComponent();
            resultControlDataGridView.AddContextMenu();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Update()
        {
            this.resultControlDataGridView.Text = "";
        }

        public void Update(WebTestRequestResult WebTestResults)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi");

            this.resultControlDataGridView.Text = "";
            sb.Append(@"\b*************************REQUEST******************************\b0" + @" \line ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(WebTestResults.Request.UrlWithQueryString.ToString() + @" \line ");
            sb.Append("Method= " + WebTestResults.Request.Method + @" \line ");
            sb.Append("HTTP " + WebTestResults.Request.Version + @" \line ");

            sb.Append(@"\b HEADERS \b0" + @" \line ");
            foreach (WebTestRequestHeader item in WebTestResults.Request.Headers)
            {
                sb.Append(item.Name + " = " + item.Value + @" \line ");
            }
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(@"\b QUERYSTRING \b0" + @" \line ");
            foreach (var t in WebTestResults.Request.QueryStringParameters)
            {
                sb.Append(t.Name + " = " + t.Value + @" \line ");
            }
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(@"\b BODY \b0" + @" \line ");
            if (WebTestResults.Request.ContentType.IndexOf("json") > 0)
            {
                string UniReq = TestUtils.GetRequestStringBody(WebTestResults.Request.Body);
                UniReq = FormatJson(UniReq);
                UniReq = GetRtfUnicodeEscapedString(UniReq);
                sb.Append(UniReq + @" \line ");
            }
            else
            {
                sb.Append(TestUtils.GetRequestStringBody(WebTestResults.Request.Body) + @" \line ");
            }

            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(@"\b***************************RESPONSE****************************\b0" + @" \line ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            //sb.Append(WebTestResults.Response.ResponseUri.ToString() + @" \line ");
            sb.Append(@"\b HEADERS \b0" + @" \line ");
            WebHeaderCollection headers = WebTestResults.Response.Headers;
            for (int i = 0; i < headers.Count; ++i)
            {
                string header = headers.GetKey(i);
                foreach (string value in headers.GetValues(i))
                {
                    sb.Append(String.Format("{0} = {1}", header, value) + @" \line ");
                }
            }

            sb.Append(@" \line ");
            sb.Append(@"\b BODY \b0" + @" \line ");
            if (WebTestResults.Response.ContentType.IndexOf("json") > 0)
            {
                string Uni = FormatJson(WebTestResults.Response.BodyString);
                Uni = GetRtfUnicodeEscapedString(Uni);
                sb.Append(Uni + @" \line ");
            }
            else
            {
                if (WebTestResults.Response.BodyString.IndexOf("/%/|%|") > 0)
                {
                    int row = 1;
                    foreach (var str in responseHelper.getRows(WebTestResults.Response.BodyString))
                    {
                        string[] cols = responseHelper.getCols(str);
                        sb.Append(@"\b " + row + @"- \b0 " + str + @" - \b " + cols.Length.ToString() + @" Cols\b0" + @" \line ");
                        row += 1;
                    }
                }
                else
                {
                    sb.Append(WebTestResults.Response.BodyString + @" \line ");
                }
            }

            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append("**************************OTHER*****************************" + @" \line ");
            sb.Append("extraction rule results: " + @" \line ");
            foreach (RuleResult ruleResult in WebTestResults.ExtractionRuleResults)
            {
                sb.Append(ruleResult.Name + " - " + ruleResult.Message.ToString() + @" \line ");
            }
            sb.Append(@" \line ");
            sb.Append("Validation rule results: " + @" \line ");
            foreach (RuleResult ruleResult in WebTestResults.ValidationRuleResults)
            {
                string sucesss = "";
                if (ruleResult.Success)
                    sucesss = "Passed";
                else
                    sucesss = "Failed";
                sb.Append(ruleResult.Name + " - " + sucesss + " - " + ruleResult.Message.ToString() + " - " + ruleResult.Exception + @" \line ");
            }
            sb.Append(@" \line ");
            sb.Append("Errors: " + @" \line ");
            foreach (WebTestError webTestError in WebTestResults.Errors)
            {
                sb.Append(webTestError.ErrorType.ToString() + " " + webTestError.ErrorSubtype.ToString() + " " + webTestError.ErrorText.ToString() + @" \line ");
            }
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append("*******************************************************" + @" \line ");
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        public void UpdateComment(WebTestResultComment WebTestResults)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi");
            sb.Append(@"\b *************************COMMENT****************************** \b0 ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            this.resultControlDataGridView.Text = "";

            sb.Append(WebTestResults.Comment);
            sb.Append(@" \line ");
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        public void updateTotal(WebTestRequestResult x, int i)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi");
            sb.Append(@"\b *************************ITERATION " + i + @"****************************** \b0 ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(x.ToString());
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        #endregion Public Methods

        #region Private Methods

        private static string FormatJson(string json)
        {
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null
                            ? openChar.Length > 1
                                ? openChar
                                : closeChar
                            : lineBreak;

            return String.Concat(result);
        }

        private static string GetRtfUnicodeEscapedString(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c == '\\' || c == '{' || c == '}')
                    sb.Append(@"\" + c);
                else if (c <= 0x7f)
                    sb.Append(c);
                else
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
            }
            return sb.ToString();
        }

        #endregion Private Methods
    }
}