# FakeDataLoader

A dotnet console app which can populate a collection with random data using a sample file

## Settings

* ConnectionString, DatabaseName, CollectionName: the server/db/collection to act upon
* ModelSampleFileName: a json/bson file that will be used as a sample file to build the document. I just pulled a document out of CosmosDB in int to start with (and removed the _id field)
* NumberToGenerate: the number of documents to write

## Supported field types and random values

A subset of valid JSON/BSON types are supported.
The sample file obtains a 'sample size' for certain types that it uses to define the range of the random value

* String - if 'x' is the length of the string in the sample file, generate a random string of length x/2 - x*2. e.g. if x = 8, length is random from 4-16
* Number - if 'x' is the value of the number, generate a random un-signed int in the range of x/2 - x*2. e.g. if x = 10000, value is random from 5000-20000 
* Date - generates a random recent date (note: there is no JSON Date type, so Date is inferred if it's an 'Object' type with a single field called '$date')
* Boolean - generates a random bool
* Null - is null

## To do

Add a capability to seed certain fields given a well known set of values. e.g. if I want to use a specific range of values for 'accountNumber', that can be defined in configuration.