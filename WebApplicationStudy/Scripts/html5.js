function check() {
    text = document.getElementById("text1");
    var value = text.value;
    if (value == "" || isNaN(value)) {
        alert("Not Numeric");
    }
    else if(value<5){
        alert("Too Small");
    }
    else if(value>10){
        alert("Too Big");
    }
}
function check1() {
    try{
        text = document.getElementById("text11");
        var value = text.value;

        if (value == "" || isNaN(value)) {
            throw "为空或者不是数字！"
        }
        else if (value < 5) {
            throw ("Too Small");
        }
        else if (value > 10) {
            throw ("Too Big");
        }
    }
    catch(err)
    {
        alert(err);
    }
}