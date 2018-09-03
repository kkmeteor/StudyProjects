using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public enum FindDirection
    {
        Up = 0,             //���ϲ���
        Down = 1,            //���²���
    }

    [Serializable]
    public class QuickLocationItem
    {
        private string _fieldName="";
        private string _cellName = "";
        private bool _bCurPage=true;
        private FindDirection _direction=FindDirection.Down;        
        private int _currentRow=0;        
        private bool _findResult = false;
        int _groupLevel = 0;            //�����鼶�Σ������ɴ��ж��Ƿ���ڷ���
        int _currentLevel = 1;          //��ǰ����
        bool _showDetail = true;
        int[] _arrGrade = new int[100]; //��¼���鼶��ѭ����λ��������
        int _lastDeep = 0;              //ѭ�����        
        string _pattern = "";           //ƥ���ʽ��
        string _dbMatchSql = "";        //���ݿ�ƥ��sql��
        bool _bFirstLocation = true;    //�Ƿ��״ζ�λ    
        bool _bReStartRow = false;      //�Ƿ������趨��ʼ��
        int _sortItemsCount = 0;        //������Ŀ����
        bool _bPageByGroup = false;     //�Ƿ����ҳ
        ArrayList _TopGroupItem;        //��������
        string _baseId = "baseid";       //baseid�ֶ���

        /// <summary>
        /// ��λ�ֶ�, ��FieldName="*"��ʾ�����ֶζ�λ
        /// </summary>
        public string FieldName
        {
            get{ return _fieldName; }
            set{_fieldName = value; }
        }

        /// <summary>
        /// ��λ��Ԫ��, ��CellName="*"��ʾ�����ֶζ�λ
        /// </summary>
        public string CellName
        {
            get{return _cellName;}
            set { _cellName = value;}
        }

        /// <summary>
        /// baseid�ֶ���
        /// </summary>
        public string BaseId
        {
            get { return _baseId; }
            set { _baseId= value; }
        }
        
        /// <summary>
        /// �Ƿ�λ��ǰҳ
        /// </summary>
        public bool CurrentPage
        {
            get{ return _bCurPage;}
            set{ _bCurPage = value; }
        }

        /// <summary>
        /// �Ƿ��״ζ�λ   
        /// </summary>
        public bool FirstLocation
        {
            get { return _bFirstLocation; }
            set { _bFirstLocation=value;}
        }

        /// <summary>
        /// �Ƿ������趨��ʼ��
        /// </summary>
        public bool ReStartRow
        {
            get { return _bReStartRow; }
            set { _bReStartRow = value; }
        }

        /// <summary>
        /// ��λ����
        /// </summary>
        public FindDirection Direction
        {
            get{return _direction; }
            set { _direction = value; }
        }       

        /// <summary>
        /// �Ƿ�λ������
        /// </summary>
        public bool FindResult
        {
            get{return _findResult;}
            set{_findResult = value;}
        }

        /// <summary>
        /// ��ǰ��λ�Ŀ�ʼ��
        /// </summary>
        public int CurrentRow
        {
            get{ return _currentRow; }
            set { _currentRow = value; }
        }

        /// <summary>
        /// ��¼���鼶��ѭ����λ��������
        /// </summary>
        public int[] ArrGrade
        {
            get { return _arrGrade; }
            set { _arrGrade=value;}
        }
        /// <summary>
        /// ��ǰҳ�Ķ�λѭ�����
        /// </summary>
        public int LastDeep
        {
            get { return _lastDeep; }
            set { _lastDeep = value; }
        }

        /// <summary>
        /// �����鼶�Σ������ɴ��ж��Ƿ���ڷ���
        /// </summary>
        public int GroupLevel
        {
            get { return _groupLevel; }
            set { _groupLevel = value; }
        }

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int CurrentLevel
        {
            get { return _currentLevel; }
            set { _currentLevel = value; }
        }

        /// <summary>
        /// ��������
        /// </summary>
        public ArrayList TopGroupItem
        {
            get { return _TopGroupItem; }
            set { _TopGroupItem = value; }
        }

        /// <summary>
        /// �Ƿ���ʾ��ϸ
        /// </summary>
        public bool ShowDetail
        {
            get { return _showDetail; }
            set { _showDetail = value; }
        }

        /// <summary>
        /// �Ƿ����ҳ
        /// </summary>
        public bool PageByGroup
        {
            get { return _bPageByGroup; }
            set { _bPageByGroup = value; }
        }

        /// <summary>
        /// ƥ���ʽ��
        /// </summary>
        public string Pattern
        {
            get { return _pattern;}
            set { _pattern=value;}
        }

        /// <summary>
        /// ���ݿ�ƥ��sql��
        /// </summary>
        /// <returns></returns>
        public string DbMatchSql
        {
            get { return _dbMatchSql;}
            set { _dbMatchSql=value;}
        }

        /// <summary>
        /// ���¶�λ
        /// </summary>
        public void ReLocation()
        {
            _bFirstLocation = true;
            _lastDeep = 0;
        }

        /// <summary>
        /// ������Ŀ����
        /// </summary>
        public int SortItemsCount
        {
            get { return _sortItemsCount; }
            set { _sortItemsCount = value; }
        }
    }
}
