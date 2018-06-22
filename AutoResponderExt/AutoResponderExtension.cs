using System;
using System.Collections.Generic;
using System.Text;
using Fiddler;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Xml;

namespace AutoResponderExt
{
    public class AutoResponderExtension : IAutoTamper, IFiddlerExtension
    {
        private List<ResponderRule> _orules = new List<ResponderRule>();
        private List<ResponderRuleExt> _orules2 = new List<ResponderRuleExt>();
        private ReaderWriterLock _RWLockRules = new ReaderWriterLock();
        private CheckBox ckAutoResponderExt;
        private TabPage epage;
        private List<string> lstDuplicate = new List<string>();
        private string path;
        private ResponderRule resprule;
        private ResponderRuleExt respruleExt;


        public AutoResponderExtension()
        {


        }

        public void AutoTamperRequestAfter(Session oSession)
        {



        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            //throw new NotImplementedException();
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
            if (this.ckAutoResponderExt.Checked && (FiddlerApplication.oAutoResponder.IsEnabled & this.lstDuplicate.Contains("EXACT:" + oSession.fullUrl)))
            {
                for (int i = 0; i < this._orules.Count; i++)
                {
                    if (this._orules[i].sMatch.Substring(6) == oSession.fullUrl)
                    {
                        try
                        {
                            this._RWLockRules.AcquireWriterLock(0x2710);
                            FiddlerApplication.oAutoResponder.RemoveRule(this._orules[i]);
                            this.resprule = FiddlerApplication.oAutoResponder.AddRule(this._orules2[i].strMatch, this._orules2[i].oResponseHeaders, this._orules2[i].arrResponseBytes, this._orules2[i].strDescription, this._orules2[i].iLatencyMS, this._orules2[i].bEnabled);
                            this.respruleExt = this._orules2[i];
                            this._orules.RemoveAt(i);
                            this._orules2.RemoveAt(i);
                            this._orules.Add(this.resprule);
                            this._orules2.Add(this.respruleExt);
                            i = this._orules.Count;
                        }
                        finally
                        {
                            this._RWLockRules.ReleaseWriterLock();
                        }
                    }
                }
            }

        }

        public void AutoTamperResponseBefore(Session oSession)
        {
            // throw new NotImplementedException();
        }

        public void OnBeforeReturningError(Session oSession)
        {
            //throw new NotImplementedException();
        }

        public void OnBeforeUnload()
        {
            //throw new NotImplementedException();
        }

        public void OnLoad()
        {

            this.epage = FiddlerApplication.UI.tabsViews.TabPages["pageResponder"];
            if (this.epage != null)
            {
                this.ckAutoResponderExt = new CheckBox();
                this.ckAutoResponderExt.Name = "ckAutoResponderExt";
                this.ckAutoResponderExt.Checked = false;
                this.ckAutoResponderExt.Width = 20;
                Label label = new Label();
                label.Name = "lblTxt";
                label.Text = "EnableAutoResponderExt";
                label.Font = new Font(label.Font, FontStyle.Bold);
                label.Width = 300;
                label.ForeColor = Color.Black;
                Button button = new Button();
                button.Text = "EnableExtension";
                button.Name = "btnctrl";
                button.Width = 100;
                button.Top = 20;
                button.Left = this.ckAutoResponderExt.Left + 20;
                button.FlatStyle = FlatStyle.Standard;
                button.Click += new EventHandler(this.btnCtrl_Click);
                this.epage.Controls.Add(button);
                this.epage.Controls.Add(label);
                this.epage.Controls.Add(this.ckAutoResponderExt);
                label.Left = this.ckAutoResponderExt.Left + this.ckAutoResponderExt.Width;
                label.Top = this.ckAutoResponderExt.Top + 5;
                this.epage.Controls["ckAutoResponderExt"].Dock = DockStyle.Top;
                this.epage.Controls["btnCtrl"].Dock = DockStyle.Top;
            }
        }


