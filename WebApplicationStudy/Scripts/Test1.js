
var person = { fname: "ma", sname: "tengfei", age: "23" };
var cars = new Array("a1", "b1", "c1", "d1");
function test() {
    cars = ["aa1", "bb1", "cc1", "dd1"];
    var i = 1;
    for(;i<cars.length;)
    {
        document.write("car: " + cars[i].toString() + " Coming!" + "<br>");
        i++;
    }
}
function test1() {
    var txt="";
    for (x in person)
    {
        txt = txt + person[x]+"<br>";
    }
    document.write(txt);
}
function test2() {
    var txt = "",i=0;
    while (i < 5)
    {

        txt = txt + i + "<br>";
        i++;
    }
    document.write(txt);
}
function test3(){
    var i=0,txt="";
    do{
        txt += i.toString();
        i++;
    }
    while (i < 5);
    document.write(txt);
}
function test4() {
    list: {
        document.write(cars[0] + "<br>")
        document.write(cars[1] + "<br>")
        document.write(cars[2] + "<br>")
        break list;
        document.write(cars[3] + "<br>")
    }
}