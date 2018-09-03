<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="NhibernateDemo._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<link rel="stylesheet" type="text/css" href="css/base.css" />
	<link rel="stylesheet" type="text/css" href="css/style.css" />
	<script type="text/javascript" language="javascript" src="js/jquery.js"></script>
	<script type="text/javascript" language="javascript" src="js/jquery.tableui.js"></script>
	<script type="text/javascript">
	    $().ready(function() {
	        $(".table_solid").tableUI();
	    })
	</script>
</head>
<body>
<form id="form1" runat="server">
<div id="wrapper">
	<div id="header">
		<div id="top_info">
		<h1 class="fl">管理系统后台</h1>
		<div class="fr">
			<strong class="red" id="import_notice">重要信息通知区域</strong>
			<span>欢迎你，<span>FlyDragon</span> | <a href="Default.aspx" title="">退出</a></span>
		</div>
		<div class="clear"></div>
		</div><!--end: top_info -->

		<div id="menu">
			<div class="tabArea fullwidth">
				<ul class="tabList">
					<li><a href="Default.aspx">后台首页</a></li>
					<li><a href="Default.aspx">产品管理</a></li>
					<li><a class="current" href="Default.aspx">用户管理</a></li>
					<li><a href="Default.aspx">新闻管理</a></li>
				</ul>
			<div class="clear"></div>
			</div>
			<div id="menu_level2"> 
				<ul id="menu_level2_list">
					<li><a class="current" href="Default.aspx">用户管理</a></li>
					<li><a href="Default.aspx">产品管理</a></li>
					<li><a href="Default.aspx">新闻管理</a></li>
				</ul>			
			</div>
		</div><!--end: menu -->
	</div><!--end: header -->

	<div id="container">
		<div class="block"> 
			<div class="fl"> 
				<button>新建用户</button>
			</div>
			<div class="fr" id="search"> 
			<input type="text" id="txtSearch" />
			<input type="button" id="btnSearch" value="搜索" />
			</div>
			<div class="clear"></div>
		</div><!--end: block -->

		<div class="block"> 
		<table class="fullwidth table_solid">
		<tr>
			<th>用户名</th>
			<th width="150px" >上次登录日期</th>
			<th width="150px" class="txt_c">创建人</th>
			<th width="265px">真实姓名</th>
			<th width="175px">操作</th>
		</tr>

		<%for (int i = 0; i < cUsers.Count; i++) %>		
		
		<% {%>
		<tr>
		   <td><%=cUsers[i].Name %><a href='ShowUserInfo.aspx?id=<%=cUsers[i].Id %>' title="查看详情"><img src="images/btn_detail.gif" alt="查看详情"   align="absmiddle"/></a></td>
			<td class="txt_c bold"><span class="color1"><%=cUsers[i].LastTimeLogOn.Year %></span>-<span class="color2"><%=cUsers[i].LastTimeLogOn.Month %></span>-<span class="color3"><%=cUsers[i].LastTimeLogOn.Day %></span> </td>
			<td class="txt_c"><%=cUsers[i].Creator %></td>
			<td><%=cUsers[i].NickName %></td>
			<td>
				<a class="fl btn1" href='ShowUserInfo.aspx?id=<%=cUsers[i].Id %>'>添加任务</a>
				<a class="fl btn2" href='ShowUserInfo.aspx?id=<%=cUsers[i].Id %>'>编辑</a>
				<a class="fr btn3" href='ShowUserInfo.aspx?id=<%=cUsers[i].Id %>'>删除</a>
			</td>
			</tr>
		<%} %>
        
		</table>
		</div><!--end: block -->

		<div class="block"> 
		<div class="fl"> 
		<button >显示全部记录</button><span class="gray">（默认显示最新操作的12条记录。共：999条记录）</span>
		</div>
		<div class="fr"> 
		<div class="page_list"><span class="current">1</span><a href="/new/page2/">2</a><a href="/new/page3/">3</a><a href="/new/page4/">4</a><a href="/new/page5/">5</a><a href="/new/page6/">6</a><a href="/new/page7/">7</a><a href="/new/page8/">8</a><a href="/new/page9/">9</a><a href="/new/page10/">10</a><a href="/new/page11/">11</a><a href="/new/page12/">12</a>...<a href="/new/page479/">479</a><a href="/new/page2/"> Next  &gt;</a></div>

		</div>
		<div class="clear"></div>
		</div>



	</div><!--end: container -->

	<div id="footer">
		<div id="friendLink">
		</div><!--end: friendLink -->
	</div><!--end: footer -->
</div><!--end: wrapper -->
</form>
</body>
</html>

