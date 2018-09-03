/*
这里是我的扩展js脚本，它执行了OpenCloseLight.html中的btn2的click事件
*/

var cars = new Array();
cars[0] = "Audio";
cars[1] = "BMW";
cars[2] = "volvo";
var buss = new Array("aa", "bb", "cc", "dd");
var abcs = ["ad", "ac", "ab"]
var person = {
    firstname: "ma",
    secondName: "tengfei"
}
var person1 = {
    firstname: "zhang",
    secondname: "fan",
}
var carname = new String;
person = new Object();
person.firstname = "Bill";
person.lastname = "Gates";
person.age = 56;
person.eyecolor = "blue";
person.familyname = "ma";
person.getfamilyname = function () {
    return person.familyname;
}
function btnClick1() {
    x = document.getElementById("txtNum");
    x.value = "我是被更改过的值了！";
    var aa = 7;
    var aa = "我是一个字符串变量，我来自myscript！我原来是一个数字"
    document.getElementById("p3").innerHTML = person.eyecolor;
    var carname = "i'm little bird!"
    document.getElementById("txtNum").value = carname.toUpperCase();
    carname = sayHello(carname, x.value);
    alert(carname);
}
function sayHello(a, b) {
    return ("Hello,Welcome" + a + " and " + b);
}
function myFunction(a, b) {
    a = parseInt(a);
    b = parseInt(b);
    if (a > b) {
        alert("a>b")
        return;
    }
    x = a + b
    alert(x.toString())
}
function upperCase() {
    var phone = document.getElementById('phone').value;
    var pat = /^1[34578]\d{9}$/
    if (!(pat.test(phone))) {
        alert("手机号码有误，请重填");
        return false;
    }
}
function GetDay() {
    var day = new Date().getDay();
    switch (day) {
        case 0:
            x = "Today it's Sunday";
            break;
        case 1:
            x = "Today it's Monday";
            break;
        case 2:
            x = "Today it's Tuesday";
            break;
        case 3:
            x = "Today it's Wednesday";
            break;
        case 4:
            x = "Today it's Thursday";
            break;
        case 5:
            x = "Today it's Friday";
            break;
        case 6:
            x = "Today it's Saturday";
            break;
    }
    alert(x);
    var y = TestFor();
}
function TestFor() {
    var x = "Begin";
    for (var i = 0; i < 5; i++) {
        x = x + i.toString() + "<br>";
    }
    return x;
}
function goBack() {
    window.history.back()
}
function goForward() {
    history.forward();
}