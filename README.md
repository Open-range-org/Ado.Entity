# Ado.Entity

Ado.Entity is an object-relational mapping framework for .NET applications. Object-relational mapping allows the use of database queries and operations with object-oriented programming languages.

```cs
class DbConnection
    {
        string connectionString = "Data Source=.;Initial Catalog=ur db;Integrated Security=True";
        public DbConnection()
        {
            IConnection connection = new Connection(connectionString);
            var data=connection.GetDataByQuery<Users>("select * from Users");
            data.ForEach(d =>
            {
                d.DOB = DateTime.Now;
                connection.UpdateEntry<Users>(d);
            });
        }
    }
```
