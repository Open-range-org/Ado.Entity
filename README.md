# Ado.Entity

  Ado.Entity is an object-relational mapping framework for .NET applications. Object-relational mapping allows the use of database queries and operations with object-oriented programming languages.
  It has similarty with Entity framework But you must agree that , it is a lot simpler than Entity framework . If you don;t know about The entity , you can have a look here . Even if you are familier with Entity and trying to avoid complex  `Code First, Model First, and Database First` approaches , you can try this .

You need to import Ado.Entity Namespace'
```cs
  using Ado.Entity;
```
After Improting the namespace we need to create one POCO class which will hve same name as a table . That POCO need to inharite the class **`AdoBase`** . That class should contain properties Where Property names and Column names of the Table should be same . For Example, if we have a table called **Users** and it's having 3 columns `"Id(int)","Name(varchar(20))","Age(int)","IsAdmin(bit)","DOB(datetime)"` then the mapping poco class is  

```cs
    class Users:AdoBase
    {
        [Primary]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public int Age { get; set; }
        public DateTime DOB { get; set; }
    }
```
Note : We can use attribute like , Primary , Unique  for a property . One class can have only one property tith attribute primary . More then one primary attribute will throw error(runtime).

## Get data from table
We need to follow very simple steps to get data from database . First we need to create a instance of `Connection` class where we need to pass connection string .  `GetDataByQuery` helps to get data from table . `GetDataByQuery` expects sql query , which we need to pass when calling this method . 



```cs
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            var data=connection.GetDataByQuery<Users>("select * from Users");
        }
```

## Update row(s) of a table
To update a row of the table Users first we need to create a instance of `Connection` class where we need to pass connection string .  `UpdateEntry` helps to update data of a row . we need to pass an object of class "Users" in `UpdateEntry` . 
 Updating multiple rows also possible here . 

```cs
//single row update
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            //data is a object of class Users .
            connection.UpdateEntry<Users>(data);
        }
//multiple row update
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            //dataList is List<Users>  .
            connection.UpdateEntry<Users>(dataList);
        }
```
## Insert row(s) in a table
To Insert a row of the table Users first we need to create a instance of `Connection` class where we need to pass connection string .  `AddEntry` helps to insert a row . we need to pass an object of class "Users" in `AddEntry` . 
 Adding multiple rows also possible here . 

```cs
//single row Add
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            //data is a object of class Users .
            connection.AddEntry<Users>(data);
        }
//multiple row Add
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            //dataList is List<Users>  .
            connection.AddEntry<Users>(dataList);
        }
```

So , The concept is not new right ? Yes I know. But still if you want a light weight object-relational mapping library try this .

https://www.nuget.org/packages/Ado.Entity

