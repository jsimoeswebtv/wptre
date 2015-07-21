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

        const string INDENT_STRING = "    ";

        #endregion Private Fields

        #region Public Constructors

        public WebTestResultControl()
        {
            InitializeComponent();
            resultControlDataGridView.AddContextMenu();
        }

        #endregion Public Constructors

        #region Public Methods

        public void UpdateGrid()
        {
            this.resultControlDataGridView.Text = "";
        }

        public void Update(WebTestRequestResult WebTestResults)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi\deff0");
            sb.Append(@"{\colortbl;red0\green0\blue0; \red255\green0\blue0;\red0\green255\blue0;\red0\green0\blue255;}");

            this.resultControlDataGridView.Text = "";

            sb.Append(writeRTF(WebTestResults));
            int count = 0;
            foreach (WebTestRequestResult item in WebTestResults.DependantResults)
            {
                if (count == 0)
                    sb.Append(@"\b*************************DEPENDANT REQUESTS******************************\b0" + @" \line ");
                sb.Append(writeRTF(item));
                count += 1;
            }

            sb.Append(@"}");
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        public void UpdateComment(WebTestResultComment WebTestResults)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi\deff0");
            sb.Append(@"{\colortbl;\red255\green0\blue0;\red0\green255\blue0;\red0\green0\blue255;}");
            sb.Append(@"\b *************************COMMENT****************************** \b0 ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            this.resultControlDataGridView.Text = "";

            sb.Append(WebTestResults.Comment);
            sb.Append(@" \line ");
            sb.Append(@"}");
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        public void updateTotal(WebTestRequestResult x, int i)
        {
            var sb = new StringBuilder();
            sb.Append(@"{\rtf1\ansi\deff0");
            sb.Append(@"{\colortbl;\red255\green0\blue0;\red0\green255\blue0;\red0\green0\blue255;}");
            sb.Append(@"\b *************************ITERATION " + i + @"****************************** \b0 ");
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            sb.Append(x.ToString());
            sb.Append(@"}");
            this.resultControlDataGridView.Rtf = sb.ToString();
        }

        #endregion Public Methods

        #region Private Methods

        static string FormatJson(string json)
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

        static string GetRtfUnicodeEscapedString(string s)
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

        string writeRTF(WebTestRequestResult WebTestResults)
        {
            StringBuilder sb = new StringBuilder();
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
            if (WebTestResults.Request.ContentType.IndexOf("json", StringComparison.Ordinal) > 0)
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
            if (WebTestResults.Response.ContentType.IndexOf("json", StringComparison.Ordinal) > 0)
            {
                string Uni = FormatJson(WebTestResults.Response.BodyString);
                Uni = GetRtfUnicodeEscapedString(Uni);
                sb.Append(Uni + @" \line ");
            }
            else
            {
                if (WebTestResults.Response.BodyString.IndexOf("/%/|%|", StringComparison.Ordinal) > 0)
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

            int count = 0;
            int counter = 0;
            string suc;
            foreach (RuleResult ruleResult in WebTestResults.ExtractionRuleResults)
            {
                if (counter == 0)
                    sb.Append(@"\b**************************OTHER*****************************\b0" + @" \line ");
                if (count == 0)
                    sb.Append("extraction rule results: " + @" \line ");
                if (ruleResult.Success)
                {
                    suc = @"\cf3" + @"Passed\cf1";
                    sb.Append(ruleResult.Name + " - " + suc + @" \line ");
                    sb.Append("Message : " + ruleResult.Message.ToString() + @" \line ");
                }
                else
                {
                    suc = @"\cf2" + @"False\cf1";
                    sb.Append(ruleResult.Name + " - " + suc + @" \line ");
                    sb.Append("Message : " + ruleResult.Message.ToString() + @" \line ");
                    sb.Append("Exception : " + ruleResult.Exception + @" \line ");
                }
                count += 1;
                counter += 1;
            }
            count = 0;

            foreach (RuleResult ruleResult in WebTestResults.ValidationRuleResults)
            {
                if (count == 0)
                    sb.Append(@" \line ");
                sb.Append("Validation rule results: " + @" \line ");
                if (ruleResult.Success)
                {
                    suc = @"\cf3" + @"Passed\cf1";
                    sb.Append(ruleResult.Name + " - " + suc + @" \line ");
                    sb.Append("Message : " + ruleResult.Message.ToString() + @" \line ");
                }
                else
                {
                    suc = @"\cf2" + @"False\cf1";
                    sb.Append(ruleResult.Name + " - " + suc + @" \line ");
                    sb.Append("Message : " + ruleResult.Message.ToString() + @" \line ");
                    sb.Append("Exception : " + ruleResult.Exception + @" \line ");
                }
                count += 1;
                counter += 1;
            }
            count = 0;

            foreach (WebTestError webTestError in WebTestResults.Errors)
            {
                if (count == 0)
                    sb.Append("Errors: " + @" \line ");
                sb.Append(webTestError.ErrorType.ToString() + " " + webTestError.ErrorSubtype.ToString() + " " + webTestError.ErrorText.ToString() + @" \line ");
                count += 1;
                counter += 1;
            }
            sb.Append(@" \line ");
            sb.Append(@" \line ");
            if (counter > 0)
                sb.Append(@"\b*******************************************************\b0" + @" \line ");
            return sb.ToString();
        }

        #endregion Private Methods
    }
}