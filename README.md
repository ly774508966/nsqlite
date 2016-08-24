════════════════════════════════════════════════

                NSqlite		          
        Copyright © 2015 NPlugins      
              Version 1.0.1                       
        nplugins.unity@gmail.com   

════════════════════════════════════════════════

NSqlite is an extremely simple library for writing and retrieving data from local Sqlite database. It’s differential resides in the fact that it requires no query writing for the developer and supports many types of variables.

It takes as parameters, for inserting to the database, custom models, created by the developer, and, retrieve the data from the database with instances of this same models. 

As a limitation, NSqlite cannot handle some Unity specific class (GameObject, Transform, Material, etc), and requires that the developer’s models extends from it’s base class, “Storable”. Also, in this first version, any changes in the models may require the developer to recreate the correspondent Sqlite table. 

╔═════╗

║Usage║

╚═════╝

NSqlite is really straightforward to use. It provides four kind of interactions:

* Insert
  - There are two different Insert methods. One takes only one instance of a class, and, the other, a whole list. Both of them make a simple insertion of the data received into the database.

* Update
  - Update works similarly to the Insert. However, you can only make updates in data you have already retrieved from the database. Do not try to update a new instance of a class that you have not had access to by other means than the Find method.

* Find
  - There are many different ways to retrieve your data from the database. They can provide either a single instance of a model or a list of instances. For more information on these methods, you can check the documentation (see link below). 

* Delete
  - Allows you to delete single entries in the database, within a criteria, or the whole table.

╔═══════╗

║Samples║

╚═══════╝

Samples can be found inside sample folder.

