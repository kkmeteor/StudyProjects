<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplicationStudy.WebForm1" %>

<!DOCTYPE html>


<html>

<body style="background-color: #b6ff00; text-align: center;">
    <h2>Hello RUNOOB.COM!</h2>
    <script type="text/javascript">
        var a = "<%=ss()%>"
        var b = a + "hahaha"
        alert(a)
        function JsCallCSharp(param1) {
            //sayhell是后台标注了【webMethod】属性的方法 param1是传入该方法的参数，onSayHelloSucceeded是回调函数主要是对后台返回的结果进一步处理  
            PageMethods.sayhell(param1, onSayHelloSucceeded);
        }
        //绑定的回调函数   
        function onSayHelloSucceeded(result)
        {
            alert(result);
        }
    </script>
    <p><%Response.Write(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));%></p>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat ="server" EnablePageMethods="true"></asp:ScriptManager>
        <div>
        <input type="button" value="btn1" name="123123123" OnClick="JsCallCSharp(b)" />
        </div>
    </form>
</body>
</html>

