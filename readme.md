I have implemented 90%-95% of the functionality requested, only the placeholder action on the index page was left to do, but that should just be simple scaffolding.

My next steps with this would have been:

To implement the provided Models in a DDD way as to abstract some of the logic that is present in the WarehouseService back into the construtors of the Models, for example input validation and the like.

Create clearer architectural boundaries between the project runner, and the two types of controllers, using the ports and adaptors/ clean architecture model, including DTO's for boundary models (this will help with a seperation of concerns and the possibility for a modular building process).

Move the Repository Interface within the Application layer as to create propper seperation of concerns, using the project runner to match up the moved interface with the repository within the DLL, so that the Repository Pattern is followed correctly, decreasing coupling between the application layer and the DLL.

I had a busy weekend, so had to squeeze this in where I could, if you have any questions about the architecture and patterns that I have used here please let me know, I LOVE talking about this stuff!

