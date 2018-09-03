using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	// ԭ����whereΪ������û�ж��������ʱ��������where��
	// ����ĳ�������С�Ĭ��Ҫ�ֽ��SQL�ĸ�ʽ������ȷ��.
	public class DispartSqlSelect
	{
		private string		_Sql		= string.Empty;
		private string		_SqlBackup	= string.Empty;

		private int		_WhereIndex		= -1;
		private int		_OrderByIndex	= -1;
		private int		_GroupByIndex	= -1;
		private int		_HavingIndex	= -1;

		#region Constructor

		public DispartSqlSelect(){}

		public DispartSqlSelect( string sql )
		{
			_Sql = sql;
		}

		#endregion

		#region Property

		public string Sql
		{
			get { return _Sql; }
		}

		public string SelectPart
		{
			get { return GetSubSting( 0, _WhereIndex ); }
		}

		public string WherePart
		{
			get { return GetSubSting( _WhereIndex, _GroupByIndex ); }
		}

		public string GroupByPart
		{
			get { return GetSubSting( _GroupByIndex, _HavingIndex ); }
		}

		public string HavingPart
		{
			get { return GetSubSting( _HavingIndex, _OrderByIndex ); }
		}

		public string OrderByPart
		{
			get { return GetSubSting( _OrderByIndex, _Sql.Length ); }
		}

		#endregion

		#region Public Method

		public void Dispart()
		{
			if( _Sql == string.Empty )
				return;

			ClareDirty();
			
			FindKeyWordIndex( "Order By",	ref _OrderByIndex,	this._Sql.Length );
			FindKeyWordIndex( "Having",		ref _HavingIndex,	_OrderByIndex );
			FindKeyWordIndex( "Group By",	ref _GroupByIndex,	_HavingIndex );
			FindKeyWordIndex( "Where",		ref _WhereIndex,	_GroupByIndex );
		}

		public void Dispart( string sql )
		{
			_Sql = sql;
			Dispart();
		}

		#endregion

		#region Private Method

		// 1.ȥ������(sql)���˵�����
		// 2.���������ڵĿո񶼺ϳ�һ���ո�
		private void ClareDirty()
		{
			_Sql = _Sql.Trim();
			int selectStart = _Sql.IndexOf( "select", StringComparison.OrdinalIgnoreCase );
			if( selectStart > 0 )
			{ 
				Stack leftBrachet = new Stack();
				for( int i = 0; i < selectStart; i++ )
				{
					if( _Sql[i] == '(' || _Sql[i] == '��' )
					{
						leftBrachet.Push( '(' );
						_Sql = _Sql.Remove( i, 1 );
						i--;
					}
				}

				for( int i = _Sql.Length -1;; i-- )
				{
					if( _Sql[i] == ')' || _Sql[i] == '��' )
					{
						leftBrachet.Pop();
						_Sql = _Sql.Remove( i, 1 );
					}
					
					if( leftBrachet.Count == 0 )
						break;
				}
			}

			selectStart = 0;
			while( selectStart != -1 )
			{
			    _Sql = _Sql.Replace( "  ", " " );
			    selectStart = _Sql.IndexOf( "  " );
			}
		}

		private void FindKeyWordIndex( 
			string key,
			ref int keyIndex,
			int indexWhenNotFound )
		{
			int selectStart = 0;
			keyIndex = indexWhenNotFound;
			while( selectStart != -1 )
			{
				selectStart = _Sql.IndexOf( key, selectStart + key.Length, StringComparison.OrdinalIgnoreCase );
				if( !IsInBraket( selectStart ) )
				{
					keyIndex = selectStart;
					break;
				}
			}
		}

		// �ж��Ƿ���ĳ��С�����С���ĩβ��λ��index
		// ��飬������ڲ��ԳƵ�С���ţ��򷵻���
		private bool IsInBraket( int index )
		{ 
			if( index == -1 )
				return true;

			Stack leftBrachet = new Stack();
			for( int i = _Sql.Length -1; i >= index; i-- )
			{
				if( _Sql[i] == ')' || _Sql[i] == '��' )
					leftBrachet.Push( ')' );
				else if( _Sql[i] == '(' || _Sql[i] == '��' )
					leftBrachet.Pop();
			}

			if( leftBrachet.Count > 0 )
				return true;

			return false;
		}
			
		private string GetSubSting( 
			int startIndex,
			int endIndex )
		{
			if( _Sql != string.Empty
				&& startIndex >= 0
				&& endIndex >= startIndex )
			{
				string subString = _Sql.Substring( startIndex, endIndex - startIndex );
				if( subString != string.Empty )
					return subString.Trim();
			}

			return string.Empty;
		}

		#endregion
	}
}