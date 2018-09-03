function Person(firstname, lastname, age) {
    this.firstname = firstname;
    this.lastname = lastname;
    this.age = age;
    this.changeName = changeName;
    this.changeFirstName = changeFirstName;
    function changeName(name) {
        this.lastname = name;
    }
    function changeFirstName(fname)
    {
        this.firstname = fname;
    }
}