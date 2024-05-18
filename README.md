## Core api template
Core api template is designed to be a quick way to create a new api project that has a reasonable separation of data and logic layers without any of the hassle. If you're looking to explore specific patterns, designs or implementations this may provide a core system for you to work off instead of having to create a new project from scratch and lose interest by the time you've reached the exploration state.

It's a work in progress so expect to see things change as it's improved and optimized.

There's a few limitations to be aware of:
- There is alot of reflection in the project `BaseSqlDataContext` & `DataServiceInjection` being the biggest examples. `BaseSqlDataContext` uses reflection to construct sql queries from classes and must data queries will end up using it so keep that in mind.
- Where data is to be persisted, there needs to be a model and a data class which is okay for the project's intended use but may get cumbersome over time.
- Alot of services are singleton given it's only ever going to be local development


## Persistence
The idea of this is to have an easy way to communicate with a database without having the write the sql directly and have a degree of separation for unit testing without having to do much work. I've tried to keep the 

`BaseSqlDataContext` implements all members of `IDataContext` so inheriting this into a data class should give you core SQL functionality. The idea is that in complicated data models the base implementations can be overridden to meet the requirements but most of the time `BaseSqlDataContext` will have you covered and creating a new model is about the same effort as using EF but without the complications.

`DataContextFactory` will provide you with the relevant IDataContext implementation or you could just inject the implementations you want. This works alongside `DataServiceInjection` which will use reflection to find all of the `IDataContext` implementations. Again this is really to save time and make creating a new model as easy as possible.