        private void btnCtrl_Click(object sender, EventArgs e)
        {
            this.ckAutoResponderExt = (CheckBox)this.epage.Controls["ckAutoResponderExt"];
            if (!this.ckAutoResponderExt.Checked)
            {
                MessageBox.Show("Check the EnableAutoResponderExt checkbox and then click the button", "AutoResponderExtension");
            }
            else if (FiddlerApplication.oAutoResponder.IsEnabled)
            {
                this.path = CONFIG.GetPath("MyDocs") + @"\Fiddler2\AutoResponderExt.xml";
                FiddlerApplication.oAutoResponder.SaveRules(this.path);
                this.LoadRules(this.path, true);
            }
        }

        public bool LoadRules(string sFilename, bool bIsDefaultRuleFile)
        {
            this._orules.Clear();
            this._orules2.Clear();
            this.lstDuplicate.Clear();
            if (bIsDefaultRuleFile)
            {
                FiddlerApplication.oAutoResponder.ClearRules();
            }
            try
            {
                if (!File.Exists(sFilename) || (new FileInfo(sFilename).Length < 0x8fL))
                {
                    return false;
                }
                FileStream input = new FileStream(sFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                XmlTextReader reader = new XmlTextReader(input);
                while (reader.Read())
                {
                    string str;
                    if (((reader.NodeType == XmlNodeType.Element) && ((str = reader.Name) != null)) && ((str != "State") && (str == "ResponseRule")))
                    {
                        try
                        {
                            string attribute = reader.GetAttribute("Match");
                            string sAction = reader.GetAttribute("Action");
                            int iLatencyMS = 0;
                            string s = reader.GetAttribute("Latency");
                            if (s != null)
                            {
                                iLatencyMS = XmlConvert.ToInt32(s);
                            }
                            bool bIsEnabled = "false" != reader.GetAttribute("Enabled");
                            string str5 = reader.GetAttribute("Headers");
                            if (string.IsNullOrEmpty(str5))
                            {
                                FiddlerApplication.oAutoResponder.IsRuleListDirty = false;
                                ResponderRule item = FiddlerApplication.oAutoResponder.AddRule(attribute, sAction, bIsEnabled);
                                this._orules.Add(item);
                                ResponderRuleExt ext = new ResponderRuleExt();
                                ext.arrResponseBytes = null;
                                ext.strMatch = attribute;
                                ext.strDescription = sAction;
                                ext.bEnabled = bIsEnabled;
                                ext.iLatencyMS = iLatencyMS;
                                ext.oResponseHeaders = null;
                                this._orules2.Add(ext);
                            }
                            else
                            {
                                byte[] buffer;
                                HTTPResponseHeaders oRH = new HTTPResponseHeaders();
                                str5 = Encoding.UTF8.GetString(Convert.FromBase64String(str5));
                                oRH.AssignFromString(str5);
                                string str6 = reader.GetAttribute("DeflatedBody");
                                if (!string.IsNullOrEmpty(str6))
                                {
                                    buffer = Utilities.DeflaterExpand(Convert.FromBase64String(str6));
                                }
                                else
                                {
                                    str6 = reader.GetAttribute("Body");
                                    if (!string.IsNullOrEmpty(str6))
                                    {
                                        buffer = Convert.FromBase64String(str6);
                                    }
                                    else
                                    {
                                        buffer = new byte[0];
                                    }
                                }
                                FiddlerApplication.oAutoResponder.IsRuleListDirty = false;
                                ResponderRule rule2 = FiddlerApplication.oAutoResponder.AddRule(attribute, oRH, buffer, sAction, iLatencyMS, bIsEnabled);
                                this._orules.Add(rule2);
                                ResponderRuleExt ext2 = new ResponderRuleExt();
                                ext2.arrResponseBytes = buffer;
                                ext2.strMatch = attribute;
                                ext2.strDescription = sAction;
                                ext2.bEnabled = bIsEnabled;
                                ext2.iLatencyMS = iLatencyMS;
                                ext2.oResponseHeaders = oRH;
                                this._orules2.Add(ext2);
                                try
                                {
                                    dictionary.Add(attribute, Guid.NewGuid().ToString());
                                }
                                catch (Exception)
                                {
                                    this.lstDuplicate.Add(attribute);
                                }
                            }
                            continue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                reader.Close();
                return true;
            }
            catch (Exception exception)
            {
                FiddlerApplication.ReportException(exception, "Failed to load AutoResponder settings from " + sFilename);
                return false;
            }
        }


    }

}
