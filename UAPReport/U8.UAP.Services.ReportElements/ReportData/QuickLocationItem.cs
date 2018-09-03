using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public enum FindDirection
    {
        Up = 0,             //向上查找
        Down = 1,            //向下查找
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
        int _groupLevel = 0;            //共分组级次，可以由此判断是否存在分组
        int _currentLevel = 1;          //当前级次
        bool _showDetail = true;
        int[] _arrGrade = new int[100]; //记录分组级次循环定位的行索引
        int _lastDeep = 0;              //循环深度        
        string _pattern = "";           //匹配格式串
        string _dbMatchSql = "";        //数据库匹配sql串
        bool _bFirstLocation = true;    //是否首次定位    
        bool _bReStartRow = false;      //是否重新设定起始行
        int _sortItemsCount = 0;        //排序项目数量
        bool _bPageByGroup = false;     //是否按组分页
        ArrayList _TopGroupItem;        //顶级分组
        string _baseId = "baseid";       //baseid字段名

        /// <summary>
        /// 定位字段, 当FieldName="*"表示所有字段定位
        /// </summary>
        public string FieldName
        {
            get{ return _fieldName; }
            set{_fieldName = value; }
        }

        /// <summary>
        /// 定位单元格, 当CellName="*"表示所有字段定位
        /// </summary>
        public string CellName
        {
            get{return _cellName;}
            set { _cellName = value;}
        }

        /// <summary>
        /// baseid字段名
        /// </summary>
        public string BaseId
        {
            get { return _baseId; }
            set { _baseId= value; }
        }
        
        /// <summary>
        /// 是否定位当前页
        /// </summary>
        public bool CurrentPage
        {
            get{ return _bCurPage;}
            set{ _bCurPage = value; }
        }

        /// <summary>
        /// 是否首次定位   
        /// </summary>
        public bool FirstLocation
        {
            get { return _bFirstLocation; }
            set { _bFirstLocation=value;}
        }

        /// <summary>
        /// 是否重新设定起始行
        /// </summary>
        public bool ReStartRow
        {
            get { return _bReStartRow; }
            set { _bReStartRow = value; }
        }

        /// <summary>
        /// 定位方向
        /// </summary>
        public FindDirection Direction
        {
            get{return _direction; }
            set { _direction = value; }
        }       

        /// <summary>
        /// 是否定位到数据
        /// </summary>
        public bool FindResult
        {
            get{return _findResult;}
            set{_findResult = value;}
        }

        /// <summary>
        /// 当前定位的开始行
        /// </summary>
        public int CurrentRow
        {
            get{ return _currentRow; }
            set { _currentRow = value; }
        }

        /// <summary>
        /// 记录分组级次循环定位的行索引
        /// </summary>
        public int[] ArrGrade
        {
            get { return _arrGrade; }
            set { _arrGrade=value;}
        }
        /// <summary>
        /// 当前页的定位循环深度
        /// </summary>
        public int LastDeep
        {
            get { return _lastDeep; }
            set { _lastDeep = value; }
        }

        /// <summary>
        /// 共分组级次，可以由此判断是否存在分组
        /// </summary>
        public int GroupLevel
        {
            get { return _groupLevel; }
            set { _groupLevel = value; }
        }

        /// <summary>
        /// 当前级次
        /// </summary>
        public int CurrentLevel
        {
            get { return _currentLevel; }
            set { _currentLevel = value; }
        }

        /// <summary>
        /// 顶级分组
        /// </summary>
        public ArrayList TopGroupItem
        {
            get { return _TopGroupItem; }
            set { _TopGroupItem = value; }
        }

        /// <summary>
        /// 是否显示明细
        /// </summary>
        public bool ShowDetail
        {
            get { return _showDetail; }
            set { _showDetail = value; }
        }

        /// <summary>
        /// 是否按组分页
        /// </summary>
        public bool PageByGroup
        {
            get { return _bPageByGroup; }
            set { _bPageByGroup = value; }
        }

        /// <summary>
        /// 匹配格式串
        /// </summary>
        public string Pattern
        {
            get { return _pattern;}
            set { _pattern=value;}
        }

        /// <summary>
        /// 数据库匹配sql串
        /// </summary>
        /// <returns></returns>
        public string DbMatchSql
        {
            get { return _dbMatchSql;}
            set { _dbMatchSql=value;}
        }

        /// <summary>
        /// 重新定位
        /// </summary>
        public void ReLocation()
        {
            _bFirstLocation = true;
            _lastDeep = 0;
        }

        /// <summary>
        /// 排序项目数量
        /// </summary>
        public int SortItemsCount
        {
            get { return _sortItemsCount; }
            set { _sortItemsCount = value; }
        }
    }
}
