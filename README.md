# JumpKingGraph

Displays your Progress in the game JumpKing as a Graph which can be added to your Stream.

![Example](https://i.imgur.com/eq4yWCd.png)

## TODO

Need to find a deref chain for two things:

- The game stores the highest screen that you've reached in this session - gotta use that as the "Highest" instead of the executions highest
- The game stores whether you're standing or not - need to only start a new value on the x-axis when you're standing so that "Jump peeking" the next area doesnt add a new step on the graph
