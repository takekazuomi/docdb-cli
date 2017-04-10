==================================
Azure DcumentDB command line tools
==================================

Usage
=====

.. code-block:: posh

   .\docdb create -e END_POINT_URI -k ACCESS_KEY -d DATABASE_NAME -c COLLECTION_NAME -j JSON_FILE
   .\docdb query -e END_POINT_URI -k ACCESS_KEY -d DATABASE_NAME -c COLLECTION_NAME-q SQL_QUERY


Examples
========

1. Start DocuementDB Emulator
   Ref: `Use the Azure DocumentDB Emulator for development and testing <https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator>`_

2. Create Database name 'db' and Create Collection name 'test'

3. Create following json file 'f.json'

.. code-block:: posh

   $ cat .\f.json
   {
      "id": "KinmugiFamily"
   }

4. Create document in DocumentDB

.. code-block:: posh

   .\docdb create -e EndPointUri -k "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" -d db -c test -j f.json -v


5. Query document in DocumentDB

   .\docdb query -e "https://localhost:8081" -k "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" -d db -c test -q "select * from c" | jq .


Note
====
Current version support create docuemnt and query dcuments only.
