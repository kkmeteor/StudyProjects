using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    [TypeConverterAttribute(typeof(HelpSettingTypeConverter))]
    public class HelpSetting : DisplyTextCustomTypeDescriptor,IDisposable,ICloneable
    {
        private string _filename="";
        private string _keyword="";
        private string _keyindex="";

        public HelpSetting()
        {
        }

        public HelpSetting(HelpSetting hs)
        {
            _filename = hs.FileName;
            _keyword = hs.KeyWord;
            _keyindex = hs.KeyIndex;
        }

        [DisplayText("U8.UAP.Services.ReportElements.HelpSetting.帮助文件名")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.HelpSetting.不带语言后缀、不带文件后缀的文件名")]
        public string FileName
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }

        [DisplayText("U8.AA.EAI.XML.Operation.tStyle.0042")]
        [LocalizeDescription("U8.AA.EAI.XML.Operation.tStyle.0042")]
        public string KeyWord
        {
            get
            {
                return _keyword;
            }
            set
            {
                _keyword = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.HelpSetting.帮助号")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.HelpSetting.帮助号")]
        public string KeyIndex
        {
            get
            {
                return _keyindex;
            }
            set
            {
                _keyindex = value;
            }
        }


        #region IDisposable 成员

        public void Dispose()
        {
            _filename=null;
            _keyword=null;
            _keyindex=null;
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            return new HelpSetting(this);
        }

        #endregion
    }
}
