# SmallestEditPathAlgorithm
An algorithm that expands upon the paper "An O(ND) Difference Algorithm and its Variations", Eugene W. Meyers, 'Algorithmica' Vol. 1 No 2, 1986, pp. 251-266

# Introduction
In the Spring of 2018, I was in the process of refactoring a large piece of spaghetti code that I had written over the course of six months. It was around six to eight thousand lines long, and was becoming a huge burden to maintain. After researching a couple of javascript frameworks (react and redux), and reading a lot about the architecture of large-scale projects, I decided I'd structure the class dependencies so that 

- All the business objects contained only data and would represent an immutable state
- The business logic would only know how to manipulate the state
- The services coordinated on starting at one state, and calling the correct pieces of business logic to represent a transfer to another state
- The frond-end would create a snapshot of the current state, pass it into the services, and get a new state, which it would then represent

These choices were my way of bringing Domain-Driven Design into my application, and I used mostly static methods for the business logic, based off of what I had seen in Redux, and the pure functional programming. I got about 95% test coverage of the business logic and business objects.

However, I was using Excel as a front-end, and it doesn't have any mature frameworks that give the same capabilities as something like React, so I have to replicate some of functionality so that I could efficiently represent state transitions to the user.

Coming back full circle, this algorithm was what I wrote to make sure I was making as few edits as possible to the front-end. Whenever I wanted to efficiently move around objects in a list, I decided to use the ids associated with those objects to decide which ones needed to be deleted when the state was changed, which ones needed to be added, and which ones needed to be moved.

This is where the function PerformShortestEditSequence comes into play. The first two arguments are a list of the ids before and after, the next three arguments are the functions to call when an item from the new list is inserted somewhere into the old, deleted from the old, or moved from a position in the old list to the new one.

# Description
Based off the description in Meyer's paper, I treat each snake as a node, and build a sorted tree of snakes which I use to decide which edits to make. In order to create a queue of snakes, use CreateShortestEditQueue.

ExecuteEditTree then takes a queue of those nodes, and calls the proper insert/delete/move functions that should update the list of objects that the list of ids represent. This could be lines in a database, but it could also be parts of a user interface.

PerformShortestEditSequence just takes the list of old and new ids, as well as the insert, delete, and move functions, and just chains CreateShortestEditQueue and ExecuteEditTree together.